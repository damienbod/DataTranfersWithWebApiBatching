﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
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
                Content = new MultipartContent("mixed")
                {
                    // POST http://localhost:55722/api/values
                    //new HttpMessageContent(new HttpRequestMessage(HttpMethod.Post, baseAddress + "/api/values")
                    //{
                    //    Content = new ObjectContent<string>("my value", new JsonMediaTypeFormatter())
                    //}),

                    // GET http://localhost:55722/api/values
                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, baseAddress + "/api/values/2")),

                    // GET http://localhost:55722/api/values
                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, baseAddress + "/api/values/3")),

                    // GET http://localhost:55722/api/values
                    new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, baseAddress + "/api/values/4"))
                }
            };

            HttpResponseMessage batchResponse = client.SendAsync(batchRequest).Result;

            MultipartStreamProvider streamProvider = batchResponse.Content.ReadAsMultipartAsync().Result;
            foreach (var content in streamProvider.Contents)
            {
                HttpResponseMessage response = content.ReadAsHttpResponseMessageAsync().Result;

                // Do something with the response messages
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body. Blocking!
                    var p = response.Content.ReadAsAsync<ProtobufModelDto>(new[] { new ProtoBufFormatter() }).Result;
                    Console.WriteLine("{0}\t{1};\t{2}", p.Name, p.StringValue, p.Id);
                }
            }
        }
    }
}

