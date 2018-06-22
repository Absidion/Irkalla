using UnityEngine;
using UnityEngine.AI;

//Author: Josue
//Last edited: Liam 12/30/2017

namespace TheNegative.AI.Node
{
    public class RepositionNode : Node
    {
        private float m_Cooldown = 6.0f;           //time it takes until the AI can reposition again
        private float m_TravelDistance = 3.0f;     //distance the AI will travel when repositioning

        private float m_Timer = 0.0f;           //timer that increments while on cooldown

        public RepositionNode(AI reference, float cooldown, float travelDistance) : base(reference)
        {
            m_Cooldown = cooldown;
            m_TravelDistance = travelDistance;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_Timer > m_Cooldown)
            {
                //pick a random direction
                Vector3 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
                Vector3 destination = m_AIReference.transform.position + randomDirection * m_TravelDistance;
                NavMeshPath testPath = new NavMeshPath();

                //see if the direction leads to a valid path, otherwise go in the opposite direction
                if(m_AIReference.Agent.CalculatePath(destination, testPath) == true)
                {
                    m_AIReference.Agent.SetDestination(destination);
                }
                else if(m_AIReference.Agent.isOnNavMesh)
                {
                    destination = m_AIReference.transform.position + (-randomDirection) * m_TravelDistance;
                    m_AIReference.Agent.SetDestination(destination);
                }

                m_Timer = 0.0f;
                return BehaviourState.Succeed;
            }
            else
            {
                m_Timer += Time.deltaTime;
                return BehaviourState.Failed;
            }
        }
    }
}