using UnityEngine;
using TheNegative.AI.Node;
using System;

namespace TheNegative.AI
{
    public class MasterNergAI : AI
    {
        public GameObject Nerg;
        public GameObject ProjectilePrefab;
        public GameObject ProjectileSpawnLoc;
        public GameObject DigParticleSystem;

        //dig variables
        private bool m_HasDug = false;
        private float m_BiteDamageTimer = 0.0f;
        private Player m_LocalPlayer = null;
        private bool m_ShouldBiteDamage = false;

        private LinkNodeConnector m_DigAnimationLink;
        private LinkNodeConnector m_BiteAnimationLink;
        private LinkNodeConnector m_SpitAnimationLink;

        public bool HasDug { get { return m_HasDug; } set { m_HasDug = value; } }

        public override void ActivateEnemy()
        {
            base.ActivateEnemy();
        }

        public override void Init()
        {
            base.Init();
        }

        protected override void HandleDeath()
        {
            Fadeout();
        }

        protected override void Update()
        {
            if (m_ShouldBiteDamage)
            {
                Status[] statuses = { Status.Root };
                m_LocalPlayer.TakeDamage(0, transform.position, statuses);

                m_BiteDamageTimer += Time.deltaTime;

                if (m_BiteDamageTimer >= 1.0f)
                {
                    m_BiteDamageTimer = 0.0f;
                    m_LocalPlayer.TakeDamage(1, transform.position ,null);
                }
            }

            base.Update();

            if(Health.IsDead)
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

        protected override SelectorNode CreateBehaviour()
        {
            //create the dig nodes
            CanDigNode canDigNode = new CanDigNode(this);
            HealthLessThanNode healthCheckNode = new HealthLessThanNode(this, 0.6f);
            AnimatorNode digAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsDigging", ref m_DigAnimationLink);
            ToggleParticleSystemNode toggleParticleNode = new ToggleParticleSystemNode(this, DigParticleSystem);

            DelegateNode.Delegate toggleTriggerFunc = SetColliderTrigger;
            DelegateNode toggleTriggerNode = new DelegateNode(this, toggleTriggerFunc, true);

            DelegateNode.Delegate toggleColliderFunc = ToggleCollider;
            DelegateNode toggleColliderNode = new DelegateNode(this, toggleColliderFunc);
            
            ToggleNavMeshAgentNode toggleAgentNode = new ToggleNavMeshAgentNode(this);
            DelayNode delayNode = new DelayNode(this, 2.0f);
            //TODO: COMMENT THIS SECTION AND ADD STRING
            PlaySoundNode digSFX = new PlaySoundNode(this, "NergDig");
            LerpNode lerpDownNode = new LerpNode(this, Vector3.down, 1.0f, 2.0f, 10.0f);

            DelegateNode.Delegate rotateFunc = RotateVertical;
            DelegateNode rotateNode = new DelegateNode(this, rotateFunc);

            TeleportToTargetOffsetNode teleportNode = new TeleportToTargetOffsetNode(this, new Vector3(0.0f, -5.0f, 0.0f));
            LerpToTargetNode lerpToTargetNode = new LerpToTargetNode(this, 0.5f, 1.0f, 10.0f);
            AnimatorNode biteAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsBiting", ref m_BiteAnimationLink);
            RunUntillTargetNull targetNullNode = new RunUntillTargetNull(this);

            //create the dig sequence
            SequenceNode digSequence = new SequenceNode(this, "DigSequence",
                                                               healthCheckNode,
                                                               canDigNode,
                                                               toggleParticleNode,
                                                               toggleTriggerNode,
                                                               toggleColliderNode,
                                                               toggleAgentNode,
                                                               delayNode,
                                                               digSFX,
                                                               lerpDownNode,
                                                               toggleParticleNode,
                                                               rotateNode,
                                                               teleportNode,
                                                               toggleColliderNode,
                                                               toggleParticleNode,
                                                               lerpToTargetNode,
                                                               digSFX,
                                                               delayNode,
                                                               toggleParticleNode,
                                                               targetNullNode
                                                               );

            //create the targeting nodes
            TargetingSightNode sightNode = new TargetingSightNode(this, 1);
            TargetingLowHealthNode lowHealth = new TargetingLowHealthNode(this, 3);
            CalculateTargetNode calcTarget = new CalculateTargetNode(this);            

            //assign the targeting sequence
            SequenceNode targetingSequ = new SequenceNode(this, "TargetingSequence", sightNode, lowHealth, calcTarget);           

            //create the spit nodes
            CooldownNode spitCooldownNode = new CooldownNode(this, 1.0f);
            //AnimatorNode spitAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsSpitting", ref m_SpitAnimationLink);
            ShootProjectileNode projectileNode = new ShootProjectileNode(this, 1, ProjectilePrefab, ProjectileSpawnLoc, "MasterNergProjectile", 10);
            //SFX for the sound of the nerg spitting
            PlaySoundNode spitSound = new PlaySoundNode(this, "NergSpit");


            //create the movement nodes
            CooldownNode movementCooldownNode = new CooldownNode(this, 3.0f);
            //BackAwayNode backAwayNode = new BackAwayNode(this, false, 1.0f);
            SideStepNode sideStepNode = new SideStepNode(this, false, 2.0f);
            PredictiveAvoidanceNode predictMovementNode = new PredictiveAvoidanceNode(this, false, true, 2.0f, 5.0f);
            MovementOptionRandomNode movementNodes = new MovementOptionRandomNode(this, sideStepNode, predictMovementNode);

            //SFX for the sound of the nerg moving backward
            //TODO: COMMENT THIS SECTION AND ADD STRING
            //PlaySoundNode crawlingSFX = new PlaySoundNode(this,);

            //create the spit sequence
            SequenceNode spitSequence = new SequenceNode(this, "SpitSequence", spitCooldownNode, projectileNode, spitSound, movementCooldownNode, movementNodes/*, crawlingSFX*/);
            
            //assign the attack selector
            SelectorNode attackSelector = new SelectorNode(this, "AttackSelector", digSequence, spitSequence);

            //create the attack sequence
            SequenceNode attackSequence = new SequenceNode(this, "AttackTargetSequence", targetingSequ, attackSelector);

            //create utility selector
            SelectorNode utilitySelector = new SelectorNode(this, "UtilitySelector", attackSequence);

            return utilitySelector;
        }

        private void SetColliderTrigger(params object[] args)
        {
            if (args[0] is bool)
            {
                bool flag = (bool)args[0];
                photonView.RPC("RPCSetColliderTrigger", PhotonTargets.All, flag);
            }
        }

        private void ToggleCollider(params object[] args)
        {
            photonView.RPC("RPCToggleCollider", PhotonTargets.All);
        }

        private void RotateVertical(params object[] args)
        {
            transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
        }

        private void OnTriggerEnter(Collider other)
        {
            Player p = other.GetComponent<Player>();

            if (p != null)
            {
                //if the player that collided is mine, and local player isn't set yet
                if (p.photonView.isMine && m_LocalPlayer == null)
                {
                    m_LocalPlayer = p;
                    m_ShouldBiteDamage = true;

                    Status[] statuses = { Status.Root };
                    m_LocalPlayer.TakeDamage(0, transform.position ,statuses);

                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Player p = other.GetComponent<Player>();

            if (p != null)
            {
                if (p.photonView.isMine)
                {
                    m_ShouldBiteDamage = false;
                    m_LocalPlayer = null;
                }
            }
        }

        [PunRPC]
        protected void RPCSetColliderTrigger(bool flag)
        {
            gameObject.GetComponent<Collider>().isTrigger = flag;
        }

        [PunRPC]
        protected void RPCToggleCollider()
        {
            gameObject.GetComponent<Collider>().enabled = !gameObject.GetComponent<Collider>().enabled;
        }
    }
}