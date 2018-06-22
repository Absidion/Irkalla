namespace TheNegative.AI.Node
{
    public class UseOnceNode : Node
    {
        private bool m_TreeUsed = false;

        public UseOnceNode(AI reference) : base(reference) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            return m_TreeUsed ? BehaviourState.Failed : BehaviourState.Succeed;
        }
    }
}