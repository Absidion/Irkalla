namespace TheNegative.AI.Node
{

    public class TargetSwitchNode : Node
    {
        private int m_CurrentTarget = -1;               //The player number of the AI's current target 

        public TargetSwitchNode(AI reference) : base(reference)
        {
            //If this is the first time running through the node then randomize the AI's target index
            UnityEngine.Random.Range(1, m_AIReference.PlayerList.Count);
        }

        public override void OnFirstTreeCall()
        {
            base.OnFirstTreeCall();
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //This is a 400 IQ turnary which will determine the target based on the size of the player list and who the current player is
            m_AIReference.Target = (m_CurrentTarget - 1 == m_AIReference.PlayerList.Count % 2) ? m_AIReference.PlayerList[1] : m_AIReference.PlayerList[0];
            m_CurrentTarget = m_AIReference.Target.PlayerNumber;
            return BehaviourState.Succeed;
        }
    }
}