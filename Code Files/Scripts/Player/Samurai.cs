using TheNegative.AI;
using UnityEngine;
using System.Collections.Generic;

//Author: James
//Last edited: James 11/8/2017

public class Samurai : Player
{
    public ParryRiposteCollider ParryRiposteCollider;
    public GameObject SwordRef;                                         //reference to sword to rotate and enable or disable colliders
    public GameObject BeamPrefab;                                       //Projectile which appears as a slice of energy through the air. Can hit many enemies
    public GameObject OverPenProjectilePrefab;                          //The projectile that is activated during the ranged ability.
    public float SpreadAmount = 0.5f;                                   //The amount of spread that is present on the SHOT GUN O  

    private bool m_BasicAttackActive = false;
    private bool m_MaxRotationHit = false;
    private Quaternion m_OriginalSwordRotation = Quaternion.identity;   //original sword rotation before swinging
    private Vector3 m_OriginalSwordLocation = Vector3.zero;             //original sword location before swinging
    private Vector3 m_WallPosition = Vector3.zero;

    private bool m_IsUsingRiposte = false;                                              //Animation bool for if the player is using the riposte half of the ability
    private float m_ParryTimer = 2.0f;                                                  //The timer for how long the player has been in the Parry half of the PArry and riposte ability. Decrements
    private bool m_WasAttacked = false;                                 //Whether or not the samurai was attacked during the perry and riposte ability
    private int m_ParryDamageBoost = 0;

    private const string m_BeamPoolName = "BeamPool";
    private const string c_OverpenShotPoolName = "OverPenProjectile";

    private bool m_HasAttackBegun = false;
    private BoxCollider m_SwordTrigger = null;                                      //The trigger associated witht the sword
    private int m_ChargeAttackBuffer = 0;                                           //The amount of frames that the player has held down the key for the charge attack to become active
    [SerializeField]
    private int BufferTime = 5;                                                     //The amount of frames required to pass for the player to be moved to the ChargeAttack state
    private int m_SwingStage = 0;                                                   //The current swing stage that the basic swing state is in
    private float m_ChargeTimer = 0.0f;                                             //The timer for how long things have been charging
    private float m_TimeSinceSwordSlash = 0.0f;                                     //The amount of time passed since a sword slash has happened, whether it's charge attack or whether it's a basic sword slash
    private int m_ChargeSwordDamage = 0;                                            //The temporary damage that the sword can deal during this charge attack
    private SamuraiAttackStates m_CurrentAttackStages = SamuraiAttackStates.NULL; //The current attack state that the samurai is in

    protected override void Awake()
    {
        base.Awake();

        RangedAbilityName = "UI/Texture_HUD_OverPenetration_Release3";
        MeleeAbilityName = "UI/Texture_HUD_Deflect_Release_4";
        m_OriginalSwordLocation = SwordRef.transform.localPosition;
        m_OriginalSwordRotation = SwordRef.transform.localRotation;

        //manage the sword/perry information
        m_SwordTrigger = SwordRef.GetComponentInChildren<BoxCollider>();
        photonView.RPC("SetParryCollider", PhotonTargets.All, false);

        ObjectPoolManager.Instance.CreateNetworkPoolWithName(m_BeamPoolName, "Attacks/" + BeamPrefab.name, 2, 5, true);
        ObjectPoolManager.Instance.CreateNetworkPoolWithName(c_OverpenShotPoolName, "Attacks/" + OverPenProjectilePrefab.name, 2, 5, true);

        GetComponent<LineRenderer>().positionCount = 25 * 3;
        EventFinishedPuttingAway();
    }

