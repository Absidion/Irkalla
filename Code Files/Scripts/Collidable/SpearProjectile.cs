using UnityEngine;

public class SpearProjectile : ObjectProjectile
{
    private TheNegative.AI.EreshkigalAI m_EreshkigalReference;

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & m_LayerMask) != 0)
        {
            if (collision.collider.tag == "Player")
            {
                Player collidedPlayer = collision.collider.GetComponent<Player>();

                Status[] josuePleaseFixThis = new Status[]{Status.Stun};                

                if (collidedPlayer.photonView.isMine)
                    collidedPlayer.TakeDamage(m_Damage, transform.position, josuePleaseFixThis);

                if(m_EreshkigalReference == null)
                {
                    m_EreshkigalReference = FindObjectOfType<TheNegative.AI.EreshkigalAI>();
                }

                m_EreshkigalReference.FollowUpTarget = collidedPlayer;
            }
        }

        SetActive(false);
    }
}
