using UnityEngine;

namespace TheNegative.AI.Node
{
    public class ToggleMeleeColliderNode : ToggleColliderNode
    {
        private MeleeCollider m_MeleeCollider;
        private int m_Damage;

        public ToggleMeleeColliderNode(AI reference, Collider colliderRef, int damage) : base(reference, colliderRef)
        {
            m_MeleeCollider = colliderRef.gameObject.GetComponent<MeleeCollider>();
            m_Damage = damage;
        }

        public override void OnFirstTreeCall()
        {
            base.OnFirstTreeCall();
            m_MeleeCollider.Damage = m_Damage;
            m_MeleeCollider.SetActive(!m_MeleeCollider.enabled);
        }
    }
}