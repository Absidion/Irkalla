namespace TheNegative.AI.Node
{
    public class ToggleAINode : Node
    {
        public ToggleAINode(AI reference) : base(reference)        {        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            m_AIReference.SetActive(!m_AIReference.enabled);

            return BehaviourState.Succeed;
        }
    }
}