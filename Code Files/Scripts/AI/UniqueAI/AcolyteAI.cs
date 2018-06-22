using TheNegative.AI.Node;
using UnityEngine;

//Author: Liam 
//Last edited: 3/20/2018

namespace TheNegative.AI
{
    public class AcolyteAI : WorshipperAI
    {
        public int ShadowDrainDamage = 20;                      //The amount of damage that the shadow drain does
        public int ShieldHealth = 40;                           //The amount of HP that the shield has
        public float PercentageOfHealthToAttack = 0.3f;         //The amount of HP that is remainning before the AI will use the Shadow Drain Attack
        public GameObject ParticlePrefab;                       //The prefab for the particle emitter
        public GameObject ShadowDrainProjectile;                //The prefab for Shadow Drain Projectile.

        [SyncThis]
        protected bool m_HasShield = true;
        private GameObject m_Shield;

        private int m_CurrentShieldHealth = 40;     //The current amount of HP that the shield has
        private LinkNodeConnector m_DrainShotLink;  //The link connector to the draining shot

        #region Unity Methods
        protected override void Awake()
        {
            base.Awake();

            m_CurrentShieldHealth = ShieldHealth;
            m_Shield = gameObject.transform.Find("Shield").gameObject;
        }

        protected override void Update()
        {
            base.Update();

            if (!PhotonNetwork.isMasterClient)
                return;

            UpdateShield();
        }
        #endregion

        #region AI Damaging
        //Acolyte overrides take damage so it takes none while it has a shield
        [PunRPC]
        public override void TakeDamage(int playerNumber, int damage, Status[] statusEffects, int multiplier)
        {
            if (m_DeathAnimationStarted || m_ShouldFadeOut)
                return;

            foreach (Player player in m_PlayerList)
            {
                if (player.PlayerNumber == playerNumber && player.WeaponType == WeaponType.MELEE)
                {
                    damage += damage;
                }
            }

            if (m_HasShield)
            {
                SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "AcolyteShieldHit", transform.position);
                m_CurrentShieldHealth -= damage;
                photonView.RPC("SendOnDamageEvent", PhotonTargets.All, damage, playerNumber, multiplier);
            }
            else
            {
                base.TakeDamage(playerNumber, damage, statusEffects, multiplier);
            }
        }

        private void UpdateShield()
        {
            //if enemy has shield, check if there is any other non-acolyte AI still in the room
            if (m_HasShield)
            {
                if (MyIslandRoom.EnemiesInRoom.Count > 1 && m_CurrentShieldHealth > 0)
                {
                    for (int i = 0; i < MyIslandRoom.EnemiesInRoom.Count; i++)
                    {
                        if (MyIslandRoom.EnemiesInRoom[i] != null)
                        {
                            if (MyIslandRoom.EnemiesInRoom[i].GetType() != GetType())
                            {
                                //if there is another type of AI, return before changing shield to false
                                return;
                            }
                        }
                    }
                }

                m_HasShield = false;
                Debug.Log("Disabling the shield off of " + name + " in room" + MyIslandRoom.name);
                photonView.RPC("DisableShield", PhotonTargets.All);
            }
        }
        #endregion

        #region RPCs
        [PunRPC]
        private void DisableShield()
        {
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "AcolyteShieldBroken", transform.position);

            m_Shield.SetActive(false);
        }
        #endregion

        public void RecreateAcolyte(int damageToRecover, Vector3 positionToMoveAITo)
        {
            this.SetActive(true, positionToMoveAITo);
            Health.HP += damageToRecover;
        }

        protected override Node.Node CreateAttackingSequence()
        {
            Node.Node targetingSequence = CreateTargetingSequence();

            //Create the VampireSuckieSuckie attack sequence
            HealthLessThanNode healthCheckNode = new HealthLessThanNode(this, PercentageOfHealthToAttack);
            UseOnceNode usedShadowDrain = new UseOnceNode(this);
            SequenceNode healthandAttackNotUsed = new SequenceNode(this, "Health and Attack Check", healthCheckNode, usedShadowDrain);
            AnimatorNode shadowDrainNode = new AnimatorNode(this, m_AnimatorRef, "IsUsingDrainShot", ref m_DrainShotLink);           
            ToggleAINode toggleAINode = new ToggleAINode(this);
            DrainningShotNode fireProjectileNode = new DrainningShotNode(this, ShadowDrainDamage, ShadowDrainProjectile, FireLocation, "AcolyteProjectile", 5);

            //TODO: UNCOMMENT AND ADD SOUND NAME
            //PlaySoundNode shadowDrainSFX = new PlaySoundNode(this, );

            SequenceNode shadowDrainSequence = new SequenceNode(this, "Shadow Drain Attack", healthandAttackNotUsed, shadowDrainNode, toggleAINode, /*shadowDrainSFX,*/ fireProjectileNode);

            //Create ability node            
            CooldownNode cooldownNode = new CooldownNode(this, ProjectileAttackRate);
            AnimatorNode shootAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsShooting", ref m_ShootLink);
            SummonShotNode summonShotNode = new SummonShotNode(this, ProjectileDamage, ProjectilePrefab, FireLocation, ProjectilePoolName, ProjectilePoolSize);
            CooldownNode repositionCooldownNode = new CooldownNode(this, RepositionCooldown);
            //PlaySoundNode summonSFX = new PlaySoundNode(this,);

            //handle movement and movement 
            BackAwayNode backAwayNode = new BackAwayNode(this, false, DirectionMultiplier * 2);
            SideStepNode sideStep = new SideStepNode(this, false, DirectionMultiplier);

            MovementOptionRandomNode randomMovementChoice = new MovementOptionRandomNode(this, backAwayNode, sideStep);

            SequenceNode basicAttackingSequence = new SequenceNode(this, "Basic Attack Sequence", cooldownNode, shootAnimationNode, summonShotNode, /*summonSFX, */ repositionCooldownNode, randomMovementChoice);

            //create the attacking options sequence for making the AI attack
            SelectorNode attackSelector = new SelectorNode(this, "Attack Selector", shadowDrainSequence, basicAttackingSequence);
            SequenceNode attackOptionSequence = new SequenceNode(this, "Attack Choice Sequence", targetingSequence, attackSelector);

            return attackOptionSequence;
        }

        public void EventFinishedDrainShot()
        {
            m_DrainShotLink();
        }
    }
}
