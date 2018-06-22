using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeleeWeaponCollider : MonoBehaviour
{
    private Player playerRef;

    public void Awake()
    {
        playerRef = transform.root.GetComponent<Player>();
    }

    public void OnTriggerEnter(Collider other)
    {
        //If the thing collided with is tagged as a player get the health Component
        if (other.gameObject.tag == "Enemy")
        {
            //Get the health component and deal the damage if a component is found
            AITakeDamageInterface enemy = other.transform.root.GetComponent<AITakeDamageInterface>();
            if (PhotonNetwork.isMasterClient)
            {
                for (int i = 0; i < playerRef.NumOfAttacks; i++)
                {
                    enemy.TakeDamage(playerRef.PlayerNumber, (playerRef.BoostedMeleeDamage + 2) * playerRef.NumOfAttacks, null, AIUtilits.GetCritMultiplier(other.gameObject));
                }
            }
        }
    }
}
