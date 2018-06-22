using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
	public class BulletProofVest : AbstractPassive
	{
		public BulletProofVest()
		{
			m_Description = "There isn't a whole lot of enemies firing bullets though.";
			m_Name = "BULLET PROOF VEST";
			m_SpriteName += "BulletProofVest";
			m_Tooltip = "Increased Defenses";
		}

		public override void ActivateItem(Player player)
		{
			player.IncreaseStat(StatType.DEFENSE, 1);

			Debug.Log("+2 Defenses added to the Player");
		}
	}
}
