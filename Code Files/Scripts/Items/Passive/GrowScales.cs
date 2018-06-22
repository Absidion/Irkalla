using UnityEngine;

namespace TheNegative.Items
{
    public class GrowScales : AbstractPassive
    {
        private int ExtraArmor = 25;
        private Player m_MyPlayer;
        private float m_MaxTimer = 60.0f;
        private float m_RunningTimer = 60.0f;
        private bool m_HasArmor = false;

        public GrowScales()
        {
            m_NeedsUpdate = true;
            m_SpriteName += "GrowScales";
            m_Name = "GrowScales";
            m_Description = "If you don't have armor for a minute, gain some armor.";
            m_Tooltip = "Everyone needs a little extra help sometimes.";
            m_NetworkAffect = false;
            m_NetworkVisual = false;
        }

        public override void ActivateItem(Player player)
        {
            m_MyPlayer = player;
            m_HasArmor = m_MyPlayer.Health.HasArmor;
        }

        public override void UpdateItem()
        {
            m_HasArmor = m_MyPlayer.Health.HasArmor;

            if (!m_HasArmor)
            {
                m_RunningTimer -= Time.deltaTime;
                if (m_RunningTimer < 0.0f)
                {
                    m_MyPlayer.Health.ArmorHP += ExtraArmor;
                    m_RunningTimer = m_MaxTimer;
                }
            }
        }
    }
}
