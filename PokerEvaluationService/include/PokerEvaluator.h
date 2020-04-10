#pragma once
#include <pokerEvaluator.pb.h>

using pokerEvaluator::EvaluationResult;

class PokerEvaluator
{
	public: 
		char** makeargs(std::string input, int* argumentCount);
		EvaluationResult Evaluate(int paramsCount, char** parsedParams);
};

