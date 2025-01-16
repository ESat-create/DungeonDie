using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour, IInteractable
{
	public Action OnPressed;
	public Action OnReleased;

	public AudioClip open;
	public AudioClip close;

	public bool IsActive = false;

	private Quaternion rot;
	private float degree;

	public void Interact()
    {
		if (!IsActive)
			Press();
		else
			Release();
	}

	// Start is called before the first frame update
	public void Rotate()
    {
		transform.Rotate(90, 0, 0);
		
	}
	IEnumerator RotateDown()
	{
		for (int i = 0; i < 31; i++)
		{
			transform.Rotate(-3f, 0, 0);
			yield return null;
		}

	}
	IEnumerator RotateUp()
	{
		for (int i = 0; i < 31; i++)
		{
			transform.Rotate(3f, 0, 0);
			yield return null;
		}
	}
	void Press()
	{
		IsActive = true;
		//degree = Mathf.Repeat(degree + 45f, 360f);
		StartCoroutine(RotateDown());
		//Rotate();
		// Notify 'subscribers' (like this button's associated ButtonGroup) that the button has been pressed
		AudioManager.instance.PlaySound(open);
		OnPressed?.Invoke();
	}

	void Release()
	{
		IsActive = false;
		StartCoroutine(RotateUp());
		AudioManager.instance.PlaySound(close);
		//Rotate();
		// Notify 'subscribers' (like this button's associated ButtonGroup) that the button has been released
		OnReleased?.Invoke();
	}
}
