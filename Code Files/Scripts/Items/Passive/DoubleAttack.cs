using UnityEngine;

namespace TheNegative.Items
{
    public class DoubleAttack : AbstractPassive
    {

        public DoubleAttack()
        {
            m_NeedsUpdate = false;
            m_SpriteName += "DoubleAttack";
            m_Name = "DoubleAttack";
            m_Description = "Attacks Trigger Cost, and only for the cost of one.";
            m_Tooltip = "Attacks Doubled";
            m_NetworkAffect = false;
            m_NetworkVisual = false;
        }

        public override void ActivateItem(Player player)
        {
            player.NumOfAttacks = 2;
        }

        public override void UpdateNetworkAffect() { }

        public override void UpdateNetworkVisual() { }

        public override void UpdateItem() { }
    }
}
