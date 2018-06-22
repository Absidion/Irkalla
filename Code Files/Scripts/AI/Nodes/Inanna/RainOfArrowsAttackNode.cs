using UnityEngine;

namespace TheNegative.AI.Node
{
    public class RainOfArrowsAttackNode : Node
    {
        private CircularAOE m_AOE;                  //The circular AOE reference which when enabled represents the visual attack
        private float m_WaitTime = 0.0f;            //The amount of time that is delayed before the attack falls from the sky
        private float m_WaitTimer = 0.0f;           //The current timer that represent the amount of time that has been waiting

        public RainOfArrowsAttackNode(AI reference, CircularAOE aoe, float waitTime) : base(reference)
        {
            m_AOE = aoe;
            m_WaitTime = waitTime;
        }

        public override void OnFirstTreeCall()
        {
            base.OnFirstTreeCall();

            m_AOE.SetAOEPos(((InannaAI)m_AIReference).ArrowRainTargetPos);
            m_AOE.SetActive(true);
            m_AOE.SetProjectorActive(true);
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //increase the wait time before the arrow rain attack will hit the field
            m_WaitTimer += Time.deltaTime;

            if(m_WaitTimer >= m_WaitTime)
            {
                //once the wait time has passed it is now time to unleash the rain of arrows attack so enable the AOE attack object
                m_AOE.SetAOEActive(true);
                return BehaviourState.Succeed;
            }

            return BehaviourState.Running;
        }

        public override void Reset()
        {
            base.Reset();

            m_WaitTimer = 0.0f;
        }
    }
}