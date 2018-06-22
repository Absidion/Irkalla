using TheNegative.AI.Node;
using UnityEngine;

//Writer: Liam
//Last Updated: 1/5/2018

namespace TheNegative.AI
{
    public class ShadowDemonAI : AI
    {
        public int TouchDamage;             //the Shadow demon touch damage

        private WorshipperAI m_MasterRef;         //reference to the health of the cultist that summoned this AI

        public WorshipperAI MasterRef { get { return m_MasterRef; } set { m_MasterRef = value; } }

        protected override SelectorNode CreateBehaviour()
        {
            //craete the targeting sequence
            TargetingDistanceNode targetingDist = new TargetingDistanceNode(this, 1);
            CalculateTargetNode calculateTarget = new CalculateTargetNode(this);

            //assign targeting sequence
            SequenceNode targetingSequence = new SequenceNode(this, "Targeting Sequence", targetingDist, calculateTarget);

            ApproachNode approachPlayer = new ApproachNode(this);

            //create the attack player sequence    
            SequenceNode attackPlayer = new SequenceNode(this, "Attack&TargetSequence", targetingSequence, approachPlayer);

            SelectorNode utlitySelector = new SelectorNode(this, "UtilitySelector", attackPlayer);

            return utlitySelector;
        }

        protected override void Update()
        {
            base.Update();

            if (!PhotonNetwork.isMasterClient)
                return;

            if (MasterRef != null)
            {
                if (MasterRef.Health.IsDead)
                    HandleDeath();
            }

            if (Health.IsDead)
                HandleDeath();
           
        }

        protected override void PlayInjurySound()
        {
            
        }

        protected override void PlayDeathSound()
        {
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "Dead2", transform.position);
        }

        protected override void HandleDeath()
        {
            SetActive(false);
            MasterRef.NumberOfShadowDemonsSpawned--;
            Health.ResetHealth();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!PhotonNetwork.isMasterClient)
                return;

            if (collision.gameObject.tag == "Player")
            {
                Player player = collision.gameObject.GetComponent<Player>();
                if (player != null)
                {
                    player.photonView.RPC("TakeDamage", PhotonTargets.All, TouchDamage, transform.position, ElementalDamage.ToArray());
                    SetActive(false);
                }
            }
        }
    }
}