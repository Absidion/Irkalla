
using System;
using UnityEngine;
using TheNegative.AI.Node;

namespace TheNegative.AI
{
    public class EreshkigalAI : AI
    {
        #region Public Members
        public int MaxChainCyclesPerCycle = 2;              //The maximum amount of times that spectral chain may be used in a IE cycle
        public float SpectralChainCooldown = 45.0f;         //The cooldown of the spectral chain attack
        public float EreshkigalSafeSpace = 30.0f;           //The safe range around Ereshkigal at which you will not be super targeted
        public float CenterMapSafeSpace = 30.0f;            //The safe space around the center of the map at which Ere wants the fight to take place
        public float MaxWaitTimeAwayFromCenter = 25.0f;     //The maximum amount of time away from the center that Ere will wait before doing spectral chains attack
        public float MinimumDistanceToTarget = 10.0f;       //If you're within this minimum distance then you will be more likely to be targeted by Ere
        public float ChainTravelTime = 0.5f;                //The amount of time it takes for the Spectral Chain attack to travel to the player
        public float ChainReturnSpeed = 0.5f;               //The speed at which the chain will return to the player        
        public float LungeCooldown = 8.0f;                  //The cooldown of the lunge attack
        public float SpearThrowCooldown = 10.0f;            //The cooldown of the spear throw attack
        public int LungeDamage = 8;                         //The damage that the lunge attack does
        public float DiveTime = 1.0f;                       //The time it takes Ere to do a dive
        public float DiveSpeed = 10.0f;                     //The speed Ere travels when diving
        public float JumpHeight = 300.0f;                   //The height of Ere's jump
        public float JumpSpeed = 10.0f;                     //The speed at which Ere jumps
        public int SpearThrowDamage = 10;                   //The damage the spear throw does
        public int SpearAttack360Damage = 7;                //The damage that the 360 spear attack does
        public int SpearAttackJabDamage = 3;                //The damage that the jab attack does
        public int SpearAttackUpperStabDamage = 5;          //The damage that the upper stab attack does

        public Transform SpearTipLocation;                  //The transform of the tip of the spear (used for Spectral Spear attack)
        public Transform SpearThrowLocation;                //The transform of the location that the spear will be thrown from
        public BoxCollider SpearCollider;                   //The collider of the spear that Ere uses to attack
        public GameObject SpearProjectilePrefab;            //The prefab of the spear projectile
        public LineRenderer LocalLineRenderer;              //The local line renderer used to move the players back to the origin        
        #endregion

        #region Private Members
        private int m_SpectralChainsUsed = 0;               //The number of spectral chains used this cycle
        [SyncThis]
        protected bool m_SendingChains = false;               //Whether or not Ere is using the spectral chains attack right now
        [SerializeField]
        private bool[] m_PlayersBeingPulled;                //This determines if the players are being pulled into the center of the attack

        private InannaAI m_Inanna;                          //A reference to her sister/other boss in the room Inanna
        private Transform m_RoomCenter;                     //The location of the center of the room
        private NetworkLineRenderer m_LineRenderer;         //A reference to the network line renderer attached to ere
        private Player m_FollowUpTarget = null;             //The target that Ere will follow up on after pinning someone with the spear throw attack
                
        private Vector3 m_SpearHomePosition;                //The spear's home location used for the spear pull

        #region Animation Links
        private LinkNodeConnector m_SpearRaiseLink;             //The spear raise animation node link
        private LinkNodeConnector m_SpearLowerLink;             //The spear lower animation node link
        private LinkNodeConnector m_JumpLink;                   //The jump animation node link
        private LinkNodeConnector m_JumpIdleLink;               //The jump idle animation node link
        private LinkNodeConnector m_DiveLink;                   //The dive animation node link
        private LinkNodeConnector m_PreJumpLink;                //The prejump animation node link
        private LinkNodeConnector m_DiveRecoverLink;            //The dive animation node link
        private LinkNodeConnector m_SpearThrowLink;             //The spear throw animation node link
        private LinkNodeConnector m_SpearThrowRecoveryLink;     //The spear throw recovery animation node link
        private LinkNodeConnector m_360BuildUpLink;             //The 360 swing build up animation node link
        private LinkNodeConnector m_360SwingLink;               //The 360 swing attack animation node link
        private LinkNodeConnector m_360RecoveryLink;            //The 360 swing recovery animation node link
        private LinkNodeConnector m_UpperStabBuildUpLink;       //The upper stab build up animation node link
        private LinkNodeConnector m_UpperStabSwingLink;         //The upper stab swing animation node link
        private LinkNodeConnector m_UpperStabRecoveryLink;      //The upper stab recovery animation node link
        private LinkNodeConnector m_JabBuildUpLink;             //The jab build up animation node link
        private LinkNodeConnector m_JabSwingLink;               //The jab swing animation node link
        private LinkNodeConnector m_JabRecoveryLink;            //The jab recovery animation node link
        #endregion
        #endregion

