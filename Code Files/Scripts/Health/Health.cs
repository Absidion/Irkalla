using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Writer: Daniel French    
//Date last Modified 11/30/2017 by Josue 

public class Health : SyncBehaviour
{

    [SyncThis]
    public int HP;
    public int MaxHp;
    public int FinalMaxHP;
    [SyncThis]
    public int ArmorHP;
    public int MaxArmorHP = 300;
    public int ReviveCounters = 0;

    //public DamageTakenDelegate OnDamageTaken;   // The Delegate for tossing out the event

    //[Tooltip("Keep this number from 0 -> 100 as it represents a percentage of damage reduced")]
    //instead of using the tooltip, use range with a max and min value and will become a slider in the editor
    [Range(0, 50)]
    public int m_Defence; // a value from 0-100

    private bool m_HasArmor;
    [SyncThis]
    protected bool m_IsDead;
    private bool m_IsInvincible = false;           // when this is true the player cannot take damage 
    private int m_StatusDamagePerSecond = 0;       //value that gets added to each time a status effect is added and gets applied every second
    private float m_StatusDamageTimer = 0.0f;      //timer which incremements and applies damage every refresh
    private float m_StatusDamageRefresh = 1.0f;    //how many seconds must pass before damage is refreshed
    private DamageNumber m_DamageNumber = null;    // This is uses to check if the health component has an active Damage number and add Damage onto a active Damage Number

    public DamageNumber DamageNumber { get { return m_DamageNumber; } set { m_DamageNumber = value; } }
    public bool IsDead { get { return m_IsDead; } set { m_IsDead = value; } }

    public int StatusDamagePerSecond { get { return m_StatusDamagePerSecond; } set { m_StatusDamagePerSecond = value; } }

    private void Start()
    {
        m_IsDead = false;
        if (ArmorHP > 0)
        {
            HasArmor = true;
        }
        else
        {
            HasArmor = false;
        }

        //OnDamageTaken += DamageCountManager.instance.OnTakeDamageEvent;
    }

    private void Update()
    {
        UpdateStatusDamageOverTime();
    }

    public void AddArmorHP(int armorValue)
    {
        ArmorHP += armorValue;

        if (ArmorHP > MaxArmorHP)
        {
            ArmorHP = MaxArmorHP;
        }

        if(ArmorHP >= 0 && HasArmor == false)
        {
            HasArmor = true;
        }
    }

    public void ChangeHp(int amount)
    {
        HP = amount;
        HP = Mathf.Clamp(HP, 0, MaxHp);

    }
    public void ChangeMaxHP(int amount)
    {
        MaxHp += amount;

        MaxHp = Mathf.Clamp(MaxHp, 0, FinalMaxHP);
        HP = MaxHp;
    }

    public int TakeDamage(int Damage)
    {
        if (m_IsInvincible == true)
            return 0;       

        float RealPercentReduction = (100.0f - m_Defence) / 100.0f;
        int DamageTaken = Mathf.RoundToInt(Damage * RealPercentReduction);

        //SendOnDamageEvent(DamageTaken);

        if (HasArmor == true)
        {
            if(DamageTaken > ArmorHP)
            {
                DamageTaken -= ArmorHP;
                ArmorHP = 0;
                HP -= DamageTaken;
                HasArmor = false;
                if(DamageTaken > HP)
                {
                    HP = 0;

                    if (ReviveCounters == 0)
                        m_IsDead = true;
                }
            }
            else
            {
                ArmorHP -= DamageTaken;
            }
        }
        else
        {
            if(DamageTaken >= HP)
            {
                HP = 0;

                if (ReviveCounters == 0)
                    m_IsDead = true;
            }
            else
            {
                HP -= DamageTaken;
            }
        }
        Debug.Log(gameObject.name + " : " + DamageTaken + " | Remainning HP: " + HP);
        return DamageTaken;
    }

    private void UpdateStatusDamageOverTime()
    {
        //if their is an amount of damage to be dealt per second, increment the timer
        //if the timer has past the set refresh time, apply damage and refresh the timer
        if (m_StatusDamagePerSecond > 0)
        {
            m_StatusDamageTimer += Time.deltaTime;

            if (m_StatusDamageTimer >= m_StatusDamageRefresh)
            {
                TakeDamage(m_StatusDamagePerSecond);
                m_StatusDamageTimer = 0.0f;
            }
        }
        else if (m_StatusDamageTimer == 0)
        {
            //if character is clear of damage over time, reset the timer back to 0 once
            if (m_StatusDamageTimer != 0.0f)
            {
                m_StatusDamageTimer = 0.0f;
            }
        }
    }

    public void ResetHealth()
    {
        m_IsDead = false;
        HP = MaxHp;
    }

    //private void SendOnDamageEvent(int Damage)
    //{
    //    DamageTakenArgs args = new DamageTakenArgs(Damage, this);
    //    OnDamageTaken(this, args);
    //}

    public bool HasArmor { get { return m_HasArmor; } set { m_HasArmor = value; } }
    public bool IsInvincible { get { return m_IsInvincible; } set { m_IsInvincible = value; } }

}