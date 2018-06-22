//Writer: Liam
//Last Updated: Liam 1/5/2018

namespace TheNegative.AI.Node
{
    public class AgentMoveToTarget : Node
    {
        public AgentMoveToTarget(AI reference) : base(reference)        { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //if the agent isn't on the Nav mesh then it cannot possibly move on the nav mesh. Return failed.
            if (!m_AIReference.Agent.isOnNavMesh)
                return BehaviourState.Failed;
            
            if(((m_AIReference.transform.position - m_AIReference.Target.transform.position).magnitude > MathFunc.LargeEpsilon) &&
                !MathFunc.AlmostEquals(m_AIReference.Agent.destination, m_AIReference.Target.transform.position))
            {
                m_AIReference.Agent.SetDestination(m_AIReference.Target.transform.position);                
            }
            else
            {
                m_AIReference.Agent.ResetPath();
                return BehaviourState.Succeed;
            }

            return BehaviourState.Running;
        }
    }
}