namespace GameService.Models
{
    public enum PlayerActionType
    {
        PostSmallBlind = 1,
        PostBigBlind,
        Call,
        Raise,
        Fold,
        Check,
        ShowCards,
        MockCards,
        LeftGame
    }
}