//Writer: Liam
//Last Updated: 12/30/2017

namespace TheNegative.AI.Node
{
    public abstract class TargetingNode : Node
    {
        protected int m_Score = 0;          //The score which the target node holds

        public TargetingNode(AI reference, int score) : base(reference)
        {
            m_Score = score;
        }
    }
}