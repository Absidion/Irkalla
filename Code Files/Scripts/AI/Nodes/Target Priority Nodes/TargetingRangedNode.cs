//Writer: Liam
//Last Updated: Liam 12/30/2017

namespace TheNegative.AI.Node
{
    public class TargetingRangedNode : TargetingNode
    {
        public TargetingRangedNode(AI reference, int score) : base(reference, score) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            foreach(Player player in m_AIReference.PlayerList)
            {
                //TODO: IN THE FUTURE WHEN THE PROJECT HAS MULTIPLE WEAPONS/WEAPONTYPES IMPLEMENTED UNCOMMENT CODE
                //if(player.ActiveWeapon.Classification == WeaponType.Ranged)
                //{
                //    m_AIReference.Scores[player.PlayerNumber] += m_Score;
                //}
            }

            return BehaviourState.Succeed;
        }
    }
}