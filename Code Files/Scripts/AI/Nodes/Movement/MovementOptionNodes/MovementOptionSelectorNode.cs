namespace TheNegative.AI.Node
{
    //This node operates exactly like a selector node however it only operates on MovementFormNodes
    //For more information on how Selectors work please read the selector node comments to understand
    public class MovementOptionSelectorNode : MovementOptionNode
    {
        private int m_SelectorIndex = 0;            //The index that the selector is running on

        public MovementOptionSelectorNode(AI reference, params MovementFormNode[] childNodes) : base(reference, childNodes) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            for(int i = m_SelectorIndex; i < m_ChildNodes.Count; i++)
            {
                if (!m_ChildNodes[i].FirstCallActivated)
                    m_ChildNodes[i].OnFirstTreeCall();

                BehaviourState nodeState = m_ChildNodes[i].UpdateNodeBehaviour();

                switch (nodeState)
                {
                    case BehaviourState.Failed:
                        continue;

                    case BehaviourState.Succeed:
                        Reset();
                        return BehaviourState.Succeed;

                    case BehaviourState.Running:
                        m_SelectorIndex = i;
                        return BehaviourState.Running;
                }
            }

            Reset();
            return BehaviourState.Failed;
        }

        public override void Reset()
        {
            base.Reset();
            m_SelectorIndex = 0;
        }
    }
}