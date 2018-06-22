namespace TheNegative.AI.Node
{
    public class CanUseChainsNode : Node
    {
        private int m_SpectalChainMaxUses;          //The maximum amount of uses of spectral chains per cycle that Inanna keeps track of

        public CanUseChainsNode(AI reference, int spectralChainMaxUses) : base(reference)
        {
            m_SpectalChainMaxUses = spectralChainMaxUses;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //if the spectral chain used in Ere is less then the max number then return succeed otherwise return failed
            return (((EreshkigalAI)m_AIReference).SpectralChainsUsed < m_SpectalChainMaxUses) ? BehaviourState.Succeed : BehaviourState.Failed;
        }
    }
}