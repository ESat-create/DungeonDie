using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : Pickup
{
	[SerializeField] int healValue = 4;
	protected override void OnPickup(PlayerController player)
	{
		player.HealResolve(healValue);
	}
}
