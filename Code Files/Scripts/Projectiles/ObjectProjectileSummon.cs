using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheNegative.AI;

//Author: Josue
//Last edited: 12/16/2017

public class ObjectProjectileSummon : ObjectProjectile
{
    public int MaxScaledDamage = 20;                        //the maximum scaled damage amount
    public int SummonUpperLimit = 10;                       //initial max amount of shadow demons that can be spawned
    public GameObject ObjectToSummon;                       //reference to shadow demon prefab
    public bool isImproved = false;                         //if true, this projectile also summons objects if it hits a player

    private Vector3 m_OriginalScale = Vector3.one;          //ithe original scale of the projectile

    protected string m_ShadowDemonPool = "ShadowDemon"; //constant name of pool to access from

    protected override void Awake()
    {
        base.Awake();
        ObjectPoolManager.Instance.CreateNetworkPoolWithName(m_ShadowDemonPool, "AI/" + ObjectToSummon.name, false);
        m_OriginalScale = transform.localScale;
    }

    protected override void Update()
    {
        base.Update();

        transform.localScale += new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime) / 4;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Reflector" || m_IsBeingReflected)
        {
            m_IsBeingReflected = false;
            return;
        }

        gameObject.SetActive(false);
   
        if (!PhotonNetwork.isMasterClient)
            return;

        if (((1 << collision.gameObject.layer) & m_LayerMask) != 0) //if the collided gameobject is on the layer provided fuck bitshifts
        {
            IslandRoom room = null;
            transform.localScale = m_OriginalScale;

            if (collision.gameObject.tag == "Player")
            {
                Player playerRef = collision.gameObject.GetComponent<Player>();
                int calcedDamage = (int)(m_Damage * (1 + m_ElapsedTime / 2));
                playerRef.photonView.RPC("TakeDamage", PhotonTargets.All, calcedDamage > MaxScaledDamage ? MaxScaledDamage : calcedDamage, transform.position ,m_StatusEffects);

                //if not the improved version, return as we don't spawn enemies from the player on the normal projectile
                if (!isImproved)
                    return;

                room = playerRef.MyIslandRoom;
            }

            GameObject summonedObject = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_ShadowDemonPool);

            if (summonedObject != null )
            {
                //get the room area component from the root parent of the wall or floor if we didnt hit a player
                if (room == null)
                {
                    room = collision.transform.root.gameObject.GetComponent<IslandRoom>();
                }

                WorshipperAI ownerRef = null;

                //loop through the enemies in the room to get the enemy which matches with the number we were given
                if (m_ProjectileOwnerName != string.Empty)
                {
                    for (int i = 0; i < room.EnemiesInRoom.Count; i++)
                    {
                        if (room.EnemiesInRoom[i] != null)
                        {
                            if (m_ProjectileOwnerName == room.EnemiesInRoom[i].name)
                            {
                                ownerRef = (WorshipperAI)room.EnemiesInRoom[i].gameObject.GetComponent<AI>();
                                break;
                            }
                        }
                    }
                }

                if (ownerRef.NumberOfShadowDemonsSpawned >= ownerRef.ShadowDemonSpawnLimit) return;

                float offset = 3.0f;                                                    //distance away from the hit surface to spawn from
                Vector3 dir = collision.contacts[0].normal;                             //direction to offset away from
                Vector3 spawnLocation = collision.contacts[0].point + dir * offset;     //calculated location to spawn shadow demon
                
                //if owner is found, summon the shadow demon and set it to active
                if (ownerRef != null)
                {
                    ShadowDemonAI ai = summonedObject.GetComponent<ShadowDemonAI>();
                    ai.enabled = true;
                    ai.Init();
                    ai.Health.ResetHealth();
                    ai.MyIslandRoom = room;
                    ai.MasterRef = ownerRef;
                    ai.SetActive(true, spawnLocation, dir);
                    ownerRef.NumberOfShadowDemonsSpawned++;
                }
            }
        }
    }
}