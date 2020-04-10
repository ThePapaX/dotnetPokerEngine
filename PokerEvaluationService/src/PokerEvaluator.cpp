
#include <include\PokerEvaluator.h>
#include <sstream>
#include <include\pokenum.h>

char** PokerEvaluator::makeargs(std::string input, int* argumentCount)
{
	std::vector<std::string> params;
	*argumentCount = 0;
	std::istringstream iss(input);

	for (std::string param; iss >> param; ) {
		params.push_back(param);
	}

	char** argumentVector = new char* [params.size()];
	*argumentCount = params.size();

	for (size_t i = 0; i < params.size(); i++)
	{
		int tokenLength = params[i].length() + 1;
		char* newToken = new char[tokenLength];

		strcpy_s(newToken, tokenLength, params[i].c_str());

		argumentVector[i] = newToken;
	}

	return argumentVector;
}

int PokerEvaluator::Evaluate(int paramsCount, char** parsedParams, enum_result_t* result)
{
	// TODO: agregate results from each hand evaluation.

	return pokenum(paramsCount, parsedParams, result);
}
