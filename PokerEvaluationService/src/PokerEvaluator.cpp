
#include <include\PokerEvaluator.h>
#include <sstream>
#include <include\pokenum.h>
#include <include\inline\eval.h>

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
static std::string getHandCardOrder(int handType, HandVal handValue) {
	std::string cards;
	cards.clear();

	if (StdRules_nSigCards[handType] >= 1)
		cards += StdDeck_rankChars[HandVal_TOP_CARD(handValue)];
	if (StdRules_nSigCards[handType] >= 2)
		cards += StdDeck_rankChars[HandVal_SECOND_CARD(handValue)];
	if (StdRules_nSigCards[handType] >= 3)
		cards += StdDeck_rankChars[HandVal_THIRD_CARD(handValue)];
	if (StdRules_nSigCards[handType] >= 4)
		cards += StdDeck_rankChars[HandVal_FOURTH_CARD(handValue)];
	if (StdRules_nSigCards[handType] >= 5)
		cards += StdDeck_rankChars[HandVal_FIFTH_CARD(handValue)];

	return cards;
}

EvaluationResult PokerEvaluator::Evaluate(int paramsCount, char** parsedParams)
{
	EvaluationResult evaluationResult;
	enum_result_t enumerationResult;
	StdDeck_CardMask *playerCards = new StdDeck_CardMask [ENUM_MAXPLAYERS];
	StdDeck_CardMask boardCards;
	int boardCardCount = 0;

	/** 
	* StdDeck_StdRules_EVAL_TYPE : gives the type of hand from the 
	* We need to get for each player its: HandVal handval
	* With that we can get the handtype as : HandVal_HANDTYPE(handval), which returns and integer that we can map.
	* StdRules_handTypeNames[htype] , will give us a string....
	* StdRules_nSigCards[htype], will give us how many cards are used for that htype, a number from 1 to 5
	* Then with that number we can programatically get the card Ranks with:
	* HandVal_TOP_CARD(handval); HandVal_SECOND_CARD(handval); HandVal_THIRD_CARD(handval) .. etc. 
	*/

	int isError = pokenum(paramsCount, parsedParams, &enumerationResult, &boardCards, &playerCards, boardCardCount);

	for (int i = 0; i < enumerationResult.nplayers; i++){
		pokerEvaluator::EvaluationResult_PlayerEvaluationResult* playerEvalResult = evaluationResult.add_results();
		
		StdDeck_CardMask playerHand; 
		Deck_CardMask_XOR(playerHand, playerCards[i], boardCards); // XOR combines the pocket cards with the board cards.
	
		HandVal playerHandValue = StdDeck_StdRules_EVAL_N(playerHand, boardCardCount + 2);
		int handType = HandVal_HANDTYPE(playerHandValue); // handType matches the enum value for HandType below:

		playerEvalResult->set_handtype(static_cast<EvaluationResult_PlayerEvaluationResult_HandType>(handType));

		playerEvalResult->set_cardorder(getHandCardOrder(handType, playerHandValue));
		
		playerEvalResult->set_equityvalue(enumerationResult.ev[i] / enumerationResult.nsamples);
		playerEvalResult->set_winprobability(100 * enumerationResult.nwinhi[i] / enumerationResult.nsamples);

	}

	return evaluationResult;
}
