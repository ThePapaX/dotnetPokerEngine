
#include <include\PokerEvaluator.h>
#include <sstream>
#include <include\pokenum.h>

using pokerEvaluator::EvaluationResult;
using pokerEvaluator::EvaluationResult_PlayerEvaluationResult_HandType;

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

EvaluationResult PokerEvaluator::Evaluate(int paramsCount, char** parsedParams)
{
	EvaluationResult evaluationResult;
	enum_result_t enumerationResult;

	// TODO: agregate results from each hand evaluation.

	// StdDeck_StdRules_EVAL_TYPE : gives the type of hand from the 
	// We need to get for each player its: HandVal handval
	// With that we can get the handtype as : HandVal_HANDTYPE(handval), which returns and integer that we can map
	// StdRules_handTypeNames[htype] , will give us a string....
	// StdRules_nSigCards[htype], will give us how many cards are used for that htype, a number from 1 to 5
	// Then with that number we can programatically get the card Ranks with:
	// HandVal_TOP_CARD(handval); HandVal_SECOND_CARD(handval); HandVal_THIRD_CARD(handval) .. etc.
	// StdDeck_rankChars[HandVal_TOP_CARD(handval)]); -> this wil give us the rank as a char : { '2', 'K', 'Q'... etc } 
	
	int isError = pokenum(paramsCount, parsedParams, &enumerationResult);
	std::cout << std::endl;

	for (int i = 0; i < enumerationResult.nplayers; i++){
		pokerEvaluator::EvaluationResult_PlayerEvaluationResult* playerEvalresult = evaluationResult.add_results();

		playerEvalresult->add_cardorder(3);
		playerEvalresult->add_cardorder(2);

		playerEvalresult->set_equityvalue(enumerationResult.ev[i] / enumerationResult.nsamples);
		playerEvalresult->set_winprobability(100 * enumerationResult.nwinhi[i] / enumerationResult.nsamples);

		playerEvalresult->set_handtype(EvaluationResult_PlayerEvaluationResult_HandType::EvaluationResult_PlayerEvaluationResult_HandType_FullHouse);

		printf(" %8.6f", enumerationResult.ev[i] / enumerationResult.nsamples);
		
	}
	std::cout << std::endl;

	return evaluationResult;
}
