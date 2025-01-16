using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
	protected abstract void OnPickup(PlayerController player);
	public AudioClip secret;

	private new CapsuleCollider collider;
	private void Awake()
	{
		collider = GetComponent<CapsuleCollider>();
	}
	private void OnTriggerEnter(Collider other)
	{
		PlayerController player = other.GetComponent<PlayerController>();
		if (player == null)
		{
			return;
		}
		AudioManager.instance.PlaySound(secret);
		OnPickup(player);

		Disable();
	}
	protected virtual void Disable()
	{
		gameObject.SetActive(false);
	}
}
