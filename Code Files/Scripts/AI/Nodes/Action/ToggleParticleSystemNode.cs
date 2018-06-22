using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class ToggleParticleSystemNode : Node
    {
        private GameObject m_ParentObject;
        private ParticleSystemController[] m_ParticleSystems;

        public ToggleParticleSystemNode(AI reference, GameObject particleSystem) : base(reference)
        {
            m_ParentObject = particleSystem;
            m_ParticleSystems = m_AIReference.GetComponentsInChildren<ParticleSystemController>();

            foreach (ParticleSystemController ps in m_ParticleSystems)
            {
                ps.StopParticleSystem();
            }
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (!m_ParentObject.activeInHierarchy)
            {
                foreach (ParticleSystemController ps in m_ParticleSystems)
                {
                    ps.SetActive(true);
                    ps.StartParticleSystem();
                }
            }
            else
            {
                foreach (ParticleSystemController ps in m_ParticleSystems)
                {
                    ps.StopParticleSystem();
                }
            }

            return BehaviourState.Succeed;
        }
    }
}
