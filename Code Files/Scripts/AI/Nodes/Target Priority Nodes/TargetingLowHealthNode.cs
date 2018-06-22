//Writer: Liam
//Last Updated: Liam 12/30/2017

namespace TheNegative.AI.Node
{
    public class TargetingLowHealthNode : TargetingNode
    {
        public TargetingLowHealthNode(AI reference, int score) : base(reference, score) { }

        //check the players in the list and see which one has the lowest HP value, add the score value to that target
        public override BehaviourState UpdateNodeBehaviour()
        {
            float lowestHP = float.MaxValue;
            int lowHpPlayerNumber = -1;

            foreach(Player player in m_AIReference.PlayerList)
            {            
                //if the current iterated player has the lowest health remember there info
                if(player.Health.HP < lowestHP)
                {
                    lowestHP = player.Health.HP;
                    lowHpPlayerNumber = player.PlayerNumber;
                }
                //if they're equal then we should add points now because it means that the players have matching HP values, unless somehow a player got float.MaxValue as their health
                else if(player.Health.HP == lowestHP)
                {
                    m_AIReference.Scores[player.PlayerNumber] += m_Score;
                }
            }

            //if we have a valid player number then increase the score related to that player
            if (lowHpPlayerNumber != -1)
            {
                m_AIReference.Scores[lowHpPlayerNumber] += m_Score;
            }

            return BehaviourState.Succeed;
        }
    }
}