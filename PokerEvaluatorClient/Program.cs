using Grpc.Core;
using PokerEvaluator;
using System;
using System.Text.Json;

namespace PokerEvaluatorClient
{
    class Program
    {
        static void PrintResult(EvaluationResult result)
        {
            Console.WriteLine("----- RESULT -----");
            Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions() {WriteIndented = true }));
            Console.WriteLine("--------------");
            Console.Write("\n\n command: ");
        }
        static void Main(string[] args)
        {
            var evaluatorClient = new EvaluatorGrpcClient("127.0.0.1:50051", ChannelCredentials.Insecure);

            var command = "pokenum -h As Ac - 2h 9d - Ks Kd -- 5s 4h 3h 7c 4c";

            EvaluationResult result = evaluatorClient.EvaluateBoard(command);

            PrintResult(result);

            while ((command = Console.ReadLine()) != "exit")
            {                
                result = evaluatorClient.EvaluateBoard(command);
                PrintResult(result);
            }
        }
    }
}
