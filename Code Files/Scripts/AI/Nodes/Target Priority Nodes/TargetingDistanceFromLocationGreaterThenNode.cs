using UnityEngine;

namespace TheNegative.AI.Node
{
    public class TargetingDistanceFromLocationGreaterThenNode : TargetingNode
    {
        private Transform m_TargetLocation;             //The target location that is used to compare player distance to
        private float m_AcceptableRange = 0.0f;         //The acceptable range that the players can be in to not add points to them

        public TargetingDistanceFromLocationGreaterThenNode(AI reference, Transform targetLocation, float acceptableRange, int score) : base(reference, score)
        {
            m_TargetLocation = targetLocation;
            m_AcceptableRange = acceptableRange;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_TargetLocation != null)
            {
                foreach (Player player in m_AIReference.PlayerList)
                {
                    float distance = (player.transform.position - m_TargetLocation.position).magnitude;

                    //check to see if the distance from the player to the target location is greater then the acceptable range then add 0 to the score, otherwise add the score
                    m_AIReference.Scores[player.PlayerNumber] += (distance > m_AcceptableRange) ? 0 : m_Score;
                }
            }

            return BehaviourState.Succeed;
        }
    }
}