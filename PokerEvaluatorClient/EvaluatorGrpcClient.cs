using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using PokerEvaluator;

namespace PokerEvaluatorClient
{
    public class EvaluatorGrpcClient : IPokerEvaluator
    {
        private readonly Channel _channel;
        private readonly Evaluator.EvaluatorClient _gRpcClient;
        public EvaluatorGrpcClient(string connectionString = "127.0.0.1:50051", ChannelCredentials credentials = null)
        {
            if (credentials == null) credentials = ChannelCredentials.Insecure;

            _channel = new Channel(connectionString, credentials);
            _gRpcClient = new Evaluator.EvaluatorClient(_channel);

        }
        ~EvaluatorGrpcClient()
        {
            _channel.ShutdownAsync().Wait();
        }

        public EvaluationResult EvaluateBoard(string command) => _gRpcClient.Evaluate(new EvaluationRequest { Command = command });
    }
}
