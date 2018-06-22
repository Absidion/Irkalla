using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
    //XML data for the Shop XML(later use)
    //<Name>TheNegative.Items.ChainDamage</Name>
    //<Value>50</Value>
    public class ChainDamage : AbstractPassive
    {
        public const float ShockRange        = 7.0f;        // The range that the Shock can jump
        public const float ChanceToShock     = 20.0f;       // The chance out of 100 to deal the shock damage
        public const int   ShockDamage       = 8;           // The damage that the shock deals
        public ChainDamage()
        {
            m_NeedsUpdate =      false;
            m_SpriteName +=     "TieThatBinds"; //TODO: change this to a name of a existing sprite for this item
            m_Name =            "ChainDamage";
            m_Description =     "Damage you deal will spread like a disease";
            m_Tooltip =         "Damage dealt spreads like a disease";
            m_NetworkAffect =    true;
            m_NetworkVisual =    true;
        }

        public override void ActivateItem(Player player)
        {
            //Check if the last player who dealt Damage was this player
            if (DamageCountManager.instance.PlayerWhoLastDealtDamage != -1)
            {
                if (DamageCountManager.instance.PlayerWhoLastDealtDamage == player.PlayerNumber)
                {
                    //roll for the change to shock
                    int roll = Random.Range(0, 100);
                    if (roll <= ChanceToShock)
                    {
                        //Get all the colliders in range of the shockRange a
                        Collider[] CollidersInRange;
                        Vector3 AIPos = DamageCountManager.instance.AIWhichLastTookDamage.transform.position;
                        CollidersInRange = Physics.OverlapSphere(AIPos, ShockRange, 0);
                        if (CollidersInRange.Length > 0)
                        {
                            ShockNearbyEnemies(CollidersInRange);
                        }
                    }
                }
            }
        }

        public override void UpdateNetworkVisual()
        {
            //TODO: Add a visual to this item
        }
        public override void UpdateItem() { }

        private void ShockNearbyEnemies(Collider[] CollidersInRange)
        {
            //Loop though all the colliders and find all the AI in Range
            foreach (Collider collider in CollidersInRange)
            {
                //If the collider has an AI attached to it Apply damage to the AI
                AI.AI colliderAI = collider.GetComponent<AI.AI>();
                if (collider != null)
                {
                    colliderAI.TakeDamage(DamageCountManager.instance.PlayerWhoLastDealtDamage, ShockDamage, null, 1);
                }
            }
        }

    }
}
