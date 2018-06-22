using System;
using UnityEngine;

//Writer: Liam
//Last Updated: 1/5/2018

namespace TheNegative.AI.Node
{
    public class CooldownNode : Node
    {
        protected float m_CooldownValue = 0.0f;
        protected float m_Timer = 0.0f;

        public CooldownNode(AI reference, float cooldownValue) : base(reference)
        {
            m_CooldownValue = cooldownValue;
        }

        public override void LateUpdate()
        {
            m_Timer += Time.deltaTime;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_Timer >= m_CooldownValue)
            {
                m_Timer = 0.0f;
                return BehaviourState.Succeed;
            }

            return BehaviourState.Failed;
        }
    }
}