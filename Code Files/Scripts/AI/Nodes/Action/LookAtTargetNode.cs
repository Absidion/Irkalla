using System;

namespace TheNegative.AI.Node
{
    public class LookAtTargetNode : Node
    {
        public LookAtTargetNode(AI reference) : base(reference)        {        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            m_AIReference.transform.LookAt(m_AIReference.Target.transform.position);

            return BehaviourState.Succeed;
        }
    }
}
