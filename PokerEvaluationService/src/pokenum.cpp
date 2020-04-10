#include "include/pokenum.h"

int pokenum(int argc, char** argv, enum_result_t* result, StdDeck_CardMask *board, StdDeck_CardMask **pocketCards)
{
	enum_game_t game;
	enum_sample_t enumType;
	int niter = 0, npockets, nboard, err, terse, orderflag;
	StdDeck_CardMask deadCards;

	err = 0;
	enumResultClear(result);

	// parseArgs returns 0 when it is OK! 
	bool errorParsing = parseArgs(argc, argv, &game, &enumType, &niter,
		*pocketCards, board, &deadCards, &npockets, &nboard,
		&orderflag, &terse);

	if (!errorParsing) {
		if (enumType == ENUM_EXHAUSTIVE) {
			err = enumExhaustive(game, *pocketCards, *board, deadCards, npockets, nboard,
				orderflag, result);
		}
		else if (enumType == ENUM_SAMPLE) {
			err = enumSample(game, *pocketCards, *board, deadCards, npockets, nboard, niter,
				orderflag, result);
		}
		else {
			err = 1; // ENUMERATION TYPE NOT SUPPORTED
		}

		printf("--------Terse Result--------\n");
		enumResultPrintTerse(result, *pocketCards, *board);
		printf("\n--------Details--------\n");
		enumResultPrint(result, *pocketCards, *board);
	}

	return err;
}


