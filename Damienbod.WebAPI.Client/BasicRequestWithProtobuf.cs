using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Damienbod.Common;
using WebApiContrib.Formatting;

namespace Damienbod.WebAPI.Client
{
    public class BasicRequestWithProtobuf
    {
        public static void DoRequest()
        {
            var client = new HttpClient {BaseAddress = new Uri("http://localhost:55722")};
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-protobuf"));

            HttpResponseMessage response = client.GetAsync("api/Values/4").Result;

            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var p = response.Content.ReadAsAsync<ProtobufModelDto>(new[] {new ProtoBufFormatter()}).Result;
                Console.WriteLine("{0}\t{1};\t{2}", p.Name, p.StringValue, p.Id);
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int) response.StatusCode, response.ReasonPhrase);
            }
        }
    }
}