    protected override void AttackInputCheck()
    {
        if ((CharacterController.GetWeaponOneDown() || CharacterController.GetWeaponTwoDown()) &&
            !m_IsAttacking && !m_isUsingAbility && !m_isSwitchingWeapons)
        {
            if (CharacterController.GetWeaponOneDown() && m_WeaponType != WeaponType.RANGED)
            {
                ResetAnimationTriggers();
                m_isSwitchingWeapons = true;
                m_WeaponType = WeaponType.RANGED;
            }
            else if (CharacterController.GetWeaponTwoDown() && m_WeaponType != WeaponType.MELEE)
            {
                ResetAnimationTriggers();
                m_isSwitchingWeapons = true;
                m_WeaponType = WeaponType.MELEE;
            }
        }
        else if ((CharacterController.GetSwitchHeld() || CharacterController.GetSwitchPercentHeld() != 0) &&
            !m_IsAttacking && !m_isUsingAbility && !m_isSwitchingWeapons)
        {
            //if switch button pressed and not attacking or using ability, switch weapons
            //switching can only be cancelled by switching back to the other weapons
            ResetAnimationTriggers();

            m_isSwitchingWeapons = true;

            //toggle to other weapon type
            if (m_WeaponType == WeaponType.RANGED)
                m_WeaponType = WeaponType.MELEE;
            else
                m_WeaponType = WeaponType.RANGED;
        }
        else if ((CharacterController.GetFire2Held() == true || CharacterController.GetFire2PercentHeld() > 0) &&
            !CurrentlyInAnimation() && !m_Movement.IsSprinting)
        {
            //if ability button pressed and not attacking/using ability/switching weapons
            //abilities cannot be interrupted by anything
            ResetAnimationTriggers();

            //check which weapon is currently equipped and corresponding cooldown is ready
            if (m_WeaponType == WeaponType.RANGED && m_RangedAbilityTimer == float.MinValue)
            {
                RangedAbility();
            }
            else if (m_WeaponType == WeaponType.MELEE && m_MeleeAbilityTimer == float.MinValue)
            {
                MeleeAbility();
            }
        }
        else if ((CharacterController.GetFire1Held() == true || m_HasAttackBegun || CharacterController.GetFire1PercentHeld() > 0)
                 && !m_Movement.IsSprinting && !m_isSwitchingWeapons && !m_isUsingAbility)
        {
            //if attack button pressed and not attacking/using ability/switching weapons
            //attack cannot be interrupted by anything
            if (m_WeaponType == WeaponType.RANGED && AmmoAmount > 0 && !CurrentlyInAnimation())
            {
                RangedAttack();
            }
            else if (m_WeaponType == WeaponType.MELEE)
            {
                if (m_isUsingAbility)
                    EventFinishedAbility();

                MeleeAttack();
            }
        }
        else if (CharacterController.GetReloadDown() == true && m_WeaponType == WeaponType.RANGED &&
                AmmoAmount < MaxAmmo && !CurrentlyInAnimation())
        {
            //if not already in animation, equipped with the ranged weapon and ammo is not already full, reload weapon
            //reloading can be interrupted by other actions but you don't get the ammo
            m_IsReloading = true;
        }
    }

    protected override void Update()
    {
        //if enough time has passed then the swing stage should be reset to the basic stage
        if (m_TimeSinceSwordSlash >= 1.0f && m_CurrentAttackStages == SamuraiAttackStates.NULL)
            m_SwingStage = 0;
        else
            m_TimeSinceSwordSlash += Time.deltaTime;

        //if (ParryRiposteCollider.gameObject.activeInHierarchy && CharacterController.GetFire2Up())
        //    EventFinishedAbility();

        if (ParryRiposteCollider.gameObject.activeInHierarchy && photonView.isMine)
            m_ParryTimer -= Time.deltaTime;

        //TODO: Make the melee attack check for the player get overriden to do custom details from within the Samurai class itself because switching between sword states requires that we can check which state the sword is in
        base.Update();
    }

    //Primary attack that the character uses, such as pistol fire or katana
    protected void PrimaryAttack()
    {
        AttackRateTimer = 0.0f;
        m_BasicAttackActive = true;

        SoundManager.GetInstance().photonView.RPC("PlayRandomSFXNetworked", PhotonTargets.All, "SwordSwing", "SwordSwing2", "SwordSwing3", this.gameObject.transform.position);
    }

