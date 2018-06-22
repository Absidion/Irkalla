//Writer: Liam
//Last Updated: 12/30/2017

namespace TheNegative.AI.Node
{
    public class TargetingCharacterType : TargetingNode
    {
        public WeaponType m_TargetType;

        public TargetingCharacterType(AI reference, int score, WeaponType type) : base(reference, score)
        {
            m_TargetType = type;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            foreach(Player player in m_AIReference.PlayerList)
            {
                if(player.WeaponType == m_TargetType)
                {
                    m_AIReference.Scores[player.PlayerNumber] += m_Score;
                }
            }

            return BehaviourState.Succeed;
        }
    }
}