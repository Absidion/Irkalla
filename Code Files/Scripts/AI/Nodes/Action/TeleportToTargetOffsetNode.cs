using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class TeleportToTargetOffsetNode : Node
    {
        private Vector3 m_Offset;

        public TeleportToTargetOffsetNode(AI reference, Vector3 offset) : base(reference)
        {
            m_Offset = offset;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            Vector3 newLocation = m_AIReference.Target.transform.position + m_Offset;
            m_AIReference.transform.position = newLocation;

            return BehaviourState.Succeed;
        }
    }
}
