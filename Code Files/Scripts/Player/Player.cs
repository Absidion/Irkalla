using System;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Author: Josue
//Last edited: Josue 11/13/2017

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Movement))]

public abstract class Player : LiamBehaviour
{
    #region Public Members
    //Anything set to 99 is not a final stat
    public const int MAX_CHARACTER_DAMAGE = 99;
    public const float MAX_CHARACTER_LUCK = 100.0f;
    public const float MAX_CHARACTER_CDR = 0.5f;
    public const float MAX_CHARACTER_LIFESTEAL = 0.5f;
    public const int MAX_CHARACTER_ARMOR = 250;
    public const int MAX_CHARACTER_DEFENCE = 50;
    protected float m_MaxCrit = 2.0f;

    public Transform ActiveItemLocation;                   //The position at which the active item should be place for it's affect
    public GameObject EyeLocation;                         //player' eye location on the model
    public GameObject SpectatorPosition;                   //the position from which a spectator will view the player
    public GameObject GunLocation;                         //location where the gun visual effect shoots out from
    public GameObject MeleeWeapon;                         //reference to melee weapon model
    public GameObject RangedWeapon;                        //reference to ranged weapon model
    public Transform GunFlashLocation;                    //Reference to a transform where the gun Flash Paticle System will fire
    public Transform Chestbone;                            //reference to chest bone used to rotate player around
    public Journal Journal;                                //the player's inventory manager
    public BoxCollider DownedStateTrigger;                 //trigger box that gets activated when the player is downed so their teammate can revive them
    public float AttackRate = 0.5f;                        //time in between attacks
    public float RangedAbilityCooldown = 0.0f;             //ranged ability cooldown
    public float MeleeAbilityCooldown = 0.0f;              //ability two cooldown
    public float ReloadTime = 2.0f;                        //how long it takes to reload    
    public int AmmoAmount = 6;                             //current ammo amount of the player
    public int MaxAmmo = 6;                                //max amount of ammo the player carries
    #endregion

    #region Protected Memebers
    [SerializeField]
    protected int m_RangedDamage = 1;               //how much damage the player does with their basic attack
    [SerializeField]
    protected int m_MeleeDamage = 1;                //how much damage the player does with their basic attack
    [SerializeField]
    protected float m_Range = 5000.0f;              //max distance the raycast shoots out
    [SerializeField]
    protected float m_Luck = 0.0f;                  //affects multiple random properties to be more in the player's favor. e.g. item drops
    [SerializeField]
    protected float m_CooldownReduction = 0.0f;     //makes ability cooldown timers faster by a percentage
    [SerializeField]
    protected float m_LifeSteal = 0.0f;             //the percentage value of how much damage done converts into hp
    [SerializeField]
    protected int m_Currency = 0;                   //amount of currency for use in shops
    [SerializeField]
    protected float m_CritBonus = 0.0f;             //how much bonus damage gets done when hitting critical spots 
    [SerializeField]
    protected int m_ExtraLives = 0;                 //The number of extra lives that the player has

    protected Health m_Health;                      //handles all health/armor and damage calculations
    protected Movement m_Movement;                  //handles all movement code
    protected Camera m_FirstPersonCamera;           //reference to camera for raycasting projectiles    

    protected int m_WeaponLayerMask = 0;            //the layer that your weapon can collide with
    protected int m_IgnoreMask = 0;                 //the mask that we want to ignore
    protected int m_TempDamageBoost = 0;            //non-permanent damage boosts applied to player
    protected IslandRoom m_MyIslandRoom;            //the island room that the player is in
    [SyncThis]
    [SerializeField]
    protected WeaponType m_WeaponType = WeaponType.RANGED;    //current equipped weapon type

    protected Animator m_AnimatorRef = null;        //reference to player animator
    protected bool m_isSwitchingWeapons = false;    //if true, player is currently in their switching weapons animation
    protected bool m_isUsingAbility = false;        //if true, player is currently in their ability animation
    protected bool m_IsAttacking = false;           //if true, player is currently in their basic attack animation
    protected bool m_IsReloading = false;           //if true, player is currently in their reload animation
    [SyncThis]
    [SerializeField]
    protected bool m_CanLootTreasureRoom = true;    //Determines if the player has already obtained an item from the treasure room on this floor
    protected float m_CharacterHalfHeight = 0.0f;   //The character's half height relative to the collider
    [SerializeField]
    protected int m_NumOfAttacks = 1;                   //Number of times the player can attack.
    #endregion

    #region Private Members
    private float m_PitchChange;                    //the change in pitch this frame
    private Quaternion m_GayQuaternion;
    private Quaternion m_StraightQuaternion;

    private string m_RangedAbilityName;                         //Strings for the names of the Abilitys, these are used to tell the UI which sprites to spawn on screen
    private string m_MeleeAbilityName;
    private string m_GunFlashParticlePoolName = "GunFlashParticlePool";

