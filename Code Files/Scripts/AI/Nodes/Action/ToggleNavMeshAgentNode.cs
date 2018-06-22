namespace TheNegative.AI.Node
{
    public class ToggleNavMeshAgentNode : Node
    {
        public ToggleNavMeshAgentNode(AI reference) : base(reference) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            m_AIReference.Agent.enabled = !m_AIReference.Agent.enabled;
            return BehaviourState.Succeed;
        }
    }
}