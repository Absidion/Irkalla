using TheNegative.AI.Node;
using UnityEngine;

//Writer: Liam
//Last Updated: 12/15/2017

namespace TheNegative.AI
{
    public class PhoenixAI : AI
    {
        public GameObject DebrisPrefab;                     //The prefab for the debris that gets shot out
        public Health CoreHP;                               //public value to store the CoreHP component in order to deal with if the Core is destroyed or if the body is destroyed
        public float RebuildTime = 5.0f;                    //how long it takes the pheonix to rebuild        

        private bool m_IsBodyDestroyed = false;             //determines whether or not the body is destroyed or not
        private float m_Timer = 0.0f;                       //timer for how long it will take the phoenix to rebuild

        protected override void Awake()
        {
            base.Awake();

            //make sure the agent is disabled
            m_Agent.enabled = false;

            //check to see if the photon view's observed component's list contains the CoreHP component. If it doesn't we need to add this to the list of observed components
            //TODO: CHANGE SO THAT THE CORE HP HAS IT'S OWN VIEW FOR MANAGING THE HP THERE
            if (!photonView.ObservedComponents.Contains(CoreHP))
            {
                photonView.ObservedComponents.Add(CoreHP);
            }

            rigidbody.useGravity = false;

            if (CoreHP == null)
            {
                Debug.LogError("CoreHP is null please put the phoenix's core Health component on CoreHP editor value", this);
            }
        }

        protected override void Update()
        {
            if (!PhotonNetwork.isMasterClient)
                return;

            HandleDeath();

            if (m_IsBodyDestroyed)
            {
                RestoreBody();
            }
            else
            {
                base.Update();
            }
        }

        protected override void PlayInjurySound()
        {
            
        }

        protected override void PlayDeathSound()
        {

        }

        //restores the phoenix's body to it's gloryious status
        private void RestoreBody()
        {
            if (m_Timer >= RebuildTime)
            {
                //reactivate the mesh and reset values
                SetRendererActive(true);
                m_IsBodyDestroyed = false;
                m_Health.ResetHealth();
                m_Timer = 0.0f;
            }
            else
            {
                m_Timer += Time.deltaTime;
            }
        }

        protected override void HandleDeath()
        {
            if (CoreHP.HP <= 0)
            {                
                base.HandleDeath();
            }
            else if (Health.HP <= 0)
            {
                //set the body to be destroyed
                m_IsBodyDestroyed = true;

                //set the phoenix mesh to be deactivated
                SetRendererActive(false);

                //reset the node and set the current one to null
                m_BehaviourTree.RootNode.Reset();
            }
        }

        protected override SelectorNode CreateBehaviour()
        {
            //create attack nodes
            KamikazeNode kamikazeNode = new KamikazeNode(this);
            GustNode gustNode = new GustNode(this, DebrisPrefab);
            SwoopNode swoopNode = new SwoopNode(this);

            //create the flyabove node
            FlyAboveTargetNode flyAboveTargetNode = new FlyAboveTargetNode(this);

            //create the attack selector and attach the above nodes in the following sequence : KamakaziNode, GustNode, SwoopNode, FlyAboveTargetNode
            SelectorNode attackSelector = new SelectorNode(this, "AttackSelector");
            attackSelector.AddChildren(kamikazeNode,
                                       gustNode,
                                       swoopNode,
                                       flyAboveTargetNode);

            //create the targeting nodes
            TargetingDistanceNode targetingDistanceNode = new TargetingDistanceNode(this, 1);
            TargetingLowHealthNode targetingLowHealthNode = new TargetingLowHealthNode(this, 1);
            TargetingHighestDamageNode targetingHighestDamageNode = new TargetingHighestDamageNode(this, 1);
            TargetingCharacterType targetingCharacterType = new TargetingCharacterType(this, 1, WeaponType.MELEE);
            CalculateTargetNode calculateTargetNode = new CalculateTargetNode(this);

            //create the targeting logic from the above nodes. The order is as follows: 
            SequenceNode targetingSequence = new SequenceNode(this, "TargetingSequence");
            targetingSequence.AddChildren(targetingDistanceNode,
                                          targetingLowHealthNode,
                                          targetingHighestDamageNode,
                                          targetingCharacterType,
                                          calculateTargetNode);

            //create the sequence for having a target behaviour (ie attacking, flying above players)
            SequenceNode attackSequence = new SequenceNode(this, "GetTargetAndAttack");
            attackSequence.AddChildren(targetingSequence, attackSelector);

            //create the utility selector and return it
            SelectorNode utilitySelector = new SelectorNode(this, "UtilitySelector");
            utilitySelector.AddChildren(attackSequence);

            return utilitySelector;
        }
    }
}