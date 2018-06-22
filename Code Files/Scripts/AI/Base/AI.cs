using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using TheNegative.AI.Node;

//Writer: Liam
//Last Updated: Liam 12/30/2017

namespace TheNegative.AI
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(Rigidbody), typeof(Health), typeof(NavMeshAgent))]
    public abstract class AI : LiamBehaviour, AITakeDamageInterface
    {
        #region Public Members        
        public int AIWeight;                                        //This AI's specific weight value
        public float DefaultSpeed;                                  //The AI character's default speed
        public float DetectionRange;                                //The AI character's detection range. The range at which it can detect Players
        public float FadeOutSpeed = 1.0f;                           //The speed at which the AI will fade out at
        public PaletteType PaletteSwap = PaletteType.None;          //The type of palette that the AI is/should be swaped to
        public List<Status> ElementalDamage;                        //The type of elemental damage that the AI can deal
        public DamageTakenDelegate OnDamageTaken;                   // The Delegate for tossing out the event        
        #endregion

        #region Protected Members
        protected BehaviourTree m_BehaviourTree = null;                               //The AI character's behaviour tree

        protected Player m_Target = null;                                      //The AI character's target
        protected List<Player> m_PlayerList = null;                            //The list of Players in the current game

        protected NavMeshAgent m_Agent = null;                                 //The AI character's NavMesh Agent
        protected MeshRenderer m_Renderer = null;                              //The MeshRenderer of the AI character                
        protected Health m_Health = null;                                      //The AI character's health component        

        protected IslandRoom m_MyIslandRoom = null;                            //The island room that the AI has been assigned to

        protected Dictionary<int, float> m_DamageTaken = null;                 //A dictionary to store how much damage the AI has taken and from which source
        protected Dictionary<Status, float> m_EffectsAppliedToAI = null;       //A dictionary to store the effects that are currently applied to the AI
        protected Dictionary<Status, GameObject> m_WorldEffects = null;        //gameobjects that contain shaders/particle systems for statuses that get attached to the AI
        protected Dictionary<int, int> m_TargetScores = null;                  //A dictionary to store the scores of each player 
        protected List<Status> m_StatusesToBeRemoved = null;                   //A list of statuses that are no longer affecting the AI and need to be removed
        protected float m_AmountSlowed = 0.0f;                                 //The amount the AI is slowed down  
        protected float m_OldAmountSlowed = 0.0f;                              //The previous amount that the AI was slowed down by
        protected bool m_IsImmobile = false;                                   //Whether or not the AI is able to inact any behaviours from its behaviour tree

        protected int m_TargetLayerMask = -1;                                  //The AI's layermask for dealing damage to the target
        private int m_IgnoreLayerMask = -1;                                    //The AI's layermask for which targets it should ignore with hitscan

        protected Animator m_AnimatorRef = null;

        [SyncThis]
        protected bool m_ShouldFadeOut = false;                                   //Whether or not the AI should begin the fade out process
        [SyncThis]
        protected bool m_DeathAnimationStarted = false;                           //Whether or not the death animation has begun to play
        #endregion

        #region Private Memebers
        private bool m_IsInitialized = false;                                   //Whether or not the AI has been initialized

        private float m_FadeOutValue = 1.0f;                                    //The alpha value of the current fade out        
        private List<Material> m_MaterialsThatFade = null;                      //A list of the materials that will need be faded out over time
        #endregion  

        #region Unity Monobehaviour methods

        protected override void Awake()
        {
            base.Awake();
            OnDamageTaken += DamageCountManager.instance.OnTakeDamageEvent;
            //get the components attached to the AI
            {
                m_Health = GetComponent<Health>();
                if (m_Health == null)
                {
                    Debug.LogError("Health component is null. Please attach Health component to AI " + name, this);
                }

                m_Agent = GetComponent<NavMeshAgent>();
                if (m_Agent == null)
                {
                    Debug.LogError("NavMeshAgent component is null. Please attach NavMeshAgent component to AI " + name, this);
                }
            }

            //initalize arrays, lists, dictionarys, etc...
            {
                m_TargetScores = new Dictionary<int, int>();

                m_DamageTaken = new Dictionary<int, float>();

                m_EffectsAppliedToAI = new Dictionary<Status, float>();

                m_WorldEffects = new Dictionary<Status, GameObject>();

                m_StatusesToBeRemoved = new List<Status>();

                m_PlayerList = new List<Player>();

                m_AnimatorRef = GetComponent<Animator>();
            }

            //set certain default values
            {
                m_Agent.speed = DefaultSpeed;

                m_TargetLayerMask = LayerMask.GetMask("Player");

                m_IgnoreLayerMask = ~LayerMask.GetMask("Enemy", "Room", "MapGeometry");

                m_BehaviourTree = new BehaviourTree(this);
            }

            //Handle the fading out of AI
            {
                m_MaterialsThatFade = new List<Material>();

                GetFadeOutMaterials(transform);
            }

            ChangeAIPaletteType();
        }

        protected override void Update()
        {
            base.Update();

            if (!PhotonNetwork.isMasterClient || !m_IsInitialized || Health.IsDead)
                return;

            //reset the player's target scores each frame
            foreach (Player player in m_PlayerList)
            {
                m_TargetScores[player.PlayerNumber] = 0;
            }

            //update the amount slowed
            UpdateSlowAmount();

            if (!m_IsImmobile)
            {
                //update the behaviour tree
                m_BehaviourTree.UpdateBehaviourTree();
            }

            //update for how much longer the AI is afflicted with statuses
            UpdateStatusTimers();
        }

        protected virtual void FixedUpdate()
        {
            if (!PhotonNetwork.isMasterClient && m_IsInitialized)
                return;

            if (!m_IsImmobile && m_BehaviourTree != null)
            {
                //do any physics based node logic within the behaviour tree
                m_BehaviourTree.FixedUpdateBehaviourTree();
            }
        }

        protected virtual void LateUpdate()
        {
            if (!PhotonNetwork.isMasterClient && m_IsInitialized)
                return;

            if (m_BehaviourTree != null)
                //do any late updating logic within the behaviour tree          
                m_BehaviourTree.LateUpdateBehaviourTree();

            UpdateAIAnimations();
        }

        #endregion

        #region Private Methods
        private void ChangeAIPaletteType()
        {
            switch (PaletteSwap)
            {
                //if there is no palette swap required then return out of the function so that it doesn't get set
                case PaletteType.None:
                    break;

                //if the palette type if Fire then we need to change the shader and then add the Burn status
                case PaletteType.Fire:
                    ChangeShader("Fire");
                    ElementalDamage.Add(Status.Burn);
                    break;

                //if the palette type is Ice then we need to change the shader and then add the Freeze status
                case PaletteType.Ice:
                    ChangeShader("Ice");
                    ElementalDamage.Add(Status.Freeze);
                    break;

                //if the palette type if Poison then we need to change the shader and then add the Poison status
                case PaletteType.Poison:
                    ChangeShader("Poison");
                    ElementalDamage.Add(Status.Poison);
                    break;
            }
        }

        private void ChangeShader(string shaderName)
        {
            Material mat = new Material(Shader.Find("Shaders/" + shaderName));
            m_Renderer.material = mat;
        }

        //Recursive method that will filter through children until it gets all materials that attached to the initial transform
        private void GetFadeOutMaterials(Transform iteratedTransform)
        {
            for (int i = 0; i < iteratedTransform.childCount; i++)
            {
                if (iteratedTransform.childCount > 1)
                {
                    GetFadeOutMaterials(iteratedTransform.GetChild(i));
                }

                Renderer renderer = iteratedTransform.GetChild(i).GetComponent<Renderer>();
                if (renderer != null)
                {
                    foreach (Material material in renderer.materials)
                    {
                        m_MaterialsThatFade.Add(material);
                    }

                    continue;
                }
            }
        }
        #endregion

        #region Public Methods

        //This method needs to be called if anything specific to certain AI needs to be activated when they are
        public virtual void ActivateEnemy() { }

        public virtual void AddStatusEffect(Status effect)
        {
            //if the status effect is already on the player, reset the timer
            if (m_EffectsAppliedToAI.ContainsKey(effect))
            {
                m_EffectsAppliedToAI[effect] = 0.0f;
            }
            else
            {
                m_EffectsAppliedToAI.Add(effect, 0.0f);

                //add to damage per second, amount slowed and change if player is immobile depending on the effect 
                m_Health.StatusDamagePerSecond += StatusManager.GetDOTFromStatus(effect);
                m_AmountSlowed += StatusManager.GetSlowValuesFromStatus(effect);
                m_IsImmobile = StatusManager.GetIsImmobalizedFromStatus(effect);

                ////add world effect to AI depending on the effect
                //GameObject worldEffect = StatusManager.GetWorldEffectFromStatus(effect);

                //if (worldEffect != null)
                //{
                //    worldEffect.transform.parent = gameObject.transform;    //attach the world effect to the AI
                //    worldEffect.transform.localPosition = Vector3.zero;     //reset it's position in local space
                //    worldEffect.SetActive(true);                            //set to active to enable effect
                //    m_WorldEffects.Add(effect, worldEffect);                //keep a reference to the world effect to remove it later
                //}
            }
        }

        //Initializes all required components and values appropriately
        public virtual void Init()
        {
            if (m_IsInitialized == true)
                return;

            //in order to create the behaviour tree we need to call this abstract method and get the utility selector that is returned
            //and save it into the behaviour tree's root node since what ever is returned is the root node
            m_BehaviourTree.RootNode = CreateBehaviour();

            //get the list of players that are in the game
            Player[] players = FindObjectsOfType<Player>();

            foreach (Player p in players)
            {
                //if neither the player list or the player we're iterating over are null that means that we can add the player to both the player list and the damage list
                if ((p != null) && (m_PlayerList != null))
                {
                    m_PlayerList.Add(p);
                    m_DamageTaken.Add(p.PlayerNumber, 0);
                    m_TargetScores.Add(p.PlayerNumber, 0);
                }
                else
                {
                    Debug.LogError("Player p is null or the player list in the AI " + name + " is null. Please check to to make sure neither have an error.", this);
                }
            }
            //call the init of all behaviour trees
            m_BehaviourTree.RootNode.Init();

            m_IsInitialized = true;


            //the reason why this is required is that if we try to sync a position accross the network and the client has the navmesh enabled then that will conflict with the
            //position syncing that is attempting to be done
            if (PhotonNetwork.isMasterClient)
                Agent.enabled = true;
        }
        #endregion

        #region Protected Methods

        //This method should be used to constuct the AI's behaviour tree. This method should return the Utility Selector / Root Node of the tree
        protected abstract SelectorNode CreateBehaviour();

        //This method should be used when an AI gets injured.
        protected abstract void PlayInjurySound();
        //This method should be used when an AI gets killed.
        protected abstract void PlayDeathSound();

        //Method will handle the death case for the AI. The base will stop any and all node functionality that needs to be stopped. It will also remove the AI from the scene accross the network,
        //decrement the enemy in room count and also spawn some loot into the world.
        protected virtual void HandleDeath()
        {
            if (m_ShouldFadeOut)
            {
                Fadeout();
            }


            //if the AI has died for the first time then we make sure to play the sound and stop the behaviour tree
            if (!m_DeathAnimationStarted)
            {
                SwapMaterialValues();

                if (!PhotonNetwork.isMasterClient)
                    return;

                m_BehaviourTree.Finish();

                PlayDeathSound();
                m_AnimatorRef.SetBool("IsDead", m_DeathAnimationStarted = true);
            }
        }

        protected virtual void SwapMaterialValues()
        {
            foreach (Material m in m_MaterialsThatFade)
            {
                GraphicUtilis.ChangeRenderMode(m, GraphicUtilis.BlendMode.Transparent);
            }
        }

        protected virtual void Fadeout()
        {
            m_FadeOutValue -= Time.deltaTime;

            foreach (Material material in m_MaterialsThatFade)
            {
                if (material.HasProperty("_Color"))
                {
                    Color color = material.color;
                    color.a = Mathf.Clamp(m_FadeOutValue, 0, 1);
                    material.color = color;
                }
            }

            if (m_FadeOutValue <= 0.0f)
            {
                gameObject.SetActive(false);

                if (PhotonNetwork.isMasterClient)
                {
                    m_MyIslandRoom.RemoveEnemyFromRoom(this);
                    RoomManager.Instance.DecrementEnemyCount(MyIslandRoom);

                    //drop rate is affected by the luck of the player that did the most damage
                    int playerIndex = -1;       //the number of the player that did the most damage
                    float highestDamage = 0.0f; //temporary highest damage variable to compare values in the for loop
                    float luck = 0.0f;          //the final luck value that will be used to affect the drop rate

                    //iterate through the damage taken dictionary to get the player number of the player who did the most damage
                    foreach (KeyValuePair<int, float> playerDamage in m_DamageTaken)
                    {
                        if (playerDamage.Value > highestDamage)
                        {
                            playerIndex = playerDamage.Key;
                        }
                    }

                    //iterate through the players to grab the luck of the player with the highest damage
                    foreach (Player player in m_PlayerList)
                    {
                        if (player.PlayerNumber == playerIndex)
                        {
                            luck = player.GetStat(StatType.LUCK);
                        }
                    }

                    ItemManager.Instance.SpawnItemIntoWorld(transform.position, luck);
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }

        protected void EventFinishedDeath()
        {
            m_ShouldFadeOut = true;
        }

        //Update the status timers and pop them off when done
        protected virtual void UpdateStatusTimers()
        {
            //clear the list of statuses to be removed if there is any in the list
            if (m_StatusesToBeRemoved.Count != 0)
                m_StatusesToBeRemoved.Clear();

            foreach (Status key in m_EffectsAppliedToAI.Keys.ToList())
            {
                m_EffectsAppliedToAI[key] += Time.deltaTime;

                //if the timer has passed the time set, add it to be removed
                if (m_EffectsAppliedToAI[key] >= StatusManager.GetTimerFromStatus(key))
                {
                    //subtract the status amount from relevant values
                    m_Health.StatusDamagePerSecond -= StatusManager.GetDOTFromStatus(key);
                    m_AmountSlowed -= StatusManager.GetSlowValuesFromStatus(key);

                    //if it was a stun effect, set the player to not be stunned anymore
                    if (StatusManager.GetIsImmobalizedFromStatus(key) == true)
                    {
                        m_IsImmobile = false;
                    }

                    //add status to be removed
                    m_StatusesToBeRemoved.Add(key);
                }
            }

            //Remove all statuses marked for deletion
            for (int i = 0; i < m_StatusesToBeRemoved.Count; i++)
            {
                //unparent world effect from the player and return it to the pool
                //m_WorldEffects[m_StatusesToBeRemoved[i]].SetActive(false);
                //m_WorldEffects[m_StatusesToBeRemoved[i]].transform.parent = null;
                //m_WorldEffects.Remove(m_StatusesToBeRemoved[i]);

                //remove status effect
                m_EffectsAppliedToAI.Remove(m_StatusesToBeRemoved[i]);
            }
        }

        //Update the speed whenever the slow amount has been changed
        protected virtual void UpdateSlowAmount()
        {
            //if the slow amount has changed
            if (m_AmountSlowed != m_OldAmountSlowed)
            {
                if (m_AmountSlowed > m_OldAmountSlowed)
                {
                    m_Agent.speed -= m_AmountSlowed - m_OldAmountSlowed; //if the slow amount has increased, decrease speed more by the difference
                    m_OldAmountSlowed = m_AmountSlowed;                  //update the old slow amount
                }
                else if (m_AmountSlowed < m_OldAmountSlowed)
                {
                    m_Agent.speed += m_OldAmountSlowed - m_AmountSlowed; //if the slow amount has decreased, increase speed more by the difference
                    m_OldAmountSlowed = m_AmountSlowed;                  //update the old slow amount
                }
            }
        }

        //implement all animation triggers here in child AI classes
        protected virtual void UpdateAIAnimations()
        {

        }

        //when overriden, supposed to return true if any animation triggers are true
        protected virtual bool CurrentlyInAnimation()
        {
            return false;
        }

        #endregion

        #region Properties

        public Player Target { get { return m_Target; } set { m_Target = value; } }
        public List<Player> PlayerList { get { return m_PlayerList; } set { m_PlayerList = value; } }
        public NavMeshAgent Agent { get { return m_Agent; } }
        public MeshRenderer Renderer { get { return m_Renderer; } }
        public Health Health { get { return m_Health; } }
        public Dictionary<int, float> DamageTaken { get { return m_DamageTaken; } }
        public Dictionary<int, int> Scores { get { return m_TargetScores; } }
        public float Speed { get { return m_Agent.speed; } set { m_Agent.speed = value; } }
        public float SpeedReset { set { m_Agent.speed = DefaultSpeed; } }
        public int TargetLayerMask { get { return m_TargetLayerMask; } }
        public IslandRoom MyIslandRoom { get { return m_MyIslandRoom; } set { m_MyIslandRoom = value; } }
        public int IgnoreLayerMask { get { return m_IgnoreLayerMask; } set { m_IgnoreLayerMask = value; } }

        #endregion

        #region Photon RPCs and Callbacks


        [PunRPC]
        public void SendOnDamageEvent(int Damage, int playernum, int exteriorMultiplier)
        {
            Color col = Color.white;
            if (exteriorMultiplier < 1)
                col = Color.black;
            else if (exteriorMultiplier > 1)
                col = Color.red;

            DamageTakenArgs args = new DamageTakenArgs(Damage, this.Health, playernum, col);

            OnDamageTaken(this, args);
        }

        [PunRPC]
        //Update the health component's current state and also update the list of statuses that the AI is afflicted with. Also add the damage to the AI's damage taken dictionary
        public virtual void TakeDamage(int playerNumber, int damage, Status[] statusEffects, int exteriorMultiplier)
        {
            if (m_DeathAnimationStarted || m_ShouldFadeOut)
                return;

            int DamageTaken = m_Health.TakeDamage(damage) * exteriorMultiplier;

            photonView.RPC("SendOnDamageEvent", PhotonTargets.All, DamageTaken, playerNumber, exteriorMultiplier);

            //calculate player lifesteal
            foreach (Player player in m_PlayerList)
            {
                if (player.PlayerNumber == playerNumber)
                {
                    player.photonView.RPC("ActivateHitMarker", PhotonTargets.AllViaServer, playerNumber);

                    if (player.LifeSteal > 0)
                    {
                        //if player hs life steal, increase their hp and clamp to their max hp
                        player.Health.HP += Mathf.RoundToInt(player.LifeSteal * player.Health.HP);
                        player.Health.HP = Mathf.Clamp(player.Health.HP, 0, player.Health.MaxHp);
                    }
                }
            }

            if (statusEffects != null)
            {
                for (int i = 0; i < statusEffects.Length; i++)
                {
                    AddStatusEffect(statusEffects[i]);
                }
            }

            m_DamageTaken[playerNumber] += DamageTaken; //increase individual damage count
            PlayInjurySound();
        }


        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            //get all of the remainning players in the game
            PhotonPlayer[] playersInGame = PhotonNetwork.playerList;
            //create an array to store the player numbers of the players in the game
            int[] playerNumbers = new int[playersInGame.Length];

            //loop and assign all of the player numbers appropriately
            for (int i = 0; i < playersInGame.Length; i++)
            {
                playerNumbers[i] = (int)playersInGame[i].CustomProperties["PlayerNumber"];
            }

            //compare the player numbers in the array to any players in the list
            for (int i = 0; i < m_PlayerList.Count; i++)
            {
                if (m_PlayerList[i] == null)
                {
                    m_PlayerList.RemoveAt(i);
                }

                //check to see if the list of players in the game contains the current player we are checking against
                if (!playerNumbers.Contains(m_PlayerList[i].PlayerNumber))
                {
                    //if it's not in the list that means that the player doesn't actually exsit in the game anymore and needs to be removed from the list to prevent errors
                    m_PlayerList.RemoveAt(i);
                }
            }
        }

        #endregion
    }

}