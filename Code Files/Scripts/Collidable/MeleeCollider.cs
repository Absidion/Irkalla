using System.Collections.Generic;
using UnityEngine;
using TheNegative.AI;

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class MeleeCollider : SyncBehaviour
{
    public enum ObjectTags { Player, Enemy }
    public ObjectTags OwningObject;                 //The object that owns this collider

    private int m_ReferenceID = 0;                  //The reference ID of the gameobject that dealt damage to something
    private int m_Damage = 1;                       //The damage that this collider can deal
    private Status[] m_ElementDamage;               //The elemental damage that this collider deals

    private List<Transform> m_RootObjectsHit;       //A list of transforms that represent gameobjects hit by the collider 

    private Collider m_Collider;                    //The collider for the melee collider
    private Player m_PlayerRef;                     //if player, get reference so we can get access to boosted damage

    protected override void Awake()
    {
        base.Awake();
        m_Collider = GetComponent<Collider>();
        m_PlayerRef = transform.root.gameObject.GetComponent<Player>();
    }

    //Important that when the collider becomes disabled that the items in the rootobjectshit gets cleared since 
    //next time the collider becomes active they will be able to take damage
    private void OnDisable()
    {
        if(m_RootObjectsHit != null)
            m_RootObjectsHit.Clear();
    }

    private void Update()
    {
        if (photonView.isMine)
        {
            if (OwningObject == ObjectTags.Player && m_PlayerRef.BoostedMeleeDamage != m_Damage)
                m_Damage = m_PlayerRef.BoostedMeleeDamage;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if the object has already been hit then remove 
        if (m_RootObjectsHit.Contains(collision.collider.transform.root)) return;

        if (collision.collider.gameObject.tag == "Player" && OwningObject != ObjectTags.Player)
        {
            Player player = collision.collider.transform.root.GetComponent<Player>();
            if (player.photonView.isMine)
                player.TakeDamage(m_Damage, transform.position ,m_ElementDamage);

            //add the root transform to the list in order to make sure that this object cannot be spammed with damage
            m_RootObjectsHit.Add(collision.collider.transform.root);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (photonView.isMine)
        {
            if (m_RootObjectsHit.Contains(other.transform.root)) return;

            if (other.gameObject.tag == "Enemy" && OwningObject != ObjectTags.Enemy)
            {
                AI aiTakeDamage = other.transform.root.GetComponent<AI>();
                aiTakeDamage.photonView.RPC("TakeDamage", PhotonTargets.MasterClient, m_ReferenceID, m_Damage, m_ElementDamage, AIUtilits.GetCritMultiplier(other.gameObject));

                //add the root transform to the list in order to make sure that this object cannot be spammed with damage
                m_RootObjectsHit.Add(other.transform.root);
            }
        }
    }

    public void Init(int referenceNumber, int damage, Status[] statuses)
    {
        m_ReferenceID = referenceNumber;
        m_Damage = damage;
        m_ElementDamage = statuses;
        m_RootObjectsHit = new List<Transform>();
    }

    public void SetActive(bool flag)
    {
        photonView.RPC("RPCSetActive", PhotonTargets.All, flag);
    }

    [PunRPC]
    protected void RPCSetActive(bool flag)
    {
        enabled = flag;
        m_Collider.enabled = flag;  
    }

    public int Damage { get { return m_Damage; } set { m_Damage = value; } }
    public Status[] ElementDamage { get { return m_ElementDamage; } set { m_ElementDamage = value; } }
}
