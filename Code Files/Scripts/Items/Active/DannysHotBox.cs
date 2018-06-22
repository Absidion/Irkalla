using UnityEngine;


//Author: Josue 
//Last edited: 11/13/2017

namespace TheNegative.Items
{
    public class DannysHotBox : AbstractActiveRoomBased
    {       
        public DannysHotBox()
        {
            m_RoomCount = 10;
            m_RoomCooldown = 0;
            m_SpriteName += "DannysHotBox";
            m_Tooltip = "Burn all enemies";
        }

        public override void UpdateItem()
        {
            ResetCooldown();
            //TODO: Remove this once Shadow Demons are added to the room count.
            AI.AI[] ShadowDemons = GameObject.FindObjectsOfType<AI.ShadowDemonAI>();

            for (int i = 0; i < ShadowDemons.Length; i++)
            {
                AI.AI enemy = ShadowDemons[i];
                int damage = Mathf.RoundToInt(1);
                Status[] statuses = { Status.Burn };
                enemy.TakeDamage(m_Player.PlayerNumber, damage, statuses, 1);
            }

            //Do damage to every enemy in the room by their max HP and apply burn
            for (int i = 0; i < m_Player.MyIslandRoom.EnemiesInRoomCount; i++)
            {
                AI.AI enemy = m_Player.MyIslandRoom.EnemiesInRoom[i];

                int damage = Mathf.RoundToInt(enemy.Health.MaxHp / 2);
                if (m_Player.MyIslandRoom.TypeOfRoom == RoomType.Boss)
                    damage = 30;

                Status[] statuses = { Status.Burn };
                enemy.TakeDamage(m_Player.PlayerNumber, damage, statuses, 1);
            }

        }
    }
}