        #region Unity Methods
        protected override void Awake()
        {
            base.Awake();
            GameManager.PlayerPopulation += OnFindInanna;
            //initalize the number of players who are going to be pulled
            m_PlayersBeingPulled = new bool[PhotonNetwork.playerList.Length];
            //find the room center obejct by tag RoomCenter to get 

            for (int i = 0; i < m_PlayersBeingPulled.Length; i++)
            {
                m_PlayersBeingPulled[i] = false;
            }

            m_RoomCenter = GameObject.FindGameObjectWithTag("RoomCenter").transform;
            m_LineRenderer = GetComponentInChildren<NetworkLineRenderer>();            
            LocalLineRenderer.enabled = false;

            MeleeCollider meleeCollider = SpearCollider.GetComponent<MeleeCollider>();
            meleeCollider.Init(-1, 10, null);              
        }

        protected override void Update()
        {
            base.Update();

            if (m_SendingChains)
                MovePlayerToCenter();

            if (m_Health.IsDead)
                HandleDeath();
        }

        private void OnFindInanna(object sender, EventArgs args)
        {
            //Access the resources directory in order to find every gameobject in the scene of type InannaAI. The reality is that only 1 AI in the scene 
            //will ever be of type InannaAI because only 1 will ever exsist in one scene, but we don't know if the gameobject witht the script is disabled so 
            //we use the resource method to find her.
            InannaAI[] inanna = Resources.FindObjectsOfTypeAll<InannaAI>();
            //Assign the first element because there will only be one in the Array
            m_Inanna = inanna[0];
        }
        #endregion

