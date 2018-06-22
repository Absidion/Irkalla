using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class DamageCountManager : MonoBehaviour
{
    public int PlayerWhoLastDealtDamage;                 // A reference to the player who last dealt Damage
    public GameObject AIWhichLastTookDamage;                // The AI Which last Took Damage
    public List<string> FilteredTags;                       // A filtereing system to disable damage numbers for players or AI or whatever has a healt component
    public static DamageCountManager instance = null;       // The singleton instance of this Component
    public GameObject DisplayNumberPrefab;                  // Name of a prefab used to duplicate the DamageNumbers from
    private const string m_DamageNumber = "DamageNumber";
    private bool m_IsInitialized = false;
    private Player m_LocalPlayerRef;

    // Use this for initialization
    void Start()
    {
        GameManager.FinalStep += DamageCountOnSceneLoaded;
        instance = this;
        ObjectPoolManager.Instance.CreateOfflinePoolWithName(m_DamageNumber, 30, 100, DisplayNumberPrefab, true);
    }
    private void OnDestroy()
    {
        GameManager.FinalStep -= DamageCountOnSceneLoaded;
    }

    void DamageCountOnSceneLoaded(object sender, EventArgs loadFinished)
    {
        Init();
    }

    private void Init()
    {
        //Get all the players in the game
        Player[] players = FindObjectsOfType<Player>();

        //If the players havent been spawned yet return
        if (players.Length != PhotonNetwork.playerList.Length)
            return;

        //Loop through the players found
        foreach (Player player in players)
        {
            if (player.photonView.isMine)
            {
                m_LocalPlayerRef = player;
            }
        }
    }

    public void OnTakeDamageEvent(object source, DamageTakenArgs damageArgs)
    {
        //Check to see if the health compoent has an active DamageNumber
        GameObject healthObj = damageArgs.DamageTaker.gameObject;
        
        //Cancel if the Health Component's parent has a filtered tag
        foreach (string filter in FilteredTags)
        {
            if (healthObj.tag == filter)
                return;
        }

        Health healthComponent = healthObj.GetComponent<Health>();
        int PlayerWhoDealtDamageNum = damageArgs.PlayerNum;

        AIWhichLastTookDamage = healthComponent.gameObject;

        //Only do this logic for the local player
        if (PlayerWhoDealtDamageNum != -1)
        {
            PlayerWhoLastDealtDamage = PlayerWhoDealtDamageNum;
            //If the player who dealt the damage is local do the logic
            if (PlayerWhoDealtDamageNum == m_LocalPlayerRef.PlayerNumber)
            {

                //If the Health Component already has a Damage number add to it
                if (healthComponent.DamageNumber != null)
                {
                    //Add the damage to the existing Damage Number
                    healthComponent.DamageNumber.AddDamage(damageArgs.Damage);
                    return;
                }
                //If the Damage AI does not already have an active Damage number set it
                else
                {

                    //Get a DisplayNumber from the pool
                    GameObject obj = ObjectPoolManager.Instance.GetObjectFromOfflinePool(m_DamageNumber);
                    obj.transform.SetParent(transform);
                    obj.SetActive(true);
                    //Set the DisplayNumberPrefab's pos
                    obj.transform.position = healthObj.transform.position;
                    //Set the Damage
                    DamageNumber damageNum = obj.GetComponent<DamageNumber>();
                    damageNum.AddDamage(damageArgs.Damage, healthComponent,m_LocalPlayerRef, damageArgs.DamageNumberColor);
                }
            }
        }
    }
}


// the delegate to be used for Take Damage Event
public delegate void DamageTakenDelegate(object source, DamageTakenArgs damageArgs);
public class DamageTakenArgs
{
    public int     Damage;
    public Health  DamageTaker;
    public int     PlayerNum = -1;
    public Color   DamageNumberColor = Color.white;
    public DamageTakenArgs(int damage,  Health dest)
    {
        Damage = damage;
        DamageTaker = dest;
    }

    public DamageTakenArgs(int damage, Health dest, int player, Color damageNumberColor)
    {
        Damage = damage;
        DamageTaker = dest;
        PlayerNum = player;
        DamageNumberColor = damageNumberColor;
    }
}