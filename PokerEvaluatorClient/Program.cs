using Grpc.Core;
using PokerEvaluator;
using System;

namespace PokerEvaluatorClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

            var client = new Evaluator.EvaluatorClient(channel);
            String command = "pokenum -h As Ac - 2h 9d - Ks Kd -- 5s 4h 3h 7c 4c";

            EvaluationResult result = client.Evaluate(new EvaluationRequest { Command = command });

            Console.WriteLine("Result: " + result.Result);

            channel.ShutdownAsync().Wait();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