        protected override SelectorNode CreateBehaviour()
        {
            #region Spectral Chain Sequence
            //being section that deals with the attack
            #region Create the sequence for attacking
            //when ere begins to move to the center of the map she does it through this node
            AgentMoveToLocationNode moveToLocation = new AgentMoveToLocationNode(this, m_RoomCenter.position);
            //When ere's animation of her raising the chain happens
            AnimatorNode chainRaiseNode = new AnimatorNode(this, m_AnimatorRef, "IsChainRaising", ref m_SpearRaiseLink);
            //the chain move to player node which will move the chain to the player
            ChainMoveToPlayersNode chainMoveToPlayer = new ChainMoveToPlayersNode(this, SpearTipLocation, m_LineRenderer, ChainTravelTime);
            //this node determines if the chain attack is done
            HasChainReturnedNode hasChainReturned = new HasChainReturnedNode(this);
            AnimatorNode chainLowerNode = new AnimatorNode(this, m_AnimatorRef, "IsChainLowering", ref m_SpearLowerLink);
            PlaySoundNode ChainSoundNode = new PlaySoundNode(this, "ChainSound");
                
                
            //tieing all the nodes into the sequence

            SequenceNode chainAttackSection = new SequenceNode(this, "Spectral Chain Attack", moveToLocation, chainRaiseNode, chainMoveToPlayer, hasChainReturned, chainLowerNode);
            #endregion

            #region Target Selection region
            //determines if to much time has passed away from center of the map
            TimeAwayFromCenterNode timeAwayFromCenter = new TimeAwayFromCenterNode(this, m_RoomCenter.position, CenterMapSafeSpace, MaxWaitTimeAwayFromCenter);
            //determines the player's distance from Inanna 
            PlayerToCloseTargetNode playerToCloseToTargetNode = new PlayerToCloseTargetNode(this, m_Inanna.transform, MinimumDistanceToTarget);

            SelectorNode spectralChainAttackSelector = new SelectorNode(this, "Choose Attack Selector Spectral Chain", timeAwayFromCenter, playerToCloseToTargetNode);
            #endregion
            //cooldown node for spectral chain
            CooldownNode spectralChainCooldown = new CooldownNode(this, SpectralChainCooldown);
            //determines if Ere can still use chain attack
            CanUseChainsNode canUseChain = new CanUseChainsNode(this, MaxChainCyclesPerCycle);
           

            SequenceNode specrtalChainSequence = new SequenceNode(this, "Spectral Chain Sequence", spectralChainCooldown, canUseChain, spectralChainAttackSelector, ChainSoundNode, chainAttackSection);
            #endregion

            #region Ereshkigal Attack Sequence

            #region Target Selector
            #region Protecting Inanna Sequence
            //puts points into anyone to far away from Ere
            TargetingDistanceFromLocationGreaterThenNode distanceFromEre = new TargetingDistanceFromLocationGreaterThenNode(this, transform, EreshkigalSafeSpace, 1);
            //puts points into anyone to close to Anna
            TargetingDistanceFromLocationNode distanceFromAnna = new TargetingDistanceFromLocationNode(this, m_Inanna.transform, MinimumDistanceToTarget, 2);
            CalculateTargetNode calcTarget = new CalculateTargetNode(this);
            SequenceNode protectingInannaSequence = new SequenceNode(this, "Protecting Inanna Sequence", distanceFromEre, distanceFromAnna, calcTarget);
            #endregion

            TargetFollowUp followup = new TargetFollowUp(this);
            TargetSwitchNode switchTargetNode = new TargetSwitchNode(this);

            SelectorNode targetingSelector = new SelectorNode(this, "Taregting Selector", protectingInannaSequence, followup, switchTargetNode);
            #endregion

            #region Lunge Attack
            CooldownNode lungeCooldown = new CooldownNode(this, LungeCooldown);
            ToggleNavMeshAgentNode toggleAgentOff = new ToggleNavMeshAgentNode(this);
            //the crouch before the big jump animator node
            AnimatorNode preJump = new AnimatorNode(this, m_AnimatorRef, "IsPreJumping", ref m_PreJumpLink);
            //lerp and animation which will run in tandum
            LerpNode jumpLerp = new LerpNode(this, Vector3.up, 1.0f, JumpHeight, JumpSpeed);
            AnimatorNode fulljump = new AnimatorNode(this, m_AnimatorRef, "IsFullJumping", ref m_JumpLink);
            RunUntilSuceed runJumpAndAnimation = new RunUntilSuceed(this, "Run Animation and Jump", fulljump, jumpLerp);
            PlaySoundNode LungeSoundNode = new PlaySoundNode(this, "HeavyAttack2");
            PlaySoundNode JumpSoundNode = new PlaySoundNode(this, "WingFlap");
            PlaySoundNode ScreamSoundNode = new PlaySoundNode(this, "EreshkigalScream");



            AnimatorNode jumpIdle = new AnimatorNode(this, m_AnimatorRef, "IsJumpIdle", ref m_JumpIdleLink);
            LookAtTargetNode lookAtTarget = new LookAtTargetNode(this);
            ToggleMeleeColliderNode toggleLungeOn = new ToggleMeleeColliderNode(this, SpearCollider, LungeDamage);
            AnimatorNode diveAnimation = new AnimatorNode(this, m_AnimatorRef, "IsDiving", ref m_DiveLink);
            LerpToTargetNode diveLerp = new LerpToTargetNode(this, DiveTime, JumpHeight, DiveSpeed);
            RunUntilSuceed diveAtPlayer = new RunUntilSuceed(this, "Run Dive and Animation", diveAnimation, diveLerp);
            ToggleMeleeColliderNode toggleLungeOff = new ToggleMeleeColliderNode(this, SpearCollider, 0);
            ToggleNavMeshAgentNode toggleAgentOn = new ToggleNavMeshAgentNode(this);
            AnimatorNode diveRecoveryNode = new AnimatorNode(this, m_AnimatorRef, "IsInDiveRecovery", ref m_DiveRecoverLink);

            SequenceNode lungeAttackSequence = new SequenceNode(this, "Lunge Attack Sequence",
                lungeCooldown,
                toggleAgentOff,
                preJump,
                runJumpAndAnimation,
                JumpSoundNode,
                jumpIdle,
                toggleLungeOn,
                lookAtTarget,
                ScreamSoundNode,
                diveAtPlayer,
                JumpSoundNode,
                toggleLungeOff,
                toggleAgentOn,
                diveRecoveryNode);
            #endregion

            #region Spear Toss Attack
            CooldownNode spearTossCooldown = new CooldownNode(this, SpearThrowCooldown);
            AnimatorNode spearThrowAnimator = new AnimatorNode(this, m_AnimatorRef, "IsSpearBeingThrow", ref m_SpearThrowLink);
            ShootProjectileNode spearProjectileShoot = new ShootProjectileNode(this, SpearThrowDamage, SpearProjectilePrefab, SpearThrowLocation.gameObject, "SpearProjectile", 2);
            AnimatorNode spearThrowRecoveryAnimator = new AnimatorNode(this, m_AnimatorRef, "IsInSpearRecovery", ref m_SpearThrowRecoveryLink);

            SequenceNode spearTossSequence = new SequenceNode(this, "Spear Toss Sequence", spearTossCooldown, spearThrowAnimator, spearProjectileShoot, LungeSoundNode, spearThrowRecoveryAnimator);
            #endregion

            #region Basic Attack Selector

            #region 360 Attack
            CheckDistanceToTargetNode attack360DistanceCheck = new CheckDistanceToTargetNode(this, Agent.stoppingDistance);
            AnimatorNode buildUp360 = new AnimatorNode(this, m_AnimatorRef, "IsIn360BuildUp", ref m_360BuildUpLink);
            ToggleMeleeColliderNode toggleOn360Collider = new ToggleMeleeColliderNode(this, SpearCollider, SpearAttack360Damage);
            AnimatorNode swing360 = new AnimatorNode(this, m_AnimatorRef, "IsIn360Swing", ref m_360SwingLink);
            ToggleMeleeColliderNode toggleOff360Collider = new ToggleMeleeColliderNode(this, SpearCollider, 0);
            AnimatorNode recovery360 = new AnimatorNode(this, m_AnimatorRef, "IsIn360Recovery", ref m_360RecoveryLink);

            SequenceNode sequence360Attack = new SequenceNode(this, "360 Attack Sequence", attack360DistanceCheck, buildUp360, toggleOn360Collider, swing360, LungeSoundNode, toggleOff360Collider, recovery360);
            #endregion

            #region Upper Stab
            PlaySoundNode MedThrustSound = new PlaySoundNode(this, "MedAttack3");
            CheckDistanceToTargetNode attackUpperStab = new CheckDistanceToTargetNode(this, Agent.stoppingDistance);
            AnimatorNode buildUpUpperStab = new AnimatorNode(this, m_AnimatorRef, "IsInUpperStabBuildUp", ref m_UpperStabBuildUpLink);
            ToggleMeleeColliderNode toggleOnUpperStabCollider = new ToggleMeleeColliderNode(this, SpearCollider, SpearAttackUpperStabDamage);
            AnimatorNode swingUpperStab = new AnimatorNode(this, m_AnimatorRef, "IsInUpperStabSwing", ref m_UpperStabSwingLink);
            ToggleMeleeColliderNode toggleOffUpperStabCollider = new ToggleMeleeColliderNode(this, SpearCollider, 0);
            AnimatorNode recoveryUpperStab = new AnimatorNode(this, m_AnimatorRef, "IsInUpperStabRecovery", ref m_UpperStabRecoveryLink);

            SequenceNode sequenceUpperStabAttack = new SequenceNode(this, "Upper Stab Sequence", attackUpperStab, buildUpUpperStab, toggleOnUpperStabCollider, swingUpperStab, MedThrustSound, toggleOffUpperStabCollider, recoveryUpperStab);
            #endregion

            #region Jab
            PlaySoundNode PlayLightThrustSound = new PlaySoundNode(this, "LightAttack2");
            CheckDistanceToTargetNode attackJabDistanceCheck = new CheckDistanceToTargetNode(this, Agent.stoppingDistance);
            AnimatorNode buildUpJab = new AnimatorNode(this, m_AnimatorRef, "IsInJabBuildUp", ref m_JabBuildUpLink);
            ToggleMeleeColliderNode toggleOnJabCollider = new ToggleMeleeColliderNode(this, SpearCollider, SpearAttackJabDamage);
            AnimatorNode swingJab = new AnimatorNode(this, m_AnimatorRef, "IsInJabSwing", ref m_JabSwingLink);
            ToggleMeleeColliderNode toggleOffJabCollider = new ToggleMeleeColliderNode(this, SpearCollider, 0);
            AnimatorNode recoveryJab = new AnimatorNode(this, m_AnimatorRef, "IsInJabRecovery", ref m_JabRecoveryLink);

            SequenceNode sequenceJabAttack = new SequenceNode(this, "Jab Attack Sequence", attackJabDistanceCheck, buildUpJab, toggleOnJabCollider, swingJab, PlayLightThrustSound, toggleOffJabCollider, recoveryJab);
            #endregion

            ChooseRandomChildNode randomBasicAttackSelector = new ChooseRandomChildNode(this, "Basic Attack Selector, Random", sequence360Attack, sequenceJabAttack, sequenceUpperStabAttack);
            #endregion

            ApproachNode approachTargetPlayer = new ApproachNode(this);
            SelectorNode chooseAttackSelector = new SelectorNode(this, "Attack Choosing Selector", lungeAttackSequence, spearTossSequence, randomBasicAttackSelector, approachTargetPlayer);
            SequenceNode ereshkigalAttackSequence = new SequenceNode(this, "Ereshkigal Basic Attack Sequence", targetingSelector, chooseAttackSelector);
            #endregion

            SelectorNode utilitySelector = new SelectorNode(this, "UtilitySelector", specrtalChainSequence, ereshkigalAttackSequence);

            return utilitySelector;
        }

