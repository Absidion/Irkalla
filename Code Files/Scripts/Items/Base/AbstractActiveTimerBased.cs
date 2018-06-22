using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
    [Serializable]
    public abstract class AbstractActiveTimerBased : AbstractItem
    {
        protected Player m_Player;
        protected bool m_IsActivated;
        protected float m_CooldownTime = 0.0f;
        protected float m_Timer = 0.0f;

        public AbstractActiveTimerBased()
        {
            m_ItemType = ItemType.ActiveTimer;
            m_IsActivated = false;
        }

        public override void ActivateItem(Player player)
        {
            m_Player = player;
        }

        public virtual void ResetTimer()
        {
            m_Timer = 0.0f;
        }

        public float Timer { get { return m_Timer; } set { m_Timer = value; } }
        public float CooldownTime { get { return m_CooldownTime; } set { m_CooldownTime = value; } }
        public bool IsActivated { get { return m_IsActivated; } set { m_IsActivated = value; } }
    }
}
