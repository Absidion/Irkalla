using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Josue
//Last updated: 3/19/2018

namespace TheNegative.AI.Node
{
    public class HealthLessThanNode : Node
    {
        private float m_healthPercentage = 0.0f;

        public HealthLessThanNode(AI reference, float healthPercentage) : base(reference)
        {
            m_healthPercentage = healthPercentage;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_AIReference.Health.HP <= (m_AIReference.Health.MaxHp * m_healthPercentage))
                return BehaviourState.Succeed;
            else
                return BehaviourState.Failed;
        }
    }
}