        private void MovePlayerToCenter()
        {
            foreach (Player player in m_PlayerList)
            {
                //get the line relative to the player 
                Vector3 currentPlayerLine = LocalLineRenderer.GetPosition(player.PlayerNumber * 2 - 1);

                //get the direction to the player that the line tip needs to move to 
                Vector3 directionToSpearTip = m_SpearHomePosition - currentPlayerLine;
                directionToSpearTip.Normalize();

                //calculate the amount that the "chain" must move this frame towards the origin
                Vector3 amountToMove = (directionToSpearTip * ChainReturnSpeed * Time.deltaTime) + currentPlayerLine;
                //set the position in the local line renderer to represent the new location that the line renderer will be
                LocalLineRenderer.SetPosition(player.PlayerNumber * 2 - 1, amountToMove);

                //next if the player's photon view is the local one then we need to move the player
                if (player.photonView.isMine && m_PlayersBeingPulled[player.PlayerNumber - 1])
                {
                    //calcualte the direction that the player must travel in order to get back to the origin where the spear tip is located
                    directionToSpearTip = m_SpearHomePosition - player.transform.position;
                    directionToSpearTip.Normalize();

                    //calcualte the change in frames how much the player will travel
                    player.transform.position += directionToSpearTip * Time.deltaTime * ChainReturnSpeed;

                    //next we raycast to see if the player hit any thing right behind them when being pulled. This is done to see if the chain will break
                    RaycastHit hit;
                    if (Physics.Raycast(player.transform.position, directionToSpearTip, out hit, (player.transform.position - m_SpearHomePosition).magnitude))
                    {
                        //check to see if the distance from the hit point to the player is almost nothing and if it is then break the chain
                        if ((hit.point - player.transform.position).sqrMagnitude < (MathFunc.LargeEpsilon * MathFunc.LargeEpsilon))
                        {
                            BreakChain(player);
                        }
                    }

                    //finally we determine if the chain has returned and the local player's chain should be broken
                    if (MathFunc.AlmostEquals(LocalLineRenderer.GetPosition(player.PlayerNumber * 2 - 1), LocalLineRenderer.GetPosition(0), 6.0f) ||
                        MathFunc.AlmostEquals(player.transform.position, m_SpearHomePosition, 5.0f))
                    {
                        //if the positions pretty much overlap that means that it's time for the chain to break and the player to be released back into the wild
                        BreakChain(player);
                    }
                }

                bool attackFinished = false;
                //if there's more then 1 element in the players being pulled array then check if both values are true. If both values are true then
                //the chains no longer need to be pulled and the attack is over. else the attack is over if the only element in the array is true
                if (m_PlayersBeingPulled.Length > 1)
                {
                    if (!m_PlayersBeingPulled[0] && !m_PlayersBeingPulled[1])
                        attackFinished = true;
                }
                else if (!m_PlayersBeingPulled[0])
                    attackFinished = true;

                //if the attack is done then set sendingchains to false and turn the local linerenderer off
                if (attackFinished)
                {
                    m_SendingChains = false;
                    LocalLineRenderer.enabled = false;
                }                
            }            
        }
  

