using System;
using UnityEngine;

//Author: Josue
//Last edited: James 11/30/2017
public class Cowgirl : Player
{
    public GameObject GrenadePrefab;       // projectile which does explosions
    public float MaxHitScanRange = 40.0f;  // The max range used for the Damage Dropoff 

    private GameObject m_TomahawkPrefab;   //bouncing tomahawk projectile that does more damage over time         
    private const string m_ExplosivePool = "ExplosiveShotPool";
    private int m_SwingStage = 1;          //current attack stage of animation
    [SyncThis]
    protected bool m_HasTomahawk = true;

    public bool HasTomahawk { get { return m_HasTomahawk; } set { m_HasTomahawk = value; } }

    protected override void Awake()
    {
        base.Awake();

        RangedAbilityName = "UI/Texture_HUD_Explosive_Release_1_V2";
        MeleeAbilityName = "UI/Texture_HUD_AxeThrow_Release_4";

        //Cowgirl has a higher maximum crit.
        m_MaxCrit = 3.0f;

        ObjectPoolManager.Instance.CreateNetworkPoolWithName(m_ExplosivePool, "Attacks/" + GrenadePrefab.name, 2, 4, true);

        //create networked prefab of the tomahawk and deactivate over the network
        m_TomahawkPrefab = PhotonNetwork.Instantiate("Attacks/TomahawkProjectile", Vector3.zero, Quaternion.identity, 0);
        m_TomahawkPrefab.GetComponent<LiamBehaviour>().SetActive(false);
    }

    //Checks for any attack inputs such as the primary fire and secondary abilities
    protected override void AttackInputCheck()
    {
        base.AttackInputCheck();
    }

    //Cowgirl primary revolver hitscan fire
    protected override void RangedAttack()
    {
        base.RangedAttack();

        photonView.RPC("FireSingleTargetHitscan", PhotonTargets.All,
                        m_FirstPersonCamera.transform.position, m_FirstPersonCamera.transform.forward,
                        BoostedRangedDamage * m_NumOfAttacks, PlayerNumber, m_Range, MaxHitScanRange, m_IgnoreMask, m_WeaponLayerMask, GunLocation.transform.position);

        SoundManager.GetInstance().photonView.RPC("PlaySFXRandomizedPitchNetworked", PhotonTargets.All, "RevolverPrimaryFire", GunLocation.transform.position);
    }

    protected override void MeleeAttack()
    {
        if (MeleeWeapon.activeInHierarchy)
        {
            ResetAnimationTriggers();
            m_IsAttacking = true;

            photonView.RPC("SetMeleeWeaponTrigger", PhotonTargets.All, true);

            if (m_SwingStage == 3)
                m_SwingStage = 1;

            m_AnimatorRef.SetInteger("MeleeAttackStage", m_SwingStage);
        }
    }

    //Alternate fire that damages enemies in a radius at the shot destination
    protected override void RangedAbility()
    {
        base.RangedAbility();

        RaycastHit hit;

        SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "GrenadeShot", transform.position);

        if (Physics.Raycast(m_FirstPersonCamera.transform.position, m_FirstPersonCamera.transform.forward, out hit, m_Range, m_IgnoreMask))
        {
            //Grab explosive from pool
            GameObject projectile = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_ExplosivePool);

            if (projectile != null)
            {
                //calculate direction and fire projectile
                Vector3 direction = (hit.point - GunLocation.transform.position).normalized;
                projectile.GetPhotonView().RPC("FireProjectile", PhotonTargets.All, GunLocation.transform.position, direction, PlayerNumber, m_WeaponLayerMask, Mathf.RoundToInt(BoostedRangedDamage * 1.5f), null);
            }
        }
    }

    public override void EventFinishedAttack()
    {
        base.EventFinishedAttack();

        //MeleeWeapon.GetComponent<BoxCollider>().enabled = false; //deactivate trigger on weapon
        photonView.RPC("SetMeleeWeaponTrigger", PhotonTargets.All, false);

        if (m_WeaponType == WeaponType.MELEE)
            m_SwingStage++;
    }

    public void EventSpawnTomahawk()
    {
        //fire tomahawk projectile
        RaycastHit hit;

        SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "TommahawkThrow", transform.position);

        if (Physics.Raycast(m_FirstPersonCamera.transform.position, m_FirstPersonCamera.transform.forward, out hit, m_Range, m_IgnoreMask))
        {
            MeleeWeapon.SetActive(false);

            Vector3 direction = (hit.point - GunLocation.transform.position).normalized;
            m_TomahawkPrefab.GetPhotonView().RPC("FireProjectile", PhotonTargets.All, GunLocation.transform.position, direction, PlayerNumber,
                                                    m_WeaponLayerMask, Mathf.RoundToInt(BoostedMeleeDamage * 1.1f), null);
            m_HasTomahawk = false;
        }
    }

    public override void EventFinishedPuttingAway()
    {
        if (m_WeaponType == WeaponType.RANGED)
        {
            if (photonView.isMine)
                photonView.RPC("SetWeaponsActive", PhotonTargets.All, true, false);
        }
        else
        {
            if (photonView.isMine)
                photonView.RPC("SetWeaponsActive", PhotonTargets.All, false, true);
        }
    }

    protected override void PlayInjurySound()
    {
        SoundManager.GetInstance().photonView.RPC("PlayRandomSFXNetworked", PhotonTargets.All, "Injury1", "Injury2", "Injury3", this.gameObject.transform.position);
    }

    protected override void PlayerDeathSound()
    {
        SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "GunslingerDying", this.gameObject.transform.position);
    }
}
