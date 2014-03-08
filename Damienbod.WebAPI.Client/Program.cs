using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Damienbod.Common;
using WebApiContrib.Formatting;

namespace Damienbod.WebAPI.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ProtobufBatch.DoRequest();
            
            //DefaultBatchWithJsonAndDefaultBatchHandler.DoRequest();

            //BasicRequestWithProtobuf.DoRequest();

            Console.ReadLine();
        }
    }
}
