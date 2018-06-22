using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ParryRiposteCollider : MonoBehaviour
{
    private Samurai m_AttachedCharacter;
    private bool m_ParryMode;

    private void Awake()
    {
        m_AttachedCharacter = GetComponentInParent<Samurai>();
        if (m_AttachedCharacter == null)
            Debug.LogError("The samurai value on this parry and riposte collider is null please remeady this.", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_AttachedCharacter.photonView.isMine)
            return;

        if (other.gameObject.tag == "Projectile" || other.gameObject.tag == "Enemy")
        {
            if (!m_ParryMode)
            {
                m_AttachedCharacter.ParryTarget(other.gameObject);
            }
        }
    }

    public Samurai AttachedCharacter { get { return m_AttachedCharacter; } set { m_AttachedCharacter = value; } }
    public bool ParryMode { get { return m_ParryMode; } set { m_ParryMode = value; } }
}
