using UnityEngine;
using TheNegative.AI;

public class ObjectProjectileDrainningShot : ObjectProjectile
{
    [SerializeField]
    private AcolyteAI m_AcolyteRef;
    public AcolyteAI AcolyteRef { get { return m_AcolyteRef; } set { m_AcolyteRef = value; } }

    private void OnCollisionEnter(Collision collision)
    {
        gameObject.SetActive(false);

        if (collision.collider.tag == "Player")
        {
            Player player = collision.collider.GetComponent<Player>();
            player.photonView.RPC("TakeDamage", PhotonTargets.All, m_Damage, m_StatusEffects);
            photonView.RPC("OnAttackSucceed", PhotonTargets.MasterClient, collision.contacts[0].point);
        }
        else
        {
            photonView.RPC("OnAttackFailed", PhotonTargets.MasterClient);
        }
    }

    [PunRPC]
    private void OnAttackSucceed(Vector3 recreationPoint)
    {
        AcolyteRef.RecreateAcolyte(m_Damage, recreationPoint);
    }

    [PunRPC]
    private void OnAttackFailed()
    {
        AcolyteRef.Health.TakeDamage(AcolyteRef.Health.HP);
        AcolyteRef.SetActive(true);
    }

}