    protected float m_RangedAbilityTimer = float.MinValue;    //timer that increments when this ability is on cooldown
    protected float m_MeleeAbilityTimer = float.MinValue;   //timer that increments when this ability is on cooldown
    private float m_AttackRateTimer = float.MinValue;       //timer that increments in between attacks
    private Dictionary<Status, float> m_Effects;            //status effects the player is currently afflicted by
    private Dictionary<Status, GameObject> m_WorldEffects;  //gameobjects that contain shaders/particle systems for statuses that get attached to the player
    private List<Status> m_StatusesToBeRemoved;             //if statuses are timers are up, they get stored in this list to be removed
    private bool m_IsImmobile = false;                      //if true, dont update any abilities or movement
    private float m_OldCooldownReduction = 0.0f;            //used to check when cooldown reduction has been changed

    //private int m_DownedStateCounters = 3;                  //amount of times player can go down and be revived. HP in downed state is equal to amount of counters * 10
    [SyncThis]
    protected bool m_IsDowned = false;                        //if true, don't update any attacks or movement but allow camera rotation
    private float m_DownedStateTickTimer = 0.0f;            //timer that increments and deals damage every second while downed
    private bool m_IsReviving = false;                      //true while the other player is holding reive while downed
    [SyncThis]
    protected float m_ReviveTimer = 0.0f;                     //timer increments while other player is reviving this one

    private bool m_HasDeathLogicBeenCompleted = false;      //if the death logic has been done yet over the network

    private bool m_EyeIsShaking = false;                    //Checking if the EyeLocation is already shaking so it doesn't attempt to shake again while already shaking

    [SerializeField]
    private int m_PlayerNumber = -1;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        m_Health = GetComponent<Health>();
        m_Movement = GetComponent<Movement>();
        Journal = new Journal(this);
        m_Effects = new Dictionary<Status, float>();
        m_WorldEffects = new Dictionary<Status, GameObject>();
        m_StatusesToBeRemoved = new List<Status>();
        m_AnimatorRef = GetComponent<Animator>();
        if (RangedWeapon != null)
            RangedWeapon.SetActive(true);
        GameManager.Instance.DontDestroyNetworkObject(gameObject);

        m_WeaponLayerMask = LayerMask.GetMask("Enemy");
        m_IgnoreMask = ~LayerMask.GetMask("Room", "Player");

        if (!photonView.isMine)
            return;

        rigidbody.useGravity = true;
        m_Health.ReviveCounters = 3;

        //call movement's init function to set up the required values
        m_Movement.Init();
        if (m_FirstPersonCamera == null)
        {
            m_FirstPersonCamera = Camera.main;

            FirstPersonCamera cam = Camera.main.GetComponent<FirstPersonCamera>();
            if (cam == null)
            {
                Debug.LogError("Camera is missing script FirstPersonCamera, please add that script to the Main Camera");
            }
            else
            {
                cam.SetPlayer(this);
            }
        }
        GameManager.PostPlayer += OnPlayerCreated;
        SceneManager.sceneLoaded += OnAdvanceToNextLevel;
        //SceneManager.sceneUnloaded += SceneUnloaded;
        //ObjectPoolManager.Instance.CreateNetworkPoolWithName("GunFlashParticlePool", "Effects/GunFlashParticle", true);

        m_CharacterHalfHeight = GetComponent<CapsuleCollider>().height / 2;

