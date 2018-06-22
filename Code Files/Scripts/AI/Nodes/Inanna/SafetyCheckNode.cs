using System;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class SafetyCheckNode : Node
    {    
        public SafetyCheckNode(AI reference) : base(reference) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            return ((InannaAI)m_AIReference).Cycle.ShouldChangeCycles() ? BehaviourState.Succeed : BehaviourState.Failed;
        }
    }
}