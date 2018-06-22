using TheNegative.AI.Node;
using UnityEngine;

//Author: Liam
//Last updated: 3/20/2018

namespace TheNegative.AI
{
    public class WorshipperAI : AI
    {
        public GameObject FireLocation;

        //Summon Shot Node
        public int ShadowDemonSpawnLimit = 3;                       //The limit as to how many shadow demons this AI may create
        public float ProjectileAttackRate = 3.0f;                   //rate at which projectiles are fired
        public int ProjectileDamage = 7;                            //damage the projectile does on hit
        public GameObject ProjectilePrefab;                         //prefab reference for projectile
        public string ProjectilePoolName = "WorshipperShadowBall";  //name for object pool manager
        public int ProjectilePoolSize = 5;                          //initial size of the projectile pool at creation
        public float DirectionMultiplier = 3.0f;

        //Reposition Node
        public float RepositionCooldown = 6.0f;                     //cooldown until the AI repositions again
        public float RepositionTravelDistance = 3.0f;               //how far the AI moves when they reposition

        [SyncThis]
        protected int m_NumberOfShadowDemonsSpawned = 0;              //The number of shadow demons that are currently spawned and tied to this AI

        protected LinkNodeConnector m_ShootLink;                    //The link connector for the shooting animation
        protected LinkNodeConnector m_Idle1Link;                    //The link connector for the idle animation       

        protected override void Update()
        {
            if (Health.IsDead)
                HandleDeath();
            if (!Health.IsDead)
                base.Update();
        }

        protected override void PlayInjurySound()
        {
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "WorshipperInjured", transform.position);
        }

        protected override void PlayDeathSound()
        {
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "WorshipperDying", transform.position);
        }

        protected override SelectorNode CreateBehaviour()
        {
            Node.Node fullAttackSequence = CreateAttackingSequence();

            //create the utility selector
            SelectorNode utilitySelector = new SelectorNode(this, "UtilitySelector", fullAttackSequence);

            return utilitySelector;
        }

        protected virtual Node.Node CreateAttackingSequence()
        {
            Node.Node targetingSequence = CreateTargetingSequence();

            //create ability node
            CooldownNode cooldownNode = new CooldownNode(this, ProjectileAttackRate);
            AnimatorNode shootAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsShooting", ref m_ShootLink);

            //TODO: UNCOMMENT THIS CODE AND ADD THE STRING NAME
            PlaySoundNode shotNodeSFX = new PlaySoundNode(this, "WorshipperShoot");
            SummonShotNode summonShotNode = new SummonShotNode(this, ProjectileDamage, ProjectilePrefab, FireLocation, ProjectilePoolName, ProjectilePoolSize);
            CooldownNode repositionCooldownNode = new CooldownNode(this, RepositionCooldown);

            //handle movement and movement 
            BackAwayNode backAwayNode = new BackAwayNode(this, false, DirectionMultiplier);
            SideStepNode sideStep = new SideStepNode(this, false, DirectionMultiplier);

            MovementOptionRandomNode randomMovementChoice = new MovementOptionRandomNode(this, backAwayNode, sideStep);

            SequenceNode attackingSequence = new SequenceNode(this, "Attacking Sequence", cooldownNode, shootAnimationNode, summonShotNode, shotNodeSFX, repositionCooldownNode, randomMovementChoice);

            //create the attacking options sequence for making the AI attack
            SequenceNode attackOptionSequence = new SequenceNode(this, "Attack Choice Sequence", targetingSequence, attackingSequence);

            return attackOptionSequence;
        }

        protected virtual Node.Node CreateTargetingSequence()
        {
            //Create the targeting nodes
            TargetingSightNode targetSightNode = new TargetingSightNode(this, 1);
            TargetingDistanceNode targetingDistanceNode = new TargetingDistanceNode(this, 1);
            TargetingHighHealthNode targetingHighHealthNode = new TargetingHighHealthNode(this, 1);
            TargetingHighestDamageNode targetingHighestDamageNode = new TargetingHighestDamageNode(this, 2);
            TargetingCharacterType targetingCharacterType = new TargetingCharacterType(this, 1, WeaponType.RANGED);
            CalculateTargetNode calculateTargetNode = new CalculateTargetNode(this);

            //Create the targeting sequence and attach nodes
            SequenceNode targetingSequence = new SequenceNode(this, "TargetingSequence");
            targetingSequence.AddChildren(targetingDistanceNode,
                                          targetingHighHealthNode,
                                          targetingHighestDamageNode,
                                          targetingCharacterType,
                                          calculateTargetNode);

            return targetingSequence;
        }

        public void EventFinishedSummon()
        {
            m_ShootLink();
        }

        public int NumberOfShadowDemonsSpawned { get { return m_NumberOfShadowDemonsSpawned; } set { m_NumberOfShadowDemonsSpawned = value; } }
    }
}
