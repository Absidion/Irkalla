namespace TheNegative.AI.Node
{
    public class ApproachTargetNode : MovementFormNode
    {
        public ApproachTargetNode(AI reference, bool waitForMovementToFinish) : base(reference, waitForMovementToFinish) { }

        public override void OnFirstTreeCall()
        {
            if(m_AIReference.Target != null)
                m_AIReference.Agent.SetDestination(m_AIReference.Target.transform.position);

            base.OnFirstTreeCall();
        }
    }
}