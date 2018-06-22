namespace TheNegative.AI.Node
{
    public abstract class MovementFormNode : Node
    {
        //If this boolean is true then it means that the Node will always return true after setting the agent's path in update.
        //This is useful if you want the AI to do other calculations or abilities while also moving, however if you want the AI
        //To move before calculations then make sure this bool is false.
        private bool m_WaitForMovementToFinish = false;
        //use this to determine if the path calculation failed. If the calculation did fail then the node should return failed
        //because no path is calculated
        private bool m_DidPathCalculationFail = false;

        public MovementFormNode(AI reference, bool waitForMovementToFinish) : base(reference)
        {
            m_WaitForMovementToFinish = waitForMovementToFinish;
        }

        //The base functionality must be called after the calculations to get a new destionation for the AI has been made otherwise errors will be caused 
        public override void OnFirstTreeCall()
        {
            base.OnFirstTreeCall();

            if(!m_AIReference.Agent.hasPath)
            {
                m_DidPathCalculationFail = true;
            }
            else
            {
                m_DidPathCalculationFail = false;
            }
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_DidPathCalculationFail)
                return BehaviourState.Failed;

            if (m_WaitForMovementToFinish)
            {
                return m_AIReference.Agent.hasPath ? BehaviourState.Running : BehaviourState.Succeed;
            }
            else
            {
                return BehaviourState.Succeed;
            }
        }

        public override void Stop()
        {
            base.Stop();
            m_AIReference.Agent.ResetPath();
        }
    }
}