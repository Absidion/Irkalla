using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
	public class HedgehogSpeed : AbstractPassive
	{
		public HedgehogSpeed()
		{
			m_Description = "I'm almost as fast as your average hedgehog.";
			m_Name = "Hedgehog Speed";
			m_SpriteName += "HedgehogSpeed";
			m_Tooltip = "Increased Movement Speed";
		}

		public override void ActivateItem(Player player)
		{
			player.IncreaseStat(StatType.MOVEMENT_SPEED, 2);

			Debug.Log("+2 Movement added to the Player");
		}
	}
}