        private void BreakChain(Player player)
        {
            Debug.Log("Breaking chain for player " + player.name);
            //tell the player that they are no longer immobalized
            player.IsImmobile = false;
            player.rigidbody.useGravity = true;

            //reset the player to not being pulled anymore
            photonView.RPC("TogglePlayerBeingPulled", PhotonTargets.All, player.PlayerNumber - 1, false);
        }

        [PunRPC]
        public void ActivateChainPull()
        {
            //default the positions of the lines to the spear tip
            m_SpearHomePosition = SpearTipLocation.position;
            //m_SpearHomePosition.y = -21.1f;

            LocalLineRenderer.SetPosition(0, m_SpearHomePosition);
            LocalLineRenderer.SetPosition(1, m_SpearHomePosition);
            LocalLineRenderer.SetPosition(2, m_SpearHomePosition);
            LocalLineRenderer.SetPosition(3, m_SpearHomePosition);

            for (int i = 0; i < m_PlayersBeingPulled.Length; i++)
            {
                m_PlayersBeingPulled[i] = true;
                //set the position relative to the player to be equal to the player's position
                LocalLineRenderer.SetPosition(PlayerList[i].PlayerNumber * 2 - 1, PlayerList[i].transform.position);
            }
            LocalLineRenderer.enabled = true;
            m_SpectralChainsUsed++;
            m_SendingChains = true;
        }

