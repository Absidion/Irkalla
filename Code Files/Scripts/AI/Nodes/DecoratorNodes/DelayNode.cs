using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class DelayNode : Node
    {
        private bool m_IsDelaying = false;
        private float m_Timer = 0.0f;
        private float m_DelayTime = 0.0f;

        public DelayNode(AI reference, float delayTime) : base(reference)
        {
            m_DelayTime = delayTime;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (!m_IsDelaying)
            {
                m_IsDelaying = true;
                return BehaviourState.Running;
            }
            else if (m_IsDelaying && m_Timer >= m_DelayTime)
            {
                m_Timer = 0.0f;
                m_IsDelaying = false;
                return BehaviourState.Succeed;
            }

            return BehaviourState.Running;
        }

        public override void LateUpdate()
        {
            if (m_IsDelaying)
                m_Timer += Time.deltaTime;
        }
    }
}
