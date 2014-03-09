using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Batch;

namespace Damienbod.WebAPI.Server
{
    public class ProtobufBatchHandler : HttpBatchHandler
    {
        private BatchExecutionOrder _executionOrder;
        private const string MultiPartContentSubtype = "mixed";
        private const string ApplicationProtobufSupportedContentType = "application/x-protobuf";

        public ProtobufBatchHandler(HttpServer httpServer) : base(httpServer)
        {
            ExecutionOrder = BatchExecutionOrder.Sequential;
            SupportedContentTypes = new List<string>{ ApplicationProtobufSupportedContentType, "multipart/protobuf" };
        }

         public IList<string> SupportedContentTypes { get; private set; }

        public BatchExecutionOrder ExecutionOrder
        {
            get
            {
                return _executionOrder;
            }
            set
            {
                if (!Enum.IsDefined(typeof (BatchExecutionOrder), value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof (BatchExecutionOrder));
                }
                _executionOrder = value;
            }
        }

        public override async Task<HttpResponseMessage> ProcessBatchAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
               throw new ArgumentNullException("request");
            }

            ValidateRequest(request);

            IList<HttpRequestMessage> subRequests = await ParseBatchRequestsAsync(request, cancellationToken);

            try
            {
                IList<HttpResponseMessage> responses = await ExecuteRequestMessagesAsync(subRequests, cancellationToken);
                return await CreateResponseMessageAsync(responses, request, cancellationToken);
            }
            finally
            {
                foreach (HttpRequestMessage subRequest in subRequests)
                {
                    request.RegisterForDispose(subRequest.GetResourcesForDisposal());
                    request.RegisterForDispose(subRequest);
                }
            }
        }

        public Task<HttpResponseMessage> CreateResponseMessageAsync(IList<HttpResponseMessage> responses, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (responses == null)
            {
               throw new ArgumentNullException("responses");
            }
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var batchContent = new MultipartContent(MultiPartContentSubtype);

            foreach (HttpResponseMessage batchResponse in responses)
            {
                batchContent.Add(new HttpMessageContent(batchResponse));
            }

            HttpResponseMessage response = request.CreateResponse();
            response.Content = batchContent;
            return Task.FromResult(response);
        }

        public async Task<IList<HttpResponseMessage>> ExecuteRequestMessagesAsync(
            IEnumerable<HttpRequestMessage> requests, CancellationToken cancellationToken)
        {
            if (requests == null)
            {
                throw new ArgumentNullException("requests");
            }

            var responses = new List<HttpResponseMessage>();

            try
            {
                switch (ExecutionOrder)
                {
                    case BatchExecutionOrder.Sequential:
                        foreach (HttpRequestMessage request in requests)
                        {
                            request.Headers.Add("Accept", ApplicationProtobufSupportedContentType);
                            responses.Add(await Invoker.SendAsync(request, cancellationToken));
                        }
                        break;

                    case BatchExecutionOrder.NonSequential:
                        responses.AddRange(
                            await
                                Task.WhenAll(requests.Select(request => Invoker.SendAsync(request, cancellationToken))));
                        break;
                }
            }
            catch
            {
                foreach (HttpResponseMessage response in responses)
                {
                    if (response != null)
                    {
                        response.Dispose();
                    }
                }
                throw;
            }

            return responses;
        }

        public async Task<IList<HttpRequestMessage>> ParseBatchRequestsAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var requests = new List<HttpRequestMessage>();
            cancellationToken.ThrowIfCancellationRequested();
            MultipartStreamProvider streamProvider = await request.Content.ReadAsMultipartAsync(cancellationToken);
            foreach (HttpContent httpContent in streamProvider.Contents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                HttpRequestMessage innerRequest = await httpContent.ReadAsHttpRequestMessageAsync(cancellationToken);
                innerRequest.CopyBatchRequestProperties(request);
                requests.Add(innerRequest);
            }
            return requests;
        }

        private void ValidateRequest(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.Content == null)
            {
                throw new HttpResponseException(request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, "BatchRequestMissingContent"));
            }

            MediaTypeHeaderValue contentType = request.Content.Headers.ContentType;
            if (contentType == null)
            {
                throw new HttpResponseException(request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,  "BatchContentTypeMissing"));
            }

            if (!SupportedContentTypes.Contains(contentType.MediaType, StringComparer.OrdinalIgnoreCase))
            {
                throw new HttpResponseException(request.CreateErrorResponse( HttpStatusCode.BadRequest, string.Format("BatchMediaTypeNotSupported")));
            }
        }
    }
}