    protected override void PlayInjurySound()
    {
        SoundManager.PlayRandomSFX(this.gameObject.transform.position, "RoninInjury1", "RoninInjury2", "RoninInjury3");
    }

    protected override void PlayerDeathSound()
    {
        SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "RoninDying", this.gameObject.transform.position);
    }

    #region Ranged Attack
    protected override void RangedAttack()
    {
        for (int pellet = 0; pellet < 25; pellet++)
        {
            //calculate the normal distribution of the pellet
            float angleRadians = Random.Range(0.0f, 2.0f * Mathf.PI);
            Vector3 randomOffset = new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians), Mathf.Sin(angleRadians));
            randomOffset *= MathFunc.ApproximateNormalDistribution() * SpreadAmount;

            //calcualte the direction offset for the current pellet shot
            Vector3 directionOffset = new Vector3();
            directionOffset.x += randomOffset.x;
            directionOffset.y += randomOffset.y;
            directionOffset.z += randomOffset.z;

            directionOffset = m_FirstPersonCamera.transform.forward + m_FirstPersonCamera.transform.TransformDirection(directionOffset);

            photonView.RPC("FireSingleTargetMultipleHitscan", PhotonTargets.All, m_FirstPersonCamera.transform.position, directionOffset, (BoostedRangedDamage) * m_NumOfAttacks, PlayerNumber,
                                                                        m_Range, m_IgnoreMask, m_WeaponLayerMask, GunLocation.transform.position, pellet * 3, pellet * 3 + 1);
           
        }
        SoundManager.GetInstance().photonView.RPC("PlaySFXRandomizedPitchNetworked", PhotonTargets.All, "BoomstickShot", GunLocation.transform.position);

        ResetAnimationTriggers();
        m_IsAttacking = true;
    }

    #endregion

    #region Ranged Ability
    protected override void RangedAbility()
    {
        //calcualte the direction from the gun to the destination point
        Vector3 directionToDest = GunLocation.transform.forward - m_FirstPersonCamera.transform.forward;
        //get projectile from the pool of projectiles
        GameObject projectile = ObjectPoolManager.Instance.GetObjectFromNetworkPool(c_OverpenShotPoolName);

        if (projectile != null)
        {
            projectile.GetPhotonView().RPC(
                "FireProjectile",
                PhotonTargets.All,
                GunLocation.transform.position,
                m_FirstPersonCamera.transform.forward,
                PlayerNumber,
                m_WeaponLayerMask,
                BoostedRangedDamage * 5,
                null);
        }

        //RaycastHit[] objectsHit = Physics.SphereCastAll(GunLocation.transform.position, 5.0f, directionToDest, m_WeaponLayerMask);

        //if (objectsHit.Length != 0)
        //{
        //    List<AITakeDamageInterface> enemiesDamaged = new List<AITakeDamageInterface>();

        //    foreach (RaycastHit hit in objectsHit)
        //    {
        //        AITakeDamageInterface ai = hit.transform.root.GetComponent<AITakeDamageInterface>();

        //        if (ai != null)
        //        {
        //            if (!enemiesDamaged.Contains(ai))
        //            {
        //                ai.TakeDamage(PlayerNumber, m_Damage, null, AIUtilits.GetCritMultiplier(hit.collider.gameObject));
        //            }
        //        }
        //    }
        //}

        base.RangedAbility();
    }
    #endregion

    #region Melee Attack 
    protected override void MeleeAttack()
    {
        switch (m_CurrentAttackStages)
        {
            //uses the basic attack that the samurai has
            case SamuraiAttackStates.BasicAttack:
                BasicAttack();
                break;

            //uses the samurai's charge attack
            case SamuraiAttackStates.ChargeAttack:
                ChargeAttack();
                break;

            //choose which attack the samurai should use
            case SamuraiAttackStates.NULL:
                DetermineAttack();
                break;
        }
    }

    protected virtual void BasicAttack()
    {
        if (!m_SwordTrigger.enabled)
        {
            //photonView.RPC("SetSwordActive", PhotonTargets.All, true);
            SwordRef.GetComponent<MeleeCollider>().SetActive(true);
            m_HasAttackBegun = false;
        }
    }

    protected virtual void ChargeAttack()
    {
        m_ChargeTimer += Time.deltaTime;

        if (!CharacterController.GetFire1Held())
        {           
            //calculate the damage that the sword can deal during this sword attack
            int damageFromChargeAttack = (int)(MeleeDamage * (1 + (m_ChargeTimer / 4.0f)));
            m_TempDamageBoost += m_ChargeSwordDamage = damageFromChargeAttack - MeleeDamage;

            //set the sword collider to be activated
            //photonView.RPC("SetSwordActive", PhotonTargets.All, true);
            SwordRef.GetComponent<MeleeCollider>().SetActive(true);

            m_AnimatorRef.SetInteger("MeleeAttackStage", m_SwingStage);
            m_AnimatorRef.SetBool("IsCharging", false);

            m_HasAttackBegun = false;
            if (m_ChargeTimer >= 4.0f)
            {
                //create the beam for the sword slash
                SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "AirSlash", transform.position);
                CreateSwordBeam();
            }

            m_ChargeTimer = 0.0f;
        }
    }

    protected virtual void DetermineAttack()
    {
        m_HasAttackBegun = true;

        //handle if the player should move to the basic attack first
        if (!CharacterController.GetFire1Held())
        {
            if (m_SwingStage == 4)
                m_SwingStage = 0;

            m_SwingStage++;

            m_AnimatorRef.SetInteger("MeleeAttackStage", m_SwingStage);
            m_IsAttacking = true;
            m_CurrentAttackStages = SamuraiAttackStates.BasicAttack;
            return;
        }

        //determine if enough frames have passed for the charge attack to be used
        if (m_ChargeAttackBuffer > BufferTime)
        {
            if (m_SwingStage == 4)
                m_SwingStage = 0;

            m_SwingStage++;

            m_CurrentAttackStages = SamuraiAttackStates.ChargeAttack;
            m_IsAttacking = true;
            m_AnimatorRef.SetInteger("MeleeAttackStage", m_SwingStage);
            m_AnimatorRef.SetBool("IsCharging", true);
            m_ChargeAttackBuffer = 0;
        }
        else
        {
            m_ChargeAttackBuffer++;
        }
    }

    private void CreateSwordBeam()
    {
        RaycastHit hit;

        if (Physics.Raycast(m_FirstPersonCamera.transform.position, m_FirstPersonCamera.transform.forward, out hit, m_Range, m_IgnoreMask))
        {
            RangedAbilityTimer = 0.0f;

            //Grab explosive from pool
            GameObject projectile = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_BeamPoolName);

            if (BeamPrefab != null)
            {
                //calculate direction and fire projectile
                Vector3 direction = (hit.point - SwordRef.transform.position).normalized;
                projectile.GetPhotonView().RPC("FireProjectile", PhotonTargets.All, SwordRef.transform.position, direction, PlayerNumber, m_WeaponLayerMask, Mathf.RoundToInt(BoostedMeleeDamage * 1.25f), null);
            }
        }
    }

    public void EventCalculateNextAttack()
    {
        //remove any potential boosts from the temp damage boost value
        m_TempDamageBoost -= m_ChargeSwordDamage;
        //reset the charge sword damage
        m_ChargeSwordDamage = 0;

        m_CurrentAttackStages = SamuraiAttackStates.NULL;
    }

    public override void EventFinishedAttack()
    {
        if (WeaponType == WeaponType.MELEE)
        {
            EventCalculateNextAttack();

            m_TimeSinceSwordSlash = 0.0f;
            m_IsAttacking = false;
            m_AnimatorRef.SetBool("IsCharging", false);
            //photonView.RPC("SetSwordActive", PhotonTargets.All, false);
            SwordRef.GetComponent<MeleeCollider>().SetActive(false);
        }
        else
        {
            base.EventFinishedAttack();
            AmmoAmount--;
        }
    }

    private enum SamuraiAttackStates
    {
        BasicAttack,
        ChargeAttack,
        NULL
    }
    #endregion  

    #region Parry & Riposte Ability
    protected override void MeleeAbility()
    {
        if (CharacterController.GetFire2Held() && m_ParryTimer >= 0.0f && !m_WasAttacked)
        {
            if (ParryRiposteCollider.gameObject.activeInHierarchy == false)
            {
                photonView.RPC("SetParryCollider", PhotonTargets.All, true);
                m_isUsingAbility = true;
                Health.IsInvincible = true;
            }
        }
        else
        {
            //TESTING LOGIC TO MAKE SURE THAT THE PERRY LOGIC ACTUALLY WORKS
            //TODO: remove code below when the animations are in
            //if (m_ParryTimer <= -0.5f)
            //    EventFinishedAbility();
        }
    }

    public void ParryTarget(GameObject targetOBJ)
    {
        if (targetOBJ.tag == "Enemy")
        {
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "RiposteSound", transform.position);

            //get the AI component form the AI
            AI targetAI = targetOBJ.GetComponent<AI>();

            //apply force to the AI accross the network
            //targetAI.ApplyForceOverNetwork(transform.forward * 3.0f, ForceMode.VelocityChange);

            //calculate the damage that the AI would take 
            //((parry timer (amount of time in parry) * 50) + 50) * 0.01 gets a percentage between 0.5 and 1.5 then that value gets multiplied by the samurai's base damage to get the amount done to the enemy
            int damageDealt = (int)((((m_ParryTimer * 50) + 50) * 0.01) * MeleeDamage);
            TempDamageBoost += damageDealt;
            m_ParryDamageBoost = damageDealt;

            //deal damage to the AI accross the network
            //targetAI.photonView.RPC("TakeDamage", PhotonTargets.All, PlayerNumber, damageDealt, null, 1);
        }
        else
        {
            //get the object projectile that is going to be fired back at the enemy
            ObjectProjectile targetProjectile = targetOBJ.GetComponent<ObjectProjectile>();

            targetProjectile.IsBeingReflected = true;

            //set the force of the object to be zero
            targetProjectile.photonView.RPC(
                "FireProjectile",
                PhotonTargets.All,
                targetProjectile.transform.position,
                transform.forward,
                PlayerNumber,
                m_WeaponLayerMask,
                targetProjectile.Damage,
                null);
        }

        EventParryFinished();
    }

    public override void EventFinishedAbility()
    {
        if (m_ParryDamageBoost != 0.0f)
            m_TempDamageBoost -= m_ParryDamageBoost;
        m_ParryDamageBoost = 0;
        m_ParryTimer = 2.0f;
        ParryRiposteCollider.ParryMode = false;
        m_MeleeAbilityTimer = 0.0f;
        m_AnimatorRef.SetBool("IsRiposting", false);
        SwordRef.GetComponent<MeleeCollider>().SetActive(false);

        base.EventFinishedAbility();
    }

    public void EventParryFinished()
    {
        m_AnimatorRef.SetBool("IsRiposting", true);
        photonView.RPC("SetParryCollider", PhotonTargets.All, false);
        Health.IsInvincible = false;
        SwordRef.GetComponent<MeleeCollider>().SetActive(true);
    }
    #endregion

    #region RPCs
    [PunRPC]
    //TODO: remove this infavor of the RPC bellow this one
    private void ToggleCollider(bool flag)
    {
        SwordRef.GetComponentInChildren<BoxCollider>().enabled = flag;
    }

    [PunRPC]
    private void SetSwordActive(bool flag)
    {
        m_SwordTrigger.enabled = flag;
    }

    [PunRPC]
    private void SetParryCollider(bool flag)
    {
        ParryRiposteCollider.gameObject.SetActive(flag);
    }
    #endregion
}
