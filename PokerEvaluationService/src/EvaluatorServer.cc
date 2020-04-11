#include <iostream>
#include <memory>
#include <string>

#include <grpcpp/grpcpp.h>

#include "pokerEvaluator.grpc.pb.h"
#include <include\PokerEvaluator.h>
#include <chrono>

using namespace std::chrono;

using grpc::Server;
using grpc::ServerBuilder;
using grpc::ServerContext;
using grpc::Status;
using pokerEvaluator::EvaluationRequest;
using pokerEvaluator::EvaluationResult;
using pokerEvaluator::Evaluator;
using pokerEvaluator::EvaluationResult_PlayerEvaluationResult_HandType;

// Logic and data behind the server's behavior.
class PokerEvaluatorServiceImpl final : public Evaluator::Service {
	Status Evaluate(ServerContext* context, const EvaluationRequest* request,
		EvaluationResult* result) override {

		auto start = high_resolution_clock::now();

		PokerEvaluator Evaluator;

		int argsCount = 0;
		char** parsedArgs = Evaluator.makeargs(request->command(), &argsCount);

		*result = Evaluator.Evaluate(argsCount, parsedArgs);

		int duration = std::chrono::duration_cast<std::chrono::microseconds>(high_resolution_clock::now() - start).count();

		std::cout << "Evaluation completed in: " << duration << " microseconds." << std::endl;


		return Status::OK;
	}
};

void RunServer() {
	std::string server_address("0.0.0.0:50051");
	PokerEvaluatorServiceImpl service;

	ServerBuilder builder;
	// Listen on the given address without any authentication mechanism.
	builder.AddListeningPort(server_address, grpc::InsecureServerCredentials());
	// Register "service" as the instance through which we'll communicate with
	// clients. In this case it corresponds to an *synchronous* service.
	builder.RegisterService(&service);
	// Finally assemble the server.
	std::unique_ptr<Server> server(builder.BuildAndStart());
	std::cout << "Server listening on " << server_address << std::endl;

	// Wait for the server to shutdown. Note that some other thread must be
	// responsible for shutting down the server for this call to ever return.
	server->Wait();
}

int main(int argc, char** argv) {
	RunServer();

	return 0;
}
