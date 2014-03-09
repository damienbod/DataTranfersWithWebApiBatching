using System;
using System.Net;
using System.Net.Http;
using Damienbod.Common;
using WebApiContrib.Formatting;

namespace Damienbod.WebAPI.Client
{
    public class ProtobufBatch
    {
        public static void DoRequest()
        {
            const string baseAddress = "http://localhost:55722";
            var client = new HttpClient();
            var batchRequest = new HttpRequestMessage(HttpMethod.Post, baseAddress + "/api/$batch")
            {
                Content = new MultipartContent("protobuf")
                {
                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Post, baseAddress + "/api/values")
                    {
                        Content = new ObjectContent<ProtobufModelDto>(new ProtobufModelDto() { Id = 56, Name = "ClientObjectToCreate", StringValue = "Protobuf object sent in a batch" }, new ProtoBufFormatter())
                    }),

                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, baseAddress + "/api/values/2")),

                     new HttpMessageContent(new HttpRequestMessage(HttpMethod.Put, baseAddress + "/api/values/7")
                    {
                        Content = new ObjectContent<ProtobufModelDto>(new ProtobufModelDto() { Id = 56, Name = "ClientObjectToCreate", StringValue = "Protobuf object sent in a batch" }, new ProtoBufFormatter())
                    }),

                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Delete, baseAddress + "/api/values/3")),
                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, baseAddress + "/api/values/4"))
                }
            };

            HttpResponseMessage batchResponse = client.SendAsync(batchRequest).Result;

            MultipartStreamProvider streamProvider = batchResponse.Content.ReadAsMultipartAsync().Result;
            foreach (var content in streamProvider.Contents)
            {
                HttpResponseMessage response = content.ReadAsHttpResponseMessageAsync().Result;
                if (response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        Console.WriteLine("delete, post or update ok");
                    }
                    else
                    {
                        // Parse the response body. Blocking!
                        var p = response.Content.ReadAsAsync<ProtobufModelDto>(new[] { new ProtoBufFormatter() }).Result;
                        Console.WriteLine("{0}\t{1};\t{2}", p.Name, p.StringValue, p.Id);
                    }
                    
                }
            }
        }
    }
}

