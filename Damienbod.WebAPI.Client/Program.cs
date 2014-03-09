using System;

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
