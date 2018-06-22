//Writer: Liam
//Last Updated: Liam 12/30/2017

namespace TheNegative.AI.Node
{

    public class TargetingHighHealthNode : TargetingNode
    {
        public TargetingHighHealthNode(AI reference, int score) : base(reference, score) { }

        //check the players in the list and see which one has the highest HP value, add the score value to that target
        public override BehaviourState UpdateNodeBehaviour()
        {
            float highestHPValue = float.MinValue;
            int highestHPPlayerNumber = -1;

            foreach (Player player in m_AIReference.PlayerList)
            {
                if (player.Health.HP > highestHPValue)
                {
                    highestHPValue = player.Health.HP;
                    highestHPPlayerNumber = player.PlayerNumber;
                }
                else if (player.Health.HP == highestHPValue)
                {
                    m_AIReference.Scores[player.PlayerNumber] += m_Score;
                }
            }

            //if there is a valid player number apply the score changes to that player number
            if (highestHPPlayerNumber != -1)
            {
                m_AIReference.Scores[highestHPPlayerNumber] += m_Score;
            }

            return BehaviourState.Succeed;
        }
    }
}
