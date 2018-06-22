
using System;

namespace TheNegative.AI.Node
{
    public class TargetingHighestDamageToAINode : TargetingNode
    {
        private AI m_AIToProtect;           //The AI that will be protected

        public TargetingHighestDamageToAINode(AI reference, AI aiToProtect, int score) : base(reference, score)
        {
            m_AIToProtect = aiToProtect;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //save values to represent the top damage player and there damage against
            int topDamagePlayerNumber = -1;
            float topDamage = float.MinValue;

            foreach(int playerNumber in m_AIToProtect.DamageTaken.Keys)
            {
                //if the current player number in the damage key has more damage then the topDamage temp var then set the tempDamage value to
                //be equal to their damage and then change the topDamagePlayerNumber to be equal to the key in the damage taken dictionary
                if(m_AIToProtect.DamageTaken[playerNumber] > topDamage)
                {
                    topDamage = m_AIToProtect.DamageTaken[playerNumber];
                    topDamagePlayerNumber = playerNumber;
                }
            }

            //if there is a top damage saved then make sure to add the score bonus to that player
            if (topDamagePlayerNumber != -1)
                m_AIReference.Scores[topDamagePlayerNumber] += m_Score;

            return BehaviourState.Succeed;
        }
    }
}