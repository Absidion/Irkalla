using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheNegative.Items
{
	public class LuckyCharm : AbstractItem 
	{

		public LuckyCharm()
		{
			m_Description = "I've gotten me lucky charms!";
			m_Name = "Lucky Charm";
			m_SpriteName += "LuckyCharm";
			m_Tooltip = "Increased Luck";
		}

		public override void ActivateItem(Player player)
		{
				player.IncreaseStat(StatType.LUCK, 2);

			Debug.Log("+2 Luck added to the Player");
		}
	}
}
