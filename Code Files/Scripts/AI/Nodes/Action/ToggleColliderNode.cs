using UnityEngine;

namespace TheNegative.AI.Node
{
    public class ToggleColliderNode : Node
    {
        private Collider m_ColliderReference;           //Reference to the collider object of the AI that it wishes to toggle

        public ToggleColliderNode(AI reference, Collider colliderRef) : base(reference)
        {
            m_ColliderReference = colliderRef;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //set the collider to the inverse of its current state
            m_ColliderReference.enabled = !m_ColliderReference.enabled;
            return BehaviourState.Succeed;
        }
    }
}