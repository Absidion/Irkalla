namespace TheNegative.AI.Node
{
    //Determines if Ere should follow up after a throwing a spear
    public class TargetFollowUp : Node
    {
        public TargetFollowUp(AI reference) : base(reference) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //if the FollowUpTarget isn't null that means Ere should follow up after hitting someone with the spear projectile
            if(((EreshkigalAI)m_AIReference).FollowUpTarget != null)
            {
                m_AIReference.Target = ((EreshkigalAI)m_AIReference).FollowUpTarget;
                return BehaviourState.Succeed;
            }

            return BehaviourState.Failed;
        }
    }
}