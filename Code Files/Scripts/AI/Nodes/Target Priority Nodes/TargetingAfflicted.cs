//Writer: Liam
//Last Updated: 12/30/2017

namespace TheNegative.AI.Node
{
    public class TargetingAfflicted : TargetingNode
    {
        private Status m_TargetStatus;

        public TargetingAfflicted(AI reference, int score, Status statusToLookFor) : base(reference, score)
        {
            m_TargetStatus = statusToLookFor;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //iterate through and check to see if the player has the status that this node is look for and if you do add points to there score
            foreach(Player player in m_AIReference.PlayerList)
            {
                if(player.Effects.ContainsKey(m_TargetStatus))
                {
                    m_AIReference.Scores[player.PlayerNumber] += m_Score;
                }
            }

            return BehaviourState.Succeed;
        }
    }
}