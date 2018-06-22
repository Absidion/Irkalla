using System;
using System.Collections;
using System.Collections.Generic;
using TheNegative.AI.Node;
using UnityEngine;

namespace TheNegative.AI
{
    public class GolemAI : AI
    {
        //Ground Slam Node
        public GameObject FallingRockPrefab;

        //Ground Spikes Node
        public GameObject SpikePrefab;
        public float SpikesCooldownTime = 10.0f;
        public float SpikesMovementTime = 0.1f;

        //Punch Node
        public float PunchKnockback = 6.0f;
        public float PunchAttackRange = 4.0f;
        public int PunchDamage = 15;
        public Collider PunchHitBox;

        //Rock Throw Node
        public GameObject RockThrowPrefab;
        public GameObject FireLocation;
        public GameObject RockMesh;
        public int RockThrowDamage = 20;

        //Ground Slam Node
        public float GroundSlamCooldownTime = 15.0f;

        //Ground Slam Particles
        public GameObject SlamParticleSystem;

        //Link Node Connectors
        private LinkNodeConnector m_RockThrowStartLink;
        private LinkNodeConnector m_RockThrowingLink;
        private LinkNodeConnector m_RockThrowRecoveryLink;
        private LinkNodeConnector m_GroundSpikesStartLink;
        private LinkNodeConnector m_GroundSpikesRecoveryLink;
        private LinkNodeConnector m_GroundSlamStartLink;
        private LinkNodeConnector m_GroundSlamRecoveryLink;
        private LinkNodeConnector m_PunchLink;

        private int m_AttackState = 1;

        protected override SelectorNode CreateBehaviour()
        {
            //Create melee attack nodes

            //Create ground slam nodes
            CooldownNode groundSlamCooldownNode = new CooldownNode(this, GroundSlamCooldownTime);
            AnimatorNode groundSlamStartAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsGroundSlamStarting", ref m_GroundSlamStartLink);
            GroundSlamNode groundSlamNode = new GroundSlamNode(this, FallingRockPrefab, PunchHitBox.transform);

            //TODO: COMMENT THIS SECTION AND ADD STRING
            PlaySoundNode slamSFX = new PlaySoundNode(this, "GolemGroundSlamImpact");
            AnimatorNode groundSlamRecoveryAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsGroundSlamRecovering", ref m_GroundSlamRecoveryLink);

            ToggleParticleSystemNode toggleGroundSlamPartclesNode = new ToggleParticleSystemNode(this, SlamParticleSystem);

            //Create ground slam sequence
            SequenceNode groundSlamSequence = new SequenceNode(this, "GroundSlamSequence", groundSlamCooldownNode, groundSlamStartAnimationNode, groundSlamNode, toggleGroundSlamPartclesNode, slamSFX, groundSlamRecoveryAnimationNode);

            //Create punch nodes
            CheckDistanceToTargetNode punchDistanceNode = new CheckDistanceToTargetNode(this, 5.0f);

            ToggleMeleeColliderNode togglePunchNode = new ToggleMeleeColliderNode(this, PunchHitBox, 15);

            LookAtTargetNode lookAtTargetNode = new LookAtTargetNode(this);
            AnimatorNode punchAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsPunching", ref m_PunchLink);
            //TODO: COMMENT THIS SECTION AND ADD STRING
            PlaySoundNode punchSFX = new PlaySoundNode(this, "GolemPunchImpact");
            //Create punch sequence
            SequenceNode punchSequence = new SequenceNode(this, "PunchSequence", punchDistanceNode, togglePunchNode, lookAtTargetNode, punchSFX, punchAnimationNode, togglePunchNode);

            //Create melee attack selector in order: GroundSlam, Punch
            SelectorNode meleeAttackSelector = new SelectorNode(this, "MeleeAttackSelector");
            meleeAttackSelector.AddChildren(groundSlamSequence, punchSequence);

            //Create ranged attack nodes

            //Create rock throw nodes
            CooldownNode rockThrowCooldownNode = new CooldownNode(this, 10.0f);
            AnimatorNode rockThrowStartAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsRockThrowStarting", ref m_RockThrowStartLink);
            //TODO: COMMENT THIS SECTION AND ADD STRING
            PlaySoundNode rockThrowSFX = new PlaySoundNode(this, "GolemRockThrow");

            DelegateNode.Delegate setRockMeshFunc = SetRockMesh;
            DelegateNode setRockMeshTrueNode = new DelegateNode(this, setRockMeshFunc, true);

            AnimatorNode rockThrowingAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsRockThrowing", ref m_RockThrowingLink);
            DelegateNode setRockMeshFalseNode = new DelegateNode(this, setRockMeshFunc, false);
            RockThrowNode rockThrowNode = new RockThrowNode(this, RockThrowPrefab, FireLocation, RockThrowDamage);
            AnimatorNode rockThrowRecoveryAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsRockThrowRecovering", ref m_RockThrowRecoveryLink);

            //Create rock throw sequence
            SequenceNode rockThrowSequence = new SequenceNode(this, "RockThrowSequence", rockThrowCooldownNode,
                                                                                         rockThrowStartAnimationNode,
                                                                                         setRockMeshTrueNode,
                                                                                         rockThrowingAnimationNode,
                                                                                         setRockMeshFalseNode,
                                                                                         rockThrowNode,
                                                                                         rockThrowSFX,
                                                                                         rockThrowRecoveryAnimationNode
                                                                                         );

            //Create ground spike nodes
            CooldownNode groundSpikesCooldownNode = new CooldownNode(this, 10.0f);
            AnimatorNode groundSpikesStartAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsGroundSpikeStarting", ref m_GroundSpikesStartLink);
            GroundSpikesNode groundSpikesNode = new GroundSpikesNode(this, SpikePrefab, SpikesMovementTime);
            AnimatorNode groundSpikesRecoveryAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsGroundSpikesRecovering", ref m_GroundSpikesRecoveryLink);

            //Create ground spike sequence
            SequenceNode groundSpikesSequence = new SequenceNode(this, "GroundSpikesSequence", groundSpikesCooldownNode, groundSpikesStartAnimationNode, groundSpikesNode, groundSpikesRecoveryAnimationNode);

            //Create ranged attack selector in order: RockThrow, GroundSpikes
            SelectorNode rangedAttackSelector = new SelectorNode(this, "RangedAttackSelector");
            rangedAttackSelector.AddChildren(rockThrowSequence, groundSpikesSequence);

            //Create targeting nodes
            TargetingAfflicted targetingAfflictedNode = new TargetingAfflicted(this, 8, Status.Stun);
            TargetingDistanceNode targetingDistanceNode = new TargetingDistanceNode(this, 1);
            TargetingHighHealthNode targetingHighHealthNode = new TargetingHighHealthNode(this, 1);
            TargetingHighestDamageNode targetingHighestDamageNode = new TargetingHighestDamageNode(this, 1);
            TargetingCharacterType targetingCharacterType = new TargetingCharacterType(this, 1, WeaponType.RANGED);
            CalculateTargetNode calculateTargetNode = new CalculateTargetNode(this);

            //Create the targeting sequence and attach nodes
            SequenceNode targetingSequence = new SequenceNode(this, "TargetingSequence");
            targetingSequence.AddChildren(targetingAfflictedNode,
                                          targetingDistanceNode,
                                          targetingHighHealthNode,
                                          targetingHighestDamageNode,
                                          targetingCharacterType,
                                          calculateTargetNode);

            //Create approach node
            ApproachNode approachNode = new ApproachNode(this);

            //Create Abilities/Melee/Ranged/Approach Selector
            SelectorNode actionSelector = new SelectorNode(this, "ActionSelector");
            actionSelector.AddChildren(meleeAttackSelector, rangedAttackSelector, approachNode);

            //Create Target->Action sequence
            SequenceNode getTargetAndUseAbilitySequence = new SequenceNode(this, "GetTargetAndUseAbilitySequence");
            getTargetAndUseAbilitySequence.AddChildren(targetingSequence, actionSelector);

            //Create the utility selector with the previous sequence and approach node
            SelectorNode utilitySelector = new SelectorNode(this, "UtilitySelector");
            utilitySelector.AddChildren(getTargetAndUseAbilitySequence);

            return utilitySelector;
        }

