using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedObject : MonoBehaviour, IInteractable
{
	public Action OnUnlock;
	public Action OnOpen;
	[SerializeField] private int keyId;
	[SerializeField] public PlayerController controller;

	public AudioClip locked;
	public AudioClip unlocked;
	public AudioClip open;

	public bool isLocked = true;
	public void Interact()
	{
		if (isLocked == true && controller.CheckForKey(keyId))
		{
			isLocked = false;
			OnUnlock?.Invoke();
			AudioManager.instance.PlaySound(unlocked);
		}
		else if (isLocked)
		{
			AudioManager.instance.PlaySound(locked);
		}
		else if (isLocked == false)
		{
			AudioManager.instance.PlaySound(open);
			OnOpen?.Invoke();
		}
	}
}
