using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class HasChainReturnedNode : Node
    {
        EreshkigalAI m_EriAI = null;

        public HasChainReturnedNode(AI reference) : base(reference)
        {
            m_EriAI = reference as EreshkigalAI;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_EriAI.SendingChains)
            {
                return BehaviourState.Running;
            }
            else
            {
                return BehaviourState.Succeed;
            }
        }
    }
}
