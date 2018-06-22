//Writer: Liam
//Last Updated: Liam 12/30/2017

namespace TheNegative.AI.Node
{
    public class TargetingMeleeNode : TargetingNode
    {
        public TargetingMeleeNode(AI reference, int score) : base(reference, score) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            foreach (Player player in m_AIReference.PlayerList)
            {
                //TODO: IN THE FUTURE WHEN THE PROJECT HAS MULTIPLE WEAPONS/WEAPONTYPES IMPLEMENTED UNCOMMENT CODE
                //if(player.ActiveWeapon.Classification == WeaponType.Melee)
                //{
                //    m_AIReference.Scores[player.PlayerNumber] += m_Score;
                //}
            }

            return BehaviourState.Succeed;
        }
    }
}