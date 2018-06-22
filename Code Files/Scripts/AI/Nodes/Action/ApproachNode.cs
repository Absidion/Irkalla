using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Author: Josue
//Last edited: Liam 12/30/2017

namespace TheNegative.AI.Node
{
    public class ApproachNode : Node
    {
        public ApproachNode(AI reference) : base(reference) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (!m_AIReference.Agent.isOnNavMesh)
                return BehaviourState.Failed;

            if (m_AIReference.Agent.destination != m_AIReference.Target.transform.position)
            {
                m_AIReference.Agent.SetDestination(m_AIReference.Target.transform.position);
                return BehaviourState.Succeed;
            }

            return BehaviourState.Failed;
        }

        public override void Stop()
        {
            base.Stop();
            m_AIReference.Agent.ResetPath();
        }
    }
}