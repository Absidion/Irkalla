using System;
using UnityEngine;

using TheNegative.AI.Node;

namespace TheNegative.AI
{
    public class InannaAI : AI
    {
        public GameObject ArrowProjectilePrefab;    //prefab for standard projectile shot
        public int HitscanShotDamage;               //how much damage the snipe shot does
        public float HitScanBuildUp;                //time the hitscan shot will wait before locking on target
        public float HitScanShotDelay;              //time before the hitscan shot will fire when it has locked on target
        public float RainOfArrowsWaitTime;          //time the radius of attack will show before activating damage
        public float RainOfArrowsDiameter;          //attack range of rain of arrows       
        public float RainOfArrowsCooldown;          //how much time before rain of arrows can be used again, gets doubled when players are closer
        public float JumpToMaxTime;                 //how long it takes to jump to max height when moving
        public float JumpHeight;                    //how high she jumps when moving to next tower
        public float JumpSpeed;                     //how fast she travels through air when moving
        public float ArrowShotRate;                 //how often arrows can be fired
        public int ArrowShotDamage;                 //how much damage each arrow shot does
        public GameObject ShootLocation;            //location where all attacks shoot from on the bow

        private IECycle m_Cycle;                                //reference to classe that handles all cycle functionality
        private EreshkigalAI m_Ereshkigal;                      //reference to boss partner
        private GameObject m_CircularAOEPrefab;                 //prefab for AOE attack
        private Vector3 m_ArrowRainTargetPos = Vector3.zero;

        //Link Node Connectors
        private LinkNodeConnector m_JumpLink;
        private LinkNodeConnector m_LandingLink;
        private LinkNodeConnector m_ArrowRainShotLink;
        private LinkNodeConnector m_SnipeShotLink;
        private LinkNodeConnector m_RegularShotLink;

        private HitScanSingleTarget m_HitScanShot = null;   //reference to hitscan projectile used for snipe
        private NetworkLineRenderer m_LineRenderer = null;  //reference to network line renderer used for snipe

        protected override void Awake()
        {
            base.Awake();

            m_Cycle = new IECycle(m_Health.HP / 4, 4);
            m_HitScanShot = GetComponent<HitScanSingleTarget>();
            m_LineRenderer = GetComponent<NetworkLineRenderer>();

            m_CircularAOEPrefab = PhotonNetwork.Instantiate("Attacks/InannaCircularAOE", Vector3.zero, Quaternion.identity, 0);
            m_CircularAOEPrefab.GetComponent<LiamBehaviour>().SetActive(false);

            SetNavMeshAgent(false);

            GameManager.PlayerPopulation += OnFindEreshkigal;

            m_ShouldFadeOut = false;
        }

        protected override void Update()
        {
            base.Update();

            if (Health.IsDead)
            {
                if(m_ShouldFadeOut == false)
                    m_ShouldFadeOut = true;

                HandleDeath();
            }
        }

