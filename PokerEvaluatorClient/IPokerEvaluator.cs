using PokerEvaluator;

namespace PokerEvaluatorClient
{
    public interface IPokerEvaluator
    {
        EvaluationResult EvaluateBoard(string command);
    }
}