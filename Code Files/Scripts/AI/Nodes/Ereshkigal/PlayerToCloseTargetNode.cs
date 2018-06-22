using UnityEngine;

namespace TheNegative.AI.Node
{
    public class PlayerToCloseTargetNode : Node
    {
        private float m_TriggerDistance = 0.0f;             //The trigger distance at which the player and target distance cannot go over
        private Transform m_TargetTransform = null;         //The transform of the target object that we wish to "protect"

        public PlayerToCloseTargetNode(AI reference, Transform targetTransform, float triggerDistance) : base(reference)
        {
            m_TargetTransform = targetTransform;
            m_TriggerDistance = triggerDistance;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {

            if (m_TargetTransform != null)
            {
                //iterate through the players and figure out if any are to close to the target transform
                foreach (Player player in m_AIReference.PlayerList)
                {
                    if (!player.CanBeTargeted)
                        continue;

                    float distanceToTarget = (player.transform.position - m_TargetTransform.position).magnitude;
                    if (distanceToTarget < m_TriggerDistance)
                        return BehaviourState.Succeed;
                }
            }

            return BehaviourState.Failed;
        }
    }
}