        [PunRPC]
        private void TogglePlayerBeingPulled(int index, bool leaver)
        {
            m_PlayersBeingPulled[index] = leaver;
            Debug.Log("Player is no longer being pulled, " + m_PlayerList[index].name);
        }

        #region Sound Methods
        protected override void PlayDeathSound()
        {
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "EreshkigalScream", transform.position);
        }

        protected override void PlayInjurySound()
        {
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "EreshkigalInjured2", transform.position);
        }
        #endregion

        #region Animation Events
        #region Spectral Chain Animation Events
        public void EventFinishedSpearRaise()
        {
            m_SpearRaiseLink();
        }

        public void EventFinishedSpearLower()
        {
            m_SpearLowerLink();
        }
        #endregion  

        #region Lunge Animation Events
        public void EventFinishedPreJump()
        {
            m_PreJumpLink();
        }

        public void EventFinishedJumping()
        {
            m_JumpLink();
        }

        public void EventFinishedJumpIdle()
        {
            m_JumpIdleLink();
        }

        public void EventFinishedDive()
        {
            m_DiveLink();
        }

        public void EventFinishedDiveRecovery()
        {
            m_DiveRecoverLink();
        }
        #endregion  

        #region Spear Throw
        public void EventFinishedSpearThrow()
        {
            m_SpearThrowLink();
        }

        public void EventFinishedSpearThrowRecovery()
        {
            m_SpearThrowRecoveryLink();
        }
        #endregion  

        #region 360 Swing Animation Events
        public void EventFinished360BuildUp()
        {
            m_360BuildUpLink();
        }

        public void EventFinished360Swing()
        {
            m_360SwingLink();
        }

        public void EventFinished360Recovery()
        {
            m_360RecoveryLink();
        }
        #endregion

        #region UpperStab Animation Event Region
        public void EventFinishedUpperStabBuildUp()
        {
            m_UpperStabBuildUpLink();
        }

        public void EventFinishedUpperStabSwing()
        {
            m_UpperStabSwingLink();
        }

        public void EventFinishedUpperStabRecovery()
        {
            m_UpperStabRecoveryLink();
        }
        #endregion

        #region Jab Animation Events
        public void EventFinishedJabBuildUp()
        {
            m_JabBuildUpLink();
        }

        public void EventFinishedJabSwing()
        {
            m_JabSwingLink();
        }

        public void EventFinishedJabRecovery()
        {
            m_JabRecoveryLink();
        }
        #endregion  
        #endregion

        #region Properties
        public int SpectralChainsUsed { get { return m_SpectralChainsUsed; } set { m_SpectralChainsUsed = value; } }
        public Player FollowUpTarget { get { return m_FollowUpTarget; } set { m_FollowUpTarget = value; } }
        public bool SendingChains { get { return m_SendingChains; } set { m_SendingChains = value; } }
        #endregion
    }
}