        protected override void Awake()
        {
            PunchHitBox.GetComponent<MeleeCollider>().Init(-1, 15, null);

            base.Awake();
        }

        protected override void Update()
        {
            base.Update();

            if (Health.IsDead)
                HandleDeath();

            if (PhotonNetwork.isMasterClient)
            {
                //update attack state 
                m_AnimatorRef.SetInteger("AttackStage", m_AttackState);
            }
        }

        protected override void PlayInjurySound()
        {
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "GolemInjured", transform.position);
        }

        protected override void PlayDeathSound()
        {
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "GolemDeath2", transform.position);
        }

        protected override void UpdateAIAnimations()
        {
            //if (!CurrentlyInAnimation())
            //{
            //    //activate walking animation when the agent has a path
            //    m_AnimatorRef.SetBool("IsWalking", m_Agent.hasPath);
            //}
            //else
            //    m_AnimatorRef.SetBool("IsWalking", false);
            
            ////TODO: add transition to idle
            //m_AnimatorRef.SetBool("IsIdle", false);
        }

        #region Animation Events

        public void EventRockThrowStartFinished()
        {
            m_RockThrowStartLink();
        }

        public void EventRockThrowingFinished()
        {
            m_RockThrowingLink();
        }

        public void EventRockThrowRecoveryFinished()
        {
            m_RockThrowRecoveryLink();
        }

        public void EventGroundSpikesStartFinished()
        {
            m_GroundSpikesStartLink();
        }

        public void EventGroundSpikesRecoveryFinished()
        {
            m_GroundSpikesRecoveryLink();
        }

        public void EventGroundSlamStartFinished()
        {
            m_GroundSlamStartLink();
        }

        public void EventGroundSlamRecoveryFinished()
        {
            m_GroundSlamRecoveryLink();
        }

        public void EventPunchFinished()
        {
            m_PunchLink();

            m_AttackState++;
            if (m_AttackState > 2)
                m_AttackState = 1;
        }
        
        #endregion

        //returns true if any animation triggers are true
        protected override bool CurrentlyInAnimation()
        {
            return false;
        }

        private void SetRockMesh(params object[] args)
        {
            if (args[0] is bool)
            {
                bool flag = (bool)args[0];
                RockMesh.SetActive(flag);
            }
        }
    }
}
