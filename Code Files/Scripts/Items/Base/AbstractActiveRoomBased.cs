using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
    [Serializable]
    public abstract class AbstractActiveRoomBased : AbstractItem
    {
        protected Player m_Player;
        protected bool m_IsActivated;
        protected int m_RoomCount;
        protected int m_RoomCooldown;

        public AbstractActiveRoomBased()
        {
            m_ItemType = ItemType.ActiveRoom;
            m_IsActivated = false;
        }

        public override void ActivateItem(Player player)
        {
            m_Player = player;
        }

        public virtual void DecrementRoomCooldown()
        {
            m_RoomCooldown--;
            if(m_RoomCooldown <= 0)
            {
                m_RoomCooldown = 0;
                m_IsActivated = true;
            }
        }

        public virtual void ResetCooldown()
        {
            m_RoomCooldown = m_RoomCount;
        }
        
        public int RoomCount { get { return m_RoomCount; } set { m_RoomCount = value; } }
        public int RoomCooldown { get { return m_RoomCooldown; } set { m_RoomCooldown = value; } }
        public bool IsActivated { get { return m_IsActivated; } set { m_IsActivated = value; } }
    }
}
