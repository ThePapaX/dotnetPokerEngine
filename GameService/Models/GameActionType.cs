namespace GameService.Models
{
    public enum GameActionType
    {
        // Game / Dealer actions:
        PlayerJoined = 100,
        PlayerLeft,
        PlayerTimedOut,
        UpdateDealerButton,
        CollectSmallBlind,
        CollectBigBlind,
        StartBettingRound,
        NoMoreBets,
        DealCard,
        DealEncryptedCard,
        DealFlop,
        DealTurn,
        DealRiver,
        SplitPot,
        AwardPot,
        NewHand
    }
}