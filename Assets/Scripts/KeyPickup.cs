using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPickup : Pickup
{

	[SerializeField] int keyId = 100;
	protected override void OnPickup(PlayerController player)
	{
		Debug.Log("Key got " + keyId);
		player.AddKey(keyId);
	}
}
