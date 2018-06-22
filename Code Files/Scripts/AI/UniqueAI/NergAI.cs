using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheNegative.AI.Node;
using System;

//Author: James
//Created: 12/15/2017
//Last Updated: 12/15/2017

namespace TheNegative.AI
{
    public class NergAI : AI
    {
        #region Variables/Properties

        //Dive Node
        public float DiveCooldown = 5.0f;
        public int DiveDamage = 15;
        public float DiveAngle = 20.0f;

        //Shoot Projectile Node
        public float ProjectileAttackRate = 2.0f;
        public int ProjectileDamage = 15;
        public GameObject ProjectilePrefab;
        public GameObject FireLocation;
        public string ProjectilePoolName = "NergProjectilePool";
        public int ProjectilePoolSize = 5;

        private bool m_CanAttack;                                   //used for dive node, nerg will only do damage when airborne

        public bool CanAttack { get { return m_CanAttack; } set { m_CanAttack = value; } }

        #endregion

        #region Functions

        protected override void HandleDeath()
        {
            Fadeout();
        }

        protected override SelectorNode CreateBehaviour()
        {
            //Create ability nodes            
            DiveNode diveNode = new DiveNode(this, DiveCooldown, DiveDamage, DiveAngle);
            //TODO: COMMENT THIS SECTION AND ADD STRING
            //PlaySoundNode diveSFX = new PlaySoundNode(this, );
            CooldownNode spitCooldownNode = new CooldownNode(this, 1.0f);
            SequenceNode diveSequence = new SequenceNode(this, "Dive Sequence", diveNode/*, diveSFX*/);
            //TODO: COMMENT THIS SECTION AND ADD STRING
            //PlaySoundNode spitSFX = new PlaySoundNode(this, );
            ShootProjectileNode shootProjectileNode = new ShootProjectileNode(this,
                                                                              ProjectileDamage,
                                                                              ProjectilePrefab,
                                                                              FireLocation,
                                                                              ProjectilePoolName,
                                                                              ProjectilePoolSize);

            SequenceNode spitSequence = new SequenceNode(this, "SpitSequence", spitCooldownNode, shootProjectileNode/*, spitSFX*/);

            //Create ability selector in order: Dive, Shoot Projectile
            SelectorNode abilitySelector = new SelectorNode(this, "AbilitySelector");
            abilitySelector.AddChildren(diveSequence, spitSequence);

            //Create targeting nodes
            TargetingMasterNergTarget targetingMasterNergTarget = new TargetingMasterNergTarget(this, 10);
            TargetingSightNode targetingSightNode = new TargetingSightNode(this, 1);
            CalculateTargetNode calculateTargetNode = new CalculateTargetNode(this);

            //Create targeting sequence
            SequenceNode targetingSequence = new SequenceNode(this, "TargetingSequence");
            targetingSequence.AddChildren(targetingMasterNergTarget, targetingSightNode, calculateTargetNode);

            //Create the Target->Abilities sequence
            SequenceNode getTargetAndUseAbilitySequence = new SequenceNode(this, "GetTargetAndUseAbilitySequence");
            getTargetAndUseAbilitySequence.AddChildren(targetingSequence, abilitySelector);

            //Create utility selector
            SelectorNode utilitySelector = new SelectorNode(this, "UtilitySelector");
            utilitySelector.AddChildren(getTargetAndUseAbilitySequence);

            return utilitySelector;
        }

        protected override void Awake()
        {
            base.Awake();

            if (PhotonNetwork.isMasterClient)
                rigidbody.useGravity = true;
        }

        protected override void Update()
        {
            base.Update();

            if (Health.IsDead)
                HandleDeath();
        }

        protected override void PlayInjurySound()
        {
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "NergInjured", transform.position);
        }

        protected override void PlayDeathSound()
        {
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "NergDeath2", transform.position);
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (CanAttack)
            {
                if (collision.gameObject.tag == "Player")
                {
                    Player playerRef = collision.gameObject.GetComponent<Player>();

                    if (playerRef != null)
                    {
                        playerRef.photonView.RPC("TakeDamage", PhotonTargets.All, DiveDamage, transform.position ,ElementalDamage.ToArray());
                    }
                }

                if (collision.gameObject.tag != "Enemy" | collision.gameObject.tag != "Projectile")
                {
                    if (PhotonNetwork.isMasterClient)
                        rigidbody.velocity = Vector3.zero;
                }

                if (PhotonNetwork.isMasterClient)
                {
                    SetIsKinematic(true);
                    SetNavMeshAgent(true);
                }

                photonView.RPC("SetCanAttack", PhotonTargets.All, false);
            }
        }

        #endregion

        #region RPCs

        [PunRPC]
        public void SetNergEnabled(bool active)
        {
            gameObject.SetActive(active);
            this.enabled = active;
        }

        [PunRPC]
        public void SetCanAttack(bool flag)
        {
            CanAttack = flag;
        }

        #endregion
    }
}
