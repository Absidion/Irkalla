using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Josue
//Last edited: James 12/1/2017

[RequireComponent(typeof(Rigidbody))]

public class ObjectProjectile : LiamBehaviour
{
    public float ShotSpeed = 1.0f;                          //scalar for how strong the impulse is which pushes the bullet
    public string FireSound = string.Empty;

    protected bool m_IsBeingReflected = false;              //Determines if the projectile is being relfected

    protected int m_LayerMask = 0;                          //what layers this projectile can collide with
    protected int m_Damage = 0;                             //how much damage this projectile does when it hits its target 
    protected Status[] m_StatusEffects;                     //effects that are applied when the projectile hits their target

    protected float m_ElapsedTime = 0.0f;                   //how much time has passed since the projectile has been fired
    protected int m_ProjectileOwner = 0;                    //Int that represents the player number of who shot the projectile
    protected string m_ProjectileOwnerName = string.Empty;  //The Object that fired this projectile
    protected Player m_Player;                              //Player reference to who shot the projectile
    protected Player[] m_PlayerList;                        //List of players that gets filled and used to determine who shot the projectile    

    //Applies an impulse to the projectile in the direction of the weapon
    [PunRPC]
    public void FireProjectile(Vector3 origin, Vector3 direction, int owner, int layerMask, int damage, Status[] statusTypes)
    {
        if (FireSound != string.Empty)
        {
            SoundManager.PlaySFX(FireSound, origin);
        }

        gameObject.SetActive(true);
        gameObject.transform.position = origin;
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.forward = direction;
        m_ProjectileOwner = owner;

        m_LayerMask = layerMask;
        m_Damage = damage;
        m_StatusEffects = statusTypes;

        m_PlayerList = FindObjectsOfType<Player>();
        foreach (Player player in m_PlayerList)
        {
            if (player.PlayerNumber == owner)
            {
                m_Player = player;
            }
        }

        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero; //set the velocity to 0 before applying a force
        gameObject.GetComponent<Rigidbody>().AddForce(direction * ShotSpeed, ForceMode.VelocityChange); //apply impulse to the bullet
    }

    //Applies an impulse to the projectile in the direction of the weapon
    [PunRPC]
    public void FireProjectile(Vector3 origin, Vector3 direction, int owner, int layerMask, int damage, string ownerName, Status[] statusTypes)
    {
        gameObject.SetActive(true);
        gameObject.transform.position = origin;
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.forward = direction;
        m_ProjectileOwner = owner;
        m_ProjectileOwnerName = ownerName;

        m_LayerMask = layerMask;
        m_Damage = damage;
        m_StatusEffects = statusTypes;

        m_PlayerList = FindObjectsOfType<Player>();
        foreach (Player player in m_PlayerList)
        {
            if (player.PlayerNumber == owner)
            {
                m_Player = player;
            }
        }

        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero; //set the velocity to 0 before applying a force
        gameObject.GetComponent<Rigidbody>().AddForce(direction * ShotSpeed, ForceMode.VelocityChange); //apply impulse to the bullet
    }

    //Applies a given impulse in the direction of weapon
    [PunRPC]
    public void FireProjectile(Vector3 origin, Vector3 direction, int owner, int layerMask, int damage, float force, Status[] statusTypes)
    {
        gameObject.SetActive(true);
        gameObject.transform.position = origin;
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.forward = direction;
        m_ProjectileOwner = owner;

        m_LayerMask = layerMask;
        m_Damage = damage;
        m_StatusEffects = statusTypes;

        m_PlayerList = FindObjectsOfType<Player>();
        foreach (Player player in m_PlayerList)
        {
            if (player.PlayerNumber == owner)
            {
                m_Player = player;
            }
        }

        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero; //set the velocity to 0 before applying a force
        gameObject.GetComponent<Rigidbody>().AddForce(direction * force, ForceMode.VelocityChange); //apply impulse to the bullet
    }

    protected override void Update()
    {
        base.Update();
        m_ElapsedTime += Time.deltaTime;
    }

    public int Damage { get { return m_Damage; } set { m_Damage = value; } }
    public bool IsBeingReflected { get { return m_IsBeingReflected; } set { m_IsBeingReflected = value; } }
}
