using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
	public class HealthKick : AbstractPassive
	{
		public HealthKick()
		{
			m_Description = "A vegan diet is the best diet.";
			m_Name = "HEALTH KICK";
			m_SpriteName += "HealthKick";
			m_Tooltip = "Max Health increased!";
		}

		public override void ActivateItem(Player player)
		{
            player.Health.ChangeMaxHP(25);

			Debug.Log("25 HP Added to the Max Health");
		}
	}
}