        m_StraightQuaternion = Chestbone.rotation;
        m_GayQuaternion = Chestbone.rotation;
    }

    private void OnPlayerCreated(object scene, EventArgs mode)
    {
        if (photonView.isMine)
        {
            FirstPersonCamera cam = Camera.main.GetComponent<FirstPersonCamera>();
            if (cam == null)
            {
                Debug.LogError("Camera is missing script FirstPersonCamera, please add that script to the Main Camera");
            }
            else
            {
                cam.SetPlayer(this);
            }

            m_FirstPersonCamera = Camera.main;

            FirstPersonCamera[] cameras = FindObjectsOfType<FirstPersonCamera>();
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i].transform.root.tag == "MainCamera")
                    cameras[i].gameObject.SetActive(false);
            }
        }

        Status[] statuses = { Status.Hitstun };
        MeleeWeapon.GetComponent<MeleeCollider>().Init(PlayerNumber, MeleeDamage, statuses);
    }

    private void OnAdvanceToNextLevel(Scene scene, LoadSceneMode sceneMode)
    {
        m_CanLootTreasureRoom = true;
    }

    private void SceneUnloaded(Scene scene)
    {
        if (m_FirstPersonCamera != null)
            Destroy(m_FirstPersonCamera.gameObject);
    }

    protected override void Update()
    {
        base.Update();

        if (!photonView.isMine || m_Health.IsDead)
            return;


        if (IsImmobile != true && !InGameMenuNavigator.IsGamePaused) //if the player isn't stunned
        {
            UpdateRotation();

            if (IsDowned != true) //if the player isn't downed
                AttackInputCheck();
        }

        if (!IsDowned)
            CheckDownedState();

        if (IsDowned)
            UpdateDownedState();

        if (InGameMenuNavigator.IsGamePaused || IsImmobile == true)
            m_PitchChange = 0.0f;
    }

    protected virtual void FixedUpdate()
    {
        if (!photonView.isMine || m_Health.IsDead)
            return;

        if (IsImmobile != true && IsDowned != true && !InGameMenuNavigator.IsGamePaused) //if the player isn't stunned or downed
            UpdateMovement();
    }

    protected virtual void LateUpdate()
    {
        if (!photonView.isMine)
            return;

        UpdateCooldowns();
        UpdateStatusTimers();

        if (m_Health.IsDead && !m_HasDeathLogicBeenCompleted)
        {
            UpdateDeadPlayer();
        }

        if (m_AnimatorRef == null)
            return;

        UpdateChestRotation();
        UpdatePlayerAnimations();
    }

    //Updates and rotates chest with mouselook
    private void UpdateChestRotation()
    {
        Vector3 rot = transform.rotation.eulerAngles - m_GayQuaternion.eulerAngles - m_StraightQuaternion.eulerAngles;
        rot.z = -m_PitchChange;

        m_GayQuaternion = Quaternion.Euler(m_GayQuaternion.eulerAngles + rot);
        m_GayQuaternion = Chestbone.rotation = MathFunc.ClampQuaternionZRotation(m_GayQuaternion, 200, 340);
    }

    //Calculates all player movement operations
    protected void UpdateMovement()
    {
        Vector3 movement = CharacterController.GetMovementThisFrame();
        m_Movement.UpdatePosition(movement, CharacterController.GetJumpActive(), CharacterController.GetSprintHeld());
    }

    //Calculates all player rotation operations
    protected void UpdateRotation()
    {
        Vector3 euler = CharacterController.GetRotationThisFrame().eulerAngles;
        m_PitchChange = euler.x;
        euler.x = 0.0f;
        Quaternion quat = Quaternion.Euler(euler);
        m_Movement.UpdateRotation(quat);
    }

    //Update cooldowns such as firing rate, reload times and ability cooldowns
    protected virtual void UpdateCooldowns()
    {
        //if CDR value has changed
        if (m_OldCooldownReduction != m_CooldownReduction)
        {
            //if player has more CDR than before
            if (m_OldCooldownReduction < m_CooldownReduction)
            {
                //get the difference of CDR and reduce the cooldown by it
                float diff = m_CooldownReduction - m_OldCooldownReduction;
                RangedAbilityCooldown -= RangedAbilityCooldown * diff;
                MeleeAbilityCooldown -= MeleeAbilityCooldown * diff;
            }
            else //if player has less CDR than before
            {
                //get the difference of CDR and increase the cooldown by it
                float diff = m_OldCooldownReduction - m_CooldownReduction;
                RangedAbilityCooldown += RangedAbilityCooldown * diff;
                MeleeAbilityCooldown += MeleeAbilityCooldown * diff;
            }

            m_OldCooldownReduction = m_CooldownReduction;
        }

        //Ranged ability cooldown update
        if (m_RangedAbilityTimer != float.MinValue)
        {
            m_RangedAbilityTimer += Time.deltaTime;

            if (m_RangedAbilityTimer >= RangedAbilityCooldown)
            {
                m_RangedAbilityTimer = float.MinValue;
            }
        }

        //Second ability cooldown update
        if (m_MeleeAbilityTimer != float.MinValue)
        {
            m_MeleeAbilityTimer += Time.deltaTime;

            if (m_MeleeAbilityTimer >= MeleeAbilityCooldown)
            {
                m_MeleeAbilityTimer = float.MinValue;
            }
        }

        //when player runs out of ammo, start the reload animation
        if (m_WeaponType == WeaponType.RANGED && AmmoAmount <= 0)
        {
            m_IsReloading = true;
        }
    }

    //if in downed state, take damage every second
    private void UpdateDownedState()
    {
        //while reviving, keep increasing timer until fully revived
        if (IsReviving)
        {
            PlayerHUDManager.instance.ToggleReviveTimer(true);

            ReviveTimer += Time.deltaTime;

            if (ReviveTimer >= 5.0f)
            {
                m_Health.ChangeHp(30); //when revived, start back at 30 hp
                photonView.RPC("ResetDownedState", PhotonTargets.All);
            }

            return; //return so you don't take tick damage while being revived
        }

        m_DownedStateTickTimer += Time.deltaTime;

        //if one second has passed while in downed state, tick down HP by 1
        if (m_DownedStateTickTimer >= 1.0f)
        {
            m_DownedStateTickTimer = 0.0f;
            m_Health.TakeDamage(1);

            //you died so reset downed state
            if (m_Health.HP <= 0)
            {
                m_Health.IsDead = true;
                photonView.RPC("ResetDownedState", PhotonTargets.All);
            }
        }
    }

    //trigger only gets activated during downed state
    private void OnTriggerStay(Collider other)
    {
        Player p = other.GetComponent<Player>();

        if (p != null)
        {
            //if the collider hitting this trigger is owned by me and this player component is not owned by me then this is the other player
            if (p.photonView.isMine && !photonView.isMine && p.IsDowned == false)
            {
                PlayerHUDManager.instance.ToggleInteractImage(true);

                //if the other player is holding interact and aren't reviving already, start reviving
                if (CharacterController.GetInteractHeld() && !IsReviving)
                {
                    photonView.RPC("SetIsReviving", PhotonTargets.All, true);
                    PlayerHUDManager.instance.ToggleReviveTimer(true);
                }
                //if they are already reviving and release interact, stop reviving
                else if (!CharacterController.GetInteractHeld() && IsReviving)
                {
                    photonView.RPC("SetIsReviving", PhotonTargets.All, false);
                    PlayerHUDManager.instance.ToggleReviveTimer(false);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player p = other.GetComponent<Player>();

        if (p != null)
        {
            //if the collider hitting this trigger is owned by me and this player component is not owned by me then this is the other player
            if (p.photonView.isMine && !photonView.isMine && p.IsDowned == false)
            {
                PlayerHUDManager.instance.ToggleInteractImage(false);
                PlayerHUDManager.instance.ToggleReviveTimer(false);

                photonView.RPC("SetIsReviving", PhotonTargets.All, false); //stop reviving when exiting trigger
            }
        }
    }

    //base function for ranged attack that can be overridden for unique behaviour
    protected virtual void RangedAttack()
    {
        //Play the gunshot flash   
        GameObject FlashParticleObject = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_GunFlashParticlePoolName);
        if (FlashParticleObject != null)
        {
            FlashParticleObject.SetActive(true);
            ParticleSystemController particleController = FlashParticleObject.GetComponent<ParticleSystemController>();

            if (GunFlashLocation != null)
                particleController.FireFromPositionNetworked(GunFlashLocation.position);

        }
        ResetAnimationTriggers();
        m_IsAttacking = true;
        AmmoAmount--;
    }

    //base function for melee attack that can be overridden for unique behaviour
    protected virtual void MeleeAttack()
    {
        ResetAnimationTriggers();
        m_IsAttacking = true;
    }

    //base function for ranged ability that can be overridden for unique behaviour
    protected virtual void RangedAbility()
    {
        m_isUsingAbility = true;
        m_RangedAbilityTimer = 0.0f;
    }

    //base function for melee ability that can be overriden for unique behaviour
    protected virtual void MeleeAbility()
    {
        m_isUsingAbility = true;
        m_MeleeAbilityTimer = 0.0f;
    }

    //Checks for any attack inputs such as the primary fire and secondary abilities
    protected virtual void AttackInputCheck()
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
        else if (((CharacterController.GetSwitchHeld() && CharacterController.IsUsingController == true) ||

            (CharacterController.GetSwitchPercentHeld() != 0) && CharacterController.IsUsingController == false)

            &&
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
        else if ((CharacterController.GetFire1Held() == true || CharacterController.GetFire1PercentHeld() > 0) &&
                !CurrentlyInAnimation() && !m_Movement.IsSprinting)
        {
            //if attack button pressed and not attacking/using ability/switching weapons
            //attack cannot be interrupted by anything
            if (m_WeaponType == WeaponType.RANGED && AmmoAmount > 0)
            {
                RangedAttack();
            }
            else if (m_WeaponType == WeaponType.MELEE)
            {
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

    #region Player Death Logic
    public void UpdateDeadPlayer()
    {
        PlayerDeathSound();
        photonView.RPC("TellOtherPlayerIAmDead", PhotonTargets.Others);
        photonView.RPC("KillPlayerOverNetwork", PhotonTargets.All);
        PlayerManager.Instance.OnPlayerDeath(PlayerNumber);
        m_HasDeathLogicBeenCompleted = true;
    }

    [PunRPC]
    public void KillPlayerOverNetwork()
    {
        DisableMeshes(transform);

        GetComponent<CapsuleCollider>().enabled = false;
        GetComponent<Rigidbody>().useGravity = false;
        MyIslandRoom.PlayersInRoom.Remove(this);
        MyIslandRoom = null;

    }

    private void DisableMeshes(Transform currentIteration)
    {
        for (int i = 0; i < currentIteration.childCount; i++)
        {
            if (currentIteration.GetChild(i).childCount >= 1)
            {
                DisableMeshes(currentIteration.GetChild(i));
            }

            Renderer r = currentIteration.GetChild(i).GetComponent<Renderer>();
            if (r != null)
            {
                Debug.Log("Mesh with name " + currentIteration.GetChild(i).name + " has been disabled.");
                r.enabled = false;
            }
        }
    }

    [PunRPC]
    public void TellOtherPlayerIAmDead()
    {
        PlayerHUDManager.instance.ToggleDeadNotication();
    }

    public void ResurrectPlayer()
    {
        photonView.RPC("RPCRespawnPlayer", PhotonTargets.All);
    }

    [PunRPC]
    private void RPCRespawnPlayer()
    {
        m_Health.ResetHealth();
        GetComponentInChildren<MeshRenderer>().enabled = true;
        GetComponent<CapsuleCollider>().enabled = true;
        GetComponent<Rigidbody>().useGravity = true;
    }
    #endregion

    #region Player Statistics Management
    //Increase the stat specified by the amount given
    public void IncreaseStat(StatType type, float amount)
    {
        if (!photonView.isMine)
            return;

        switch (type)
        {
            case StatType.DAMAGE:
                m_RangedDamage += Mathf.RoundToInt(amount);
                m_RangedDamage = Mathf.Clamp(m_RangedDamage, 0, MAX_CHARACTER_DAMAGE);
                JournalUIManager.instance.ChangeStatValue(type, m_RangedDamage, MAX_CHARACTER_DAMAGE);
                m_MeleeDamage += Mathf.RoundToInt(amount);
                m_MeleeDamage = Mathf.Clamp(m_MeleeDamage, 0, MAX_CHARACTER_DAMAGE);
                JournalUIManager.instance.ChangeStatValue(type, m_MeleeDamage, MAX_CHARACTER_DAMAGE);

                break;
            case StatType.LUCK:
                m_Luck += amount;
                m_Luck = Mathf.Clamp(m_Luck, 0.0f, MAX_CHARACTER_LUCK);
                JournalUIManager.instance.ChangeStatValue(type, m_Luck, MAX_CHARACTER_LUCK);
                break;
            case StatType.CDR:
                m_CooldownReduction += amount;
                m_CooldownReduction = Mathf.Clamp(m_CooldownReduction, 0.0f, MAX_CHARACTER_CDR);
                JournalUIManager.instance.ChangeStatValue(type, m_CooldownReduction, MAX_CHARACTER_DAMAGE);
                break;
            case StatType.LIFESTEAL:
                m_LifeSteal += amount;
                m_LifeSteal = Mathf.Clamp(m_LifeSteal, 0.0f, MAX_CHARACTER_LIFESTEAL);
                JournalUIManager.instance.ChangeStatValue(type, m_LifeSteal, MAX_CHARACTER_LIFESTEAL);
                break;
            case StatType.ARMOR:
                m_Health.ArmorHP += Mathf.RoundToInt(amount);
                m_Health.ArmorHP = Mathf.Clamp(m_Health.ArmorHP, 0, MAX_CHARACTER_ARMOR);
                break;
            case StatType.DEFENSE:
                m_Health.m_Defence += Mathf.RoundToInt(amount);
                m_Health.m_Defence = Mathf.Clamp(m_Health.m_Defence, 0, MAX_CHARACTER_DEFENCE);
                JournalUIManager.instance.ChangeStatValue(type, m_Health.m_Defence, MAX_CHARACTER_DEFENCE);
                break;
            case StatType.MOVEMENT_SPEED:
                m_Movement.Speed += amount;
                m_Movement.Speed = Mathf.Clamp(m_Movement.Speed, 0.0f, m_Movement.MaxSpeed);
                JournalUIManager.instance.ChangeStatValue(type, m_Movement.Speed, m_Movement.MaxSpeed);
                break;
            case StatType.CRIT:
                m_CritBonus += amount;
                m_CritBonus = Mathf.Clamp(m_CritBonus, 0.0f, m_MaxCrit);
                break;
            case StatType.EXTRALIFE:
                m_ExtraLives += (int)amount;
                break;
            default:
                break;
        }
    }

    //Increase currency by the amount given
    public void ChangeCurrency(int amount)
    {
        m_Currency += amount;
    }

    //Return the amount of currency you currently have
    public int GetCurrency()
    {
        return m_Currency;
    }

    //Return the stat specified
    public float GetStat(StatType type)
    {
        switch (type)
        {
            case StatType.DAMAGE:
                return m_MeleeDamage;
            case StatType.LUCK:
                return m_Luck;
            case StatType.CDR:
                return m_CooldownReduction;
            case StatType.LIFESTEAL:
                return m_LifeSteal;
            case StatType.ARMOR:
                return m_Health.ArmorHP;
            case StatType.DEFENSE:
                return m_Health.m_Defence;
            case StatType.MOVEMENT_SPEED:
                return m_Movement.Speed;
            case StatType.CRIT:
                return m_CritBonus;
            case StatType.EXTRALIFE:
                return m_ExtraLives;
            default:
                return -1;
        }
    }
    #endregion

    #region Statuses
    //Add or refresh a status effect in the player's status dictionary
    public void AddStatusEffect(Status effect)
    {
        //If the player is invinvible dont add the effect
        if (m_Health.IsInvincible == true)
        {
            return;
        }

        //if the status effect is already on the player, reset the timer
        if (m_Effects.ContainsKey(effect))
        {
            m_Effects[effect] = 0.0f;
        }
        else
        {
            m_Effects.Add(effect, 0.0f);

            //add to damage per second, amount slowed and change if player is immobile depending on the effect 
            m_Health.StatusDamagePerSecond += StatusManager.GetDOTFromStatus(effect);
            m_Movement.AmountSlowed += StatusManager.GetSlowValuesFromStatus(effect);
            IsImmobile = StatusManager.GetIsImmobalizedFromStatus(effect);

            //add world effect to player depending on the effect
            //GameObject worldEffect = StatusManager.GetWorldEffectFromStatus(effect);

            //if (worldEffect != null)
            //{
            //    worldEffect.transform.parent = gameObject.transform;            //attach the world effect to the player
            //    worldEffect.transform.localPosition = new Vector3(0.0f, 0.0f, 0.5f);
            //    worldEffect.transform.rotation = gameObject.transform.rotation;
            //    worldEffect.transform.Rotate(new Vector3(90, 0, 0));
            //    worldEffect.SetActive(true);                                    //set to active to enable effect
            //    m_WorldEffects.Add(effect, worldEffect);                        //keep a reference to the world effect to remove it later
            //}
        }

        StatusManager.SetStatusShader(effect);
    }

    [PunRPC]
    public void TakeDamage(int damage, Vector3 PosOfAttack, Status[] statusEffects)
    {
        if (IsDowned) //if in downed state, you can't take damage from enemies
            return;

        m_Health.TakeDamage(damage);
        //if (PhotonNetwork.player.IsLocal && !m_EyeIsShaking)
        //{
        //    StartCoroutine(Shake(0.3f, 0.7f));
        //}

        PlayInjurySound();

        if (PhotonNetwork.room.PlayerCount > 1)
        {
            //downed state trigger logic
            if (m_Health.HP <= 1 && m_Health.ReviveCounters != 0)
            {
                ActivateDownedState();
                return;
            }
        }

        if (photonView.isMine)
        { //Math done for getting the hit direction angle
            Vector3 heading = new Vector3(PosOfAttack.x - transform.position.x, 0, PosOfAttack.z - transform.position.z);
            heading.Normalize();
            transform.InverseTransformPoint(heading);
            float ForwardAngle = Mathf.Atan2(transform.forward.z, transform.forward.x) / Mathf.PI * 180;
            float angle = (Mathf.Atan2(heading.z, heading.x) / Mathf.PI) * 180;
            angle -= ForwardAngle;
            if (angle < 0)
                angle += 360.0f;
            StartCoroutine(PlayerHUDManager.instance.HitDirectionActive(angle));
        }

        if (statusEffects != null)
        {
            for (int i = 0; i < statusEffects.Length; i++)
            {
                AddStatusEffect(statusEffects[i]);
            }
        }
    }

    //Update the status timers and pop them off when done
    private void UpdateStatusTimers()
    {
        //clear the list of statuses to be removed if there is any in the list
        if (m_StatusesToBeRemoved.Count != 0)
            m_StatusesToBeRemoved.Clear();

        foreach (Status key in m_Effects.Keys.ToList())
        {
            m_Effects[key] += Time.deltaTime;

            //if the timer has passed the time set, add it to be removed
            if (m_Effects[key] >= StatusManager.GetTimerFromStatus(key))
            {
                //subtract the status amount from relevant values
                m_Health.StatusDamagePerSecond -= StatusManager.GetDOTFromStatus(key);
                m_Movement.AmountSlowed -= StatusManager.GetSlowValuesFromStatus(key);

                //if it was a stun effect, set the player to not be stunned anymore
                if (StatusManager.GetIsImmobalizedFromStatus(key) == true)
                {
                    IsImmobile = false;
                }

                //add status to be removed
                m_StatusesToBeRemoved.Add(key);
            }
        }

        //Remove all statuses marked for deletion
        for (int i = 0; i < m_StatusesToBeRemoved.Count; i++)
        {
            ////unparent world effect from the player and return it to the pool
            //if (m_WorldEffects.Count > 0)
            //{
            //    m_WorldEffects[m_StatusesToBeRemoved[i]].SetActive(false);
            //    m_WorldEffects[m_StatusesToBeRemoved[i]].transform.parent = null;
            //    m_WorldEffects.Remove(m_StatusesToBeRemoved[i]);
            //}

            //remove status effect
            m_Effects.Remove(m_StatusesToBeRemoved[i]);
        }
    }

    private void ActivateDownedState()
    {
        if (photonView.isMine)
        {
            PlayerHUDManager.instance.ToggleDownedWindow(true);             //toggle downed window if your player was downed
            ClearStatuses();
        }

        IsDowned = true;
        m_Health.ChangeHp(m_Health.ReviveCounters * 10);                        //set hp to be amount of downed counters * 10
        m_Health.ReviveCounters--;                                              //remove a downed counter
        SetIsKinematic(true);
        photonView.RPC("SetReviveTriggerActive", PhotonTargets.All, true);      //activate trigger box so other player can revive
        photonView.RPC("NotifyOtherPlayerOfDownedState", PhotonTargets.Others);
        m_Health.IsDead = false;
    }

    public void ClearStatuses()
    {
        foreach (Status key in m_Effects.Keys.ToList())
        {
            m_Health.StatusDamagePerSecond -= StatusManager.GetDOTFromStatus(key);
            m_Movement.AmountSlowed -= StatusManager.GetSlowValuesFromStatus(key);

            //if it was a stun effect, set the player to not be stunned anymore
            if (StatusManager.GetIsImmobalizedFromStatus(key) == true)
            {
                IsImmobile = false;
            }

            //add status to be removed
            m_StatusesToBeRemoved.Add(key);

            for (int i = 0; i < m_StatusesToBeRemoved.Count; i++)
            {
                m_Effects.Remove(m_StatusesToBeRemoved[i]);
            }
        }
    }

    private void CheckDownedState()
    {
        //downed state trigger logic
        if (m_Health.HP <= 1 && m_Health.ReviveCounters != 0)
        {
            if (PhotonNetwork.room.PlayerCount > 1)
            {
                ActivateDownedState();
            }
            else
            {
                m_Health.IsDead = true;
            }
        }
    }

    public IEnumerator Shake(float decay, float magnitude)
    {
        m_EyeIsShaking = true;
        Vector3 originalPos = EyeLocation.transform.localPosition;
        Quaternion originalRot = EyeLocation.transform.localRotation;

        while (magnitude > 0)
        {

            //elapsed += Time.deltaTime;

            //float percentComplete = (elapsed / duration);
            //float damper = 1.0f - Mathf.Clamp(2.0f * percentComplete - 2.0f, 0.0f, 1.0f);

            //float x = UnityEngine.Random.insideUnitCircle.x * 0.5f;
            //float y = UnityEngine.Random.insideUnitCircle.y * 0.5f;

            //x *= magnitude * damper;
            //y *= magnitude * damper;

            //EyeLocation.transform.localPosition = new Vector3(x, y, originalPos.z);

            EyeLocation.transform.localPosition = originalPos + UnityEngine.Random.insideUnitSphere * magnitude;
            EyeLocation.transform.localRotation = new Quaternion(
                originalRot.x + UnityEngine.Random.Range(-magnitude, magnitude) * .05f,
                originalRot.y + UnityEngine.Random.Range(-magnitude, magnitude) * .05f,
                originalRot.z + UnityEngine.Random.Range(-magnitude, magnitude) * .05f,
                originalRot.w + UnityEngine.Random.Range(-magnitude, magnitude) * .05f);

            magnitude -= decay;

            yield return null;

        }

        EyeLocation.transform.localPosition = originalPos;
        EyeLocation.transform.localRotation = originalRot;
        m_EyeIsShaking = false;
    }


    #endregion

    #region Abstracts
    //Primary attack that the character uses, such as pistol fire or katana
    protected abstract void PlayInjurySound();

    protected abstract void PlayerDeathSound();
    #endregion

    #region Animations
    //Update all animation properties related to the player actions
    protected virtual void UpdatePlayerAnimations()
    {
        //set the current equipped weapon to decide which type of attack/ability to use
        m_AnimatorRef.SetBool("IsRangedOrMelee", Convert.ToBoolean((int)m_WeaponType));

        //if true, start reload animation
        m_AnimatorRef.SetBool("IsReloading", m_IsReloading);

        //if true, start ability animation based on current weapon type
        m_AnimatorRef.SetBool("IsUsingAbility", m_isUsingAbility);

        //if true, start attacking animation based on current weapon type
        m_AnimatorRef.SetBool("IsAttacking", m_IsAttacking);
    }

    //Update isUsingAnimation boolean
    protected virtual bool CurrentlyInAnimation()
    {
        return m_IsReloading || m_IsAttacking || m_isUsingAbility || m_isSwitchingWeapons;
    }

    //Reset all animation triggers 
    protected virtual void ResetAnimationTriggers()
    {
        m_IsReloading = false;
        m_IsAttacking = false;
        m_isUsingAbility = false;
        m_isSwitchingWeapons = false;
    }

    //Event gets triggered when player is done reloading
    public virtual void EventFinishedReloading()
    {
        m_IsReloading = false;
        AmmoAmount = MaxAmmo;
    }

    //Event gets triggered when player is done ability animation
    public virtual void EventFinishedAbility()
    {
        m_isUsingAbility = false;
    }

    //Event gets triggered when player is done fire animation
    public virtual void EventFinishedAttack()
    {
        m_IsAttacking = false;
    }

    //Event gets triggered when player
    public virtual void EventFinishedSwitching()
    {
        m_isSwitchingWeapons = false;
    }

    public virtual void EventFinishedPuttingAway()
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
    #endregion

    #region Properties
    public Health Health { get { return m_Health; } }
    public float PitchChange { get { return m_PitchChange; } }
    public string RangedAbilityName { get { return m_RangedAbilityName; } set { m_RangedAbilityName = value; } }
    public string MeleeAbilityName { get { return m_MeleeAbilityName; } set { m_MeleeAbilityName = value; } }
    public WeaponType WeaponType { get { return m_WeaponType; } set { m_WeaponType = value; } }
    public int TempDamageBoost { get { return m_TempDamageBoost; } set { m_TempDamageBoost = value; } }
    public int PlayerNumber { get { return m_PlayerNumber; } set { m_PlayerNumber = value; } }
    public int WeaponLayerMask { get { return m_WeaponLayerMask; } }
    public int MeleeDamage { get { return m_MeleeDamage; } set { m_MeleeDamage = value; } }
    public int BoostedMeleeDamage { get { return Mathf.RoundToInt(m_MeleeDamage + m_TempDamageBoost); } } //property returns base damage as well as damage boost
    public int RangedDamage { get { return m_RangedDamage; } set { m_RangedDamage = value; } }
    public int BoostedRangedDamage { get { return Mathf.RoundToInt(m_RangedDamage + m_TempDamageBoost); } }
    public float LifeSteal { get { return m_LifeSteal; } set { m_LifeSteal = value; } }
    public IslandRoom MyIslandRoom { get { return m_MyIslandRoom; } set { m_MyIslandRoom = value; } }
    public float RangedAbilityTimer { get { return m_RangedAbilityTimer; } set { m_RangedAbilityTimer = value; } }
    public float MeleeAbilityTimer { get { return m_MeleeAbilityTimer; } set { m_MeleeAbilityTimer = value; } }
    public float AttackRateTimer { get { return m_AttackRateTimer; } set { m_AttackRateTimer = value; } }
    public Dictionary<Status, float> Effects { get { return m_Effects; } }
    public float ReviveTimer { get { return m_ReviveTimer; } set { m_ReviveTimer = value; } }
    public float HalfHeight { get { return m_CharacterHalfHeight; } }
    public bool IsReviving { get { return m_IsReviving; } set { m_IsReviving = value; } }
    public bool IsImmobile { get { return m_IsImmobile; } set { m_IsImmobile = value; } }
    public bool IsDowned { get { return m_IsDowned; } set { m_IsDowned = value; } }
    public bool CanLootTreasureRoom { get { return m_CanLootTreasureRoom; } set { m_CanLootTreasureRoom = value; } }
    public bool CanBeTargeted { get { return !m_Health.IsDead && !m_IsDowned; } }
    public int NumOfAttacks { get { return m_NumOfAttacks; } set { m_NumOfAttacks = value; } }
    public bool IsAttacking { get { return m_IsAttacking; } }
    public bool IsUsingAbility { get { return m_isUsingAbility; } }
    public Movement PlayerMovement { get { return m_Movement; } }
    #endregion

    #region Photon/Network Logic

    [PunRPC]
    public void SyncPlayerNumber(int playerNumber)
    {
        m_PlayerNumber = playerNumber;
    }

    [PunRPC]
    public void SetWeaponsActive(bool rangedWeaponStatus, bool meleeWeaponStatus)
    {
        RangedWeapon.SetActive(rangedWeaponStatus);
        MeleeWeapon.SetActive(meleeWeaponStatus);
    }

    [PunRPC]
    public void SetMeleeWeaponTrigger(bool flag)
    {
        MeleeWeapon.GetComponent<MeleeCollider>().SetActive(flag);
    }

    [PunRPC]
    public void SetReviveTriggerActive(bool flag)
    {
        DownedStateTrigger.enabled = flag;

    }

    [PunRPC]
    public void NotifyOtherPlayerOfDownedState()
    {
        PlayerHUDManager.instance.ToggleDownedNotication();
    }

    [PunRPC]
    public void SetIsReviving(bool flag)
    {
        IsReviving = flag;
    }

    //gets called when either the player is revived or they die before doing so
    [PunRPC]
    public void ResetDownedState()
    {
        PlayerHUDManager.instance.ToggleReviveTimer(false);
        PlayerHUDManager.instance.ToggleDownedWindow(false);
        PlayerHUDManager.instance.ToggleInteractImage(false);
        IsDowned = false;
        IsReviving = false;
        m_DownedStateTickTimer = 0.0f;
        ReviveTimer = 0.0f;
        DownedStateTrigger.enabled = false;
        rigidbody.isKinematic = false;
    }

    [PunRPC]
    public void ActivateHitMarker(int playernumber)
    {
        if (PhotonNetwork.player.ID == playernumber)
        {
            StartCoroutine(PlayerHUDManager.instance.HitMarkerActive());
        }
    }

    [PunRPC]
    public void SetIsImmobile(bool falg)
    {
        m_IsImmobile = falg;
    }
    #endregion
}

public enum StatType
{
    DAMAGE,
    LUCK,
    CDR,
    LIFESTEAL,
    ARMOR,
    DEFENSE,
    MOVEMENT_SPEED,
    CRIT,
    EXTRALIFE
};

public enum WeaponType
{
    MELEE,
    RANGED
};

