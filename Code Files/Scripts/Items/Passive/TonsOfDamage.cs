using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
	public class TonsOfDamage : AbstractPassive
	{
		public TonsOfDamage()
		{
			m_Description = "You'll deal TONS of Damage...Maybe.";
			m_Name = "Tons of Damage";
			m_SpriteName += "TonsOfDamage";
			m_Tooltip = "Increased Damage";
		}

		public override void ActivateItem(Player player)
		{
			player.IncreaseStat(StatType.DAMAGE, 2);

			Debug.Log("+2 Damage added to the Player");
		}
	}
}
