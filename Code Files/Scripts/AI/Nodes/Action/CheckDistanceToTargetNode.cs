namespace TheNegative.AI.Node
{

    public class CheckDistanceToTargetNode : Node
    {
        private float m_SqrDistance;            //The sqrDistance

        public CheckDistanceToTargetNode(AI reference, float acceptableRange) : base(reference)
        {
            m_SqrDistance = acceptableRange * acceptableRange;            
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //if the distance between the ai and the player is less then the acceptable distance return failed
            if ((m_AIReference.Target.transform.position - m_AIReference.transform.position).sqrMagnitude <= (m_SqrDistance))
                //else return succeed
                return BehaviourState.Succeed;
            return BehaviourState.Failed;
        }
    }
}