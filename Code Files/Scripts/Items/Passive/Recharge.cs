using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
	public class Recharge : AbstractPassive 
	{

		public Recharge()
		{
			m_Description = "I'm charged up!";
			m_Name = "Recharge";
			m_SpriteName += "Recharge";
			m_Tooltip = "Cooldown Reduction Increased";
		}

		public override void ActivateItem(Player player)
		{
			player.IncreaseStat(StatType.CDR, 2);

			Debug.Log("+2 Cooldown Reduction added to the Player");
		}
	}
}
