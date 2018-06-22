using UnityEngine;

//Writer: Liam
//Last Updated: Liam 1/5/2018

namespace TheNegative.AI.Node
{
    public class AgentMoveToLocationNode : Node
    {
        private Vector3 m_Location = Vector3.zero;                      //The location the player is going to move to

        public AgentMoveToLocationNode(AI reference, Vector3 location) : base(reference)
        {
            m_Location = location;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //if the agent isn't on the Nav mesh then it cannot possibly move on the nav mesh. Return failed.
            if (!m_AIReference.Agent.isOnNavMesh)
                return BehaviourState.Failed;

            //check to see if the AI is close to the location or if the destination is actually set
            if(((m_AIReference.transform.position - m_Location).magnitude < MathFunc.LargeEpsilon) &&
                (m_AIReference.Agent.destination != m_Location))
            {
                
                m_AIReference.Agent.SetDestination(m_Location);
            }
            else
            {
                m_AIReference.Agent.ResetPath();
                return BehaviourState.Succeed;
            }

            return BehaviourState.Running;
        }

        public override void Reset()
        {
            m_Location = Vector3.zero;
        }

        public Vector3 Location { get { return m_Location; } set { m_Location = value; } }
    }
}