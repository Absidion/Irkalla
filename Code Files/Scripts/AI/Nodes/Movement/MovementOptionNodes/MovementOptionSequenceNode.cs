namespace TheNegative.AI.Node
{
    //This node is identical to the Sequence Node however it functions only on MovementFormNodes.
    //For more information on how sequences work please read sequence node docs/comments
    public class MovementOptionSequenceNode : MovementOptionNode
    {
        private int m_SequenceLocation = 0;         //The location the movement nodes are in the sequence

        public MovementOptionSequenceNode(AI reference, params MovementFormNode[] childNodes) : base(reference, childNodes) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            for(int i = m_SequenceLocation; i < m_ChildNodes.Count; i++)
            {
                if(!m_ChildNodes[i].FirstCallActivated)
                {
                    m_ChildNodes[i].OnFirstTreeCall();
                }

                BehaviourState nodeState = m_ChildNodes[i].UpdateNodeBehaviour();

                switch (nodeState)
                {
                    case BehaviourState.Failed:
                        Reset();
                        return BehaviourState.Failed;

                    case BehaviourState.Succeed:
                        continue;

                    case BehaviourState.Running:
                        m_SequenceLocation = i;
                        return BehaviourState.Running;
                }
            }

            Reset();
            return BehaviourState.Succeed;
        }

        public override void Reset()
        {
            base.Reset();
            m_SequenceLocation = 0;
        }
    }
}