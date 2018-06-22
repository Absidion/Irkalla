//Writer: Liam
//Last Updated: 12/30/2017

namespace TheNegative.AI.Node
{
    public class TargetingHighestDamageNode : TargetingNode
    {
        public TargetingHighestDamageNode(AI reference, int score) : base(reference, score) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            float highestDamage = float.MinValue;
            int playerNumber = -1;

            foreach (Player player in m_AIReference.PlayerList)
            {
                //if the player has done more then the previous number save there information for later processing
                if (m_AIReference.DamageTaken[player.PlayerNumber] > highestDamage)
                {
                    highestDamage = m_AIReference.DamageTaken[player.PlayerNumber];
                    playerNumber = player.PlayerNumber;
                }
                //if the damage is the same it means that both players have done the same amount of damage so add points to this player here
                else if (m_AIReference.DamageTaken[player.PlayerNumber] == highestDamage)
                {
                    m_AIReference.Scores[player.PlayerNumber] += m_Score;
                }
            }

            //if we have a valid player number then increase that players score value
            if (playerNumber != -1)
            {
                m_AIReference.Scores[playerNumber] += m_Score;
            }

            return BehaviourState.Succeed;
        }
    }
}
