namespace TheNegative.AI.Node
{
    public class MovementOptionRandomNode : MovementOptionNode
    {
        private MovementFormNode m_CurrentRandomChild = null;               //The current randomly chosen child that will be operated on

        public MovementOptionRandomNode(AI reference, params MovementFormNode[] childNodes) : base(reference, childNodes)        {        }

        public override void OnFirstTreeCall()
        {
            base.OnFirstTreeCall();

            m_CurrentRandomChild = m_ChildNodes[UnityEngine.Random.Range(0, m_ChildNodes.Count)];
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (!m_CurrentRandomChild.FirstCallActivated)
                m_CurrentRandomChild.OnFirstTreeCall();

            return m_CurrentRandomChild.UpdateNodeBehaviour();
        }       
    }
}