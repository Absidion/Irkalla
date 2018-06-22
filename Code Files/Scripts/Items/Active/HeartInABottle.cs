namespace TheNegative.Items
{
    public class HeartInABottle : AbstractActiveRoomBased
    {        
        private int m_HealValue = 0;

        public HeartInABottle()
        {
            m_RoomCount = 3;
            m_RoomCooldown = 0;
            m_HealValue = 10;
            m_SpriteName += "HeartInABottle";
            m_Tooltip = "Heals on use";
        }

        public override void UpdateItem()
        {
            //reset the items cooldown
            ResetCooldown();

            m_Player.Health.HP += m_HealValue;
            //cap the HP to be the max hp if the player gained more hp then they can hold
            if (m_Player.Health.HP > m_Player.Health.MaxHp)
            {
                m_Player.Health.HP = m_Player.Health.MaxHp;
            }
        }
    }
}