        protected override SelectorNode CreateBehaviour()
        {
            //Create the reposition nodes
            SafetyCheckNode safetyCheckNode = new SafetyCheckNode(this);

            DelegateNode.Delegate invincibleFunc = SetInvincible;
            DelegateNode setInvincibleTrueNode = new DelegateNode(this, invincibleFunc, true);

            AnimatorNode jumpAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsInannaJumping", ref m_JumpLink);

            LerpNode lerpUpwardsNode = new LerpNode(this, Vector3.up, JumpToMaxTime, JumpHeight, JumpSpeed);

            //Grab all tower points in the map
            GameObject[] towerPointGameObjects = GameObject.FindGameObjectsWithTag("TowerPoint");
            Transform[] towerPointTransforms = new Transform[towerPointGameObjects.Length];

            for (int i = 0; i < towerPointTransforms.Length; i++)
            {
                towerPointTransforms[i] = towerPointGameObjects[i].transform;
            }

            InannaMoveTowerNode moveTowerNode = new InannaMoveTowerNode(this, towerPointTransforms);
            LerpNode lerpDownwardsNode = new LerpNode(this, Vector3.down, JumpToMaxTime, JumpHeight, JumpSpeed);
            AnimatorNode landAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsInannaLanding", ref m_LandingLink);
            PlaySoundNode InnanaJumpSoundNode = new PlaySoundNode(this,"WingFlap");
            DelegateNode setInvincibleFalseNode = new DelegateNode(this, invincibleFunc, false);

            //Create reposition sequence

            SequenceNode repositionSequence = new SequenceNode(this, "RepositionSequence", safetyCheckNode, setInvincibleTrueNode, jumpAnimationNode, InnanaJumpSoundNode, lerpUpwardsNode, moveTowerNode, lerpDownwardsNode, InnanaJumpSoundNode, landAnimationNode, setInvincibleFalseNode);

            //Create arrow rain nodes
            PlaySoundNode ArrowRainSound = new PlaySoundNode(this,"ArrowRain", 8.0f);
            RainOfArrowsCooldownNode arrowRainCooldownNode = new RainOfArrowsCooldownNode(this, RainOfArrowsCooldown, RainOfArrowsDiameter);
            RainOfArrowsTargetingNode arrowRainTargetNode = new RainOfArrowsTargetingNode(this, RainOfArrowsDiameter);
            AnimatorNode arrowRainAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsFiringRainOfArrows", ref m_ArrowRainShotLink);
            RainOfArrowsAttackNode arrowRainAttackNode = new RainOfArrowsAttackNode(this, m_CircularAOEPrefab.GetComponent<CircularAOE>(), RainOfArrowsWaitTime);

            //Create arrow rain sequence 
            SequenceNode arrowRainSequence = new SequenceNode(this, "RainOfArrowsSequence", arrowRainCooldownNode, arrowRainTargetNode, arrowRainAnimationNode, arrowRainAttackNode, ArrowRainSound);

            //Create snipe shot nodes
            PlaySoundNode HeavyShotSound = new PlaySoundNode(this, "HeavyShot");
            SnipeDelayNode snipeDelayNode = new SnipeDelayNode(this, m_Ereshkigal.transform, m_LineRenderer, m_Ereshkigal.EreshkigalSafeSpace, HitScanBuildUp);
            AnimatorNode snipeAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsSniping", ref m_SnipeShotLink);
            HitScanShotNode hitScanShotNode = new HitScanShotNode(this, ShootLocation.transform, m_HitScanShot, HitscanShotDamage, HitScanShotDelay);

            //Create snipe sequence
            SequenceNode snipeSequence = new SequenceNode(this, "SnipeSequence", snipeDelayNode, snipeAnimationNode, hitScanShotNode, HeavyShotSound);

            //Create arrow shot targeting nodes
            TargetingDistanceFromLocationGreaterThenNode targetingDistanceNode = new TargetingDistanceFromLocationGreaterThenNode(this, m_Ereshkigal.transform, m_Ereshkigal.EreshkigalSafeSpace * 0.5f, 2);
            TargetingLowHealthNode targetingLowestHPNode = new TargetingLowHealthNode(this, 2);
            TargetingHighestDamageNode targetingHighestDamageNode = new TargetingHighestDamageNode(this, 1);
            TargetingSightNode targetingSightNode = new TargetingSightNode(this, 1, true);
            CalculateTargetNode calculateTargetNode = new CalculateTargetNode(this);

            //Create arrow shot targeting sequence 
            SequenceNode targetingSequence = new SequenceNode(this, "ArrowTargetingSequence", targetingDistanceNode, targetingLowestHPNode, targetingHighestDamageNode, targetingSightNode, calculateTargetNode);

            //Create other arrow shot nodes
            PlaySoundNode ArrowShotSoundNode = new PlaySoundNode(this, "ArrowFire");
            CooldownNode arrowShotCooldownNode = new CooldownNode(this, ArrowShotRate);
            AnimatorNode arrowShotAnimationNode = new AnimatorNode(this, m_AnimatorRef, "IsFiringArrowShot", ref m_RegularShotLink);
            ShootProjectileNode arrowShotProjectileNode = new ShootProjectileNode(this, ArrowShotDamage, ArrowProjectilePrefab, ShootLocation, "InannaArrowPool", 5);

            //Create arrow shot sequence
            SequenceNode arrowShotSequence = new SequenceNode(this, "ArrowShotSequence", targetingSequence, arrowShotCooldownNode, arrowShotAnimationNode, ArrowShotSoundNode, arrowShotProjectileNode);

            //Create utility selector
            SelectorNode utilitySelector = new SelectorNode(this, "InannaUtilitySelector", repositionSequence, arrowRainSequence, snipeSequence, arrowShotSequence);

            return utilitySelector;
        }

        protected override void PlayDeathSound()
        {
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "InannaDeathSound", transform.position);
        }

        protected override void PlayInjurySound()
        {
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "InannaInjured", transform.position);
        }

        //Override take damage to add to cycle damage
        [PunRPC]
        public override void TakeDamage(int playerNumber, int damage, Status[] statusEffects, int multiplier)
        {
            base.TakeDamage(playerNumber, damage, statusEffects, multiplier);
            Debug.Log("DAMAGED");

            if (!Health.IsInvincible)
                m_Cycle.CycleDamage += damage;
        }

        //Event that gets called during player population to get reference to Ereshkigal
        private void OnFindEreshkigal(object sender, EventArgs eventArgs)
        {
            EreshkigalAI[] erRef = Resources.FindObjectsOfTypeAll(typeof(EreshkigalAI)) as EreshkigalAI[];
            m_Ereshkigal = erRef[0];
        }

        //Function gets passed into delegate node and gets called after safety check node is true
        private void SetInvincible(params object[] args)
        {
            if (args[0] is bool)
            {
                bool flag = (bool)args[0];
                m_Health.IsInvincible = flag;
            }
        }

        #region Animation Events
        public void EventJump()
        {
            m_JumpLink();
        }

        public void EventLanding()
        {
            m_LandingLink();
        }

        public void EventArrowRain()
        {
            m_ArrowRainShotLink();
        }

        public void EventSnipeShot()
        {
            m_SnipeShotLink();
        }

        public void EventRegularShot()
        {
            m_RegularShotLink();
        }
        #endregion

        public IECycle Cycle { get { return m_Cycle; } }

        public Vector3 ArrowRainTargetPos { get { return m_ArrowRainTargetPos; } set { m_ArrowRainTargetPos = value; } }
    }

    public class IECycle
    {
        public int CycleDamage;                         //The damage that Inanna has taken this cycle
        public int CurrentCycle;                        //The current cycle that the AI are currently in
        public event EventHandler OnCycleChange;        //The variable to handle the cycle being changed

        private int m_ChangeCycleDamage;                //The amount of damage required to change Inanna and Ereshkigal to the next cycle
        private int m_MaxCycles;                        //The maximum amount of cycles in the fight

        public IECycle(int changeCycleDamage, int maxCycles)
        {
            CycleDamage = 0;
            CurrentCycle = 1;

            m_ChangeCycleDamage = changeCycleDamage;
            m_MaxCycles = maxCycles;

            OnCycleChange = null;
        }

        public void TakeDamage(int damage)
        {
            CycleDamage += damage;
        }

        public bool ShouldChangeCycles()
        {
            if (CycleDamage >= m_ChangeCycleDamage)
            {
                if(OnCycleChange != null)   OnCycleChange(this, null);
                CycleDamage = 0;
                CurrentCycle++;
                return true;
            }

            return false;
        }
    }
}