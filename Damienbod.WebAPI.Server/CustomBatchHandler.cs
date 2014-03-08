using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
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

        public ProtobufBatchHandler(HttpServer httpServer)
            : base(httpServer)
        {
            ExecutionOrder = BatchExecutionOrder.Sequential;
        }

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
               // throw ArgumentNullException("request");
            }

            ////////ValidateRequest(request);

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
               // TODO  throw Error.ArgumentNull("responses");
            }
            if (request == null)
            {
               // TODO throw Error.ArgumentNull("request");
            }

            MultipartContent batchContent = new MultipartContent(MultiPartContentSubtype);

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
                //throw Error.ArgumentNull("requests");
            }

            List<HttpResponseMessage> responses = new List<HttpResponseMessage>();

            try
            {
                switch (ExecutionOrder)
                {
                    case BatchExecutionOrder.Sequential:
                        foreach (HttpRequestMessage request in requests)
                        {
                            request.Headers.Add("Accept", "application/x-protobuf");
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
                // TODO throw Error.ArgumentNull("request");
            }

            List<HttpRequestMessage> requests = new List<HttpRequestMessage>();
            cancellationToken.ThrowIfCancellationRequested();
            MultipartStreamProvider streamProvider = await request.Content.ReadAsMultipartAsync();
            foreach (HttpContent httpContent in streamProvider.Contents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                HttpRequestMessage innerRequest = await httpContent.ReadAsHttpRequestMessageAsync();
                innerRequest.CopyBatchRequestProperties(request);
                requests.Add(innerRequest);
            }
            return requests;
        }

        private void ValidateRequest(HttpRequestMessage request)
        {
            // TODO throw new NotImplementedException();
        }
    }
}