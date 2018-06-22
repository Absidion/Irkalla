using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class RunUntillTargetNull : Node
    {
        public RunUntillTargetNull(AI reference) : base(reference) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_AIReference.Target != null)
                return BehaviourState.Running;
            else
                return BehaviourState.Succeed;
        }
    }
}
