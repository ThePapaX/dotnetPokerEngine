#pragma once
#include <string>
#include <vector>
#include <include\enumdefs.h>

class PokerEvaluator
{
	public: 
		char** makeargs(std::string input, int* argumentCount);
		//(int argc, char** argv, enum_result_t* result)
		int Evaluate(int paramsCount, char** parsedParams, enum_result_t* result);
};

