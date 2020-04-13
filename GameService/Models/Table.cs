using System;
using System.Collections.Generic;


namespace GameService.Models
{
    public class Table
    {
        public int Id { get; set; }
        public LinkedList<GamePlayer> Players { get; private set; }
        public TableConfiguration TableConfig { get; private set; }

        public Dictionary<string, LinkedListNode<GamePlayer>> PlayersMap { get; private set; }

        public LinkedListNode<GamePlayer> Dealer;
        public LinkedListNode<GamePlayer> SmallBlind;
        public LinkedListNode<GamePlayer> BigBlind;
        public LinkedListNode<GamePlayer> CurrentActionOn;


        public Table() : this(new TableConfiguration()) { }

        public Table(TableConfiguration config)
        {
            Players = new LinkedList<GamePlayer>();
            PlayersMap = new Dictionary<string, LinkedListNode<GamePlayer>>();
            TableConfig = config;
        }

        public void AddPlayer(GamePlayer player)
        {
            player.SeatNumber = Players.Count + 1;
            player.SetCurrentStack(TableConfig.StartingChipCount); //TODO: Check if the player is reconnecting or something

            var playersNode = Players.AddLast(player);

            PlayersMap[player.Id] = playersNode;
            HandlePlayerCountChange();
        }
        public void RemovePlayer(GamePlayer player)
        {
            var ok = Players.Remove(player);

            if (ok) { 
                PlayersMap.Remove(player.Id);
                HandlePlayerCountChange();
            }

        }
        public void RemovePlayer(string playerId)
        {
            
            if (PlayersMap.ContainsKey(playerId))
            {
                var playerNode = PlayersMap[playerId];
                RemovePlayer(playerNode.Value);
            }
        }

        public void HandlePlayerCountChange()
        {
            if (Players.Count < 2)
            {
                NotEnoughtPlayersToPlayEvent?.Invoke(Players.Count);
            }
            else if (Players.Count == 2)
            {
                EnoughPlayersToPlayEvent?.Invoke(Players.Count);
            }
        }

        public void MoveDealerButton()
        {
            Dealer = Dealer == null ? Players.First : GetNextActivePlayerNode(Dealer);
        }
        public void MoveBlindsPointers()
        {
            SmallBlind = GetNextActivePlayerNode(Dealer);
            BigBlind = GetNextActivePlayerNode(SmallBlind);
        }

        
        public LinkedListNode<GamePlayer> GetNextActivePlayerNode(LinkedListNode<GamePlayer> startingNode)
        {
            CurrentActionOn = startingNode;

            while(!(CurrentActionOn = CurrentActionOn.Next ?? Players.First).Value.ActiveInHand) // Updates the CurrentActionOn while looping.
            {
                if(CurrentActionOn == startingNode)
                {
                    CurrentActionOn = null;
                    break;
                    // throw new Exception("NO_MORE_ACTIVE_PLAYERS");
                }
            }
            return CurrentActionOn;
        }
        public void SetFirstToAct(HandStage stage)
        {
            switch (stage)
            {
                case HandStage.Preflop:
                    if (BigBlind == null) throw new InvalidOperationException("Blinds must be set first.");
                    CurrentActionOn = GetNextActivePlayerNode(BigBlind);
                    break;
                default:
                    CurrentActionOn = GetNextActivePlayerNode(Dealer);
                    break;
            }
        }

        //TODO: replace this loop with a counter prop.
        public int ActivePlayersCount()
        {
            var count = 0;
            foreach (var player in Players)
            {
                count += Convert.ToInt32(player.ActiveInHand);
            }
            return count;
        }

        public delegate void PlayerCountChange(int count);

        public event PlayerCountChange EnoughPlayersToPlayEvent;
        public event PlayerCountChange NotEnoughtPlayersToPlayEvent;

    }
}
