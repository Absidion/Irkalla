//Writer: Liam
//Last Updated: Liam 12/31/2017

namespace TheNegative.AI.Node
{
    public class BerserkNode : Node
    {
        private GroundSpikesNode m_SpikeNode;               //Reference to the SpikeNode
        private GroundSlamNode m_SlamNode;                  //Reference to the SlamNode
        private RockThrowNode m_RockThrowNode;              //Reference to the RockThrowNode

        private bool m_BeserkModeTriggered = false;         //Whether or not the beserk mode has been triggered
        private bool m_AnimationFinished = false;           //node succeds when animation is finished

        private float m_BeserkPercentage = 0.4f;            //What percentage of maximum health the AI needs to be at to activate this effect
        private GolemAI m_GolemAI = null;

        public bool AnimationFinished { get { return m_AnimationFinished; } set { m_AnimationFinished = value; } }

        public BerserkNode(AI reference, GroundSpikesNode groundSpike, GroundSlamNode groundSlam, RockThrowNode rockThrow, float beserkHealthPercent) : base(reference)
        {
            m_SpikeNode = groundSpike;
            m_SlamNode = groundSlam;
            m_RockThrowNode = rockThrow;
            m_BeserkPercentage = beserkHealthPercent;
            m_GolemAI = m_AIReference as GolemAI;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //if this node hasn't been activated before and the maxHP of the AI is lower then the correct percentage then the AI will activate this node one time
            if(!m_BeserkModeTriggered &&
                (m_AIReference.Health.HP < (m_AIReference.Health.MaxHp * m_BeserkPercentage)))
            {
                m_AIReference.Speed += 2.0f;
                //m_SpikeNode.CooldownTime -= 2.0f;
                //m_SlamNode.CooldownTime -= 5.0f;
                m_BeserkModeTriggered = true;
                SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "GolemYell2", m_AIReference.transform.position);
                return BehaviourState.Running;
            }
            else if (m_GolemAI)
            {
                return BehaviourState.Running;
            }
            else if (m_AnimationFinished)
            {
                //when animiation finished, return succeed
                m_AnimationFinished = false;
                return BehaviourState.Succeed;
            }

            return BehaviourState.Failed;
        }
    }
}