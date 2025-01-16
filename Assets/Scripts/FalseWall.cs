using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FalseWall : MonoBehaviour, IInteractable
{

	public bool IsFound = false;
	public bool InPosition = false;

	public AudioClip secret;

	private Quaternion rot;
	private float degree;

	public void Interact()
	{
		if (!IsFound)
			Press();
	}

	// Start is called before the first frame update
	public void Move()
	{

		transform.Translate(0, -2f, 0);

	}
	void Press()
	{
		IsFound = true;
		//degree = Mathf.Repeat(degree + 45f, 360f);
		AudioManager.instance.PlaySound(secret);
		StartCoroutine(Schmove());
		// Notify 'subscribers' (like this button's associated ButtonGroup) that the button has been pressed

	}
	IEnumerator Schmove()
	{
		for(int i = 0; i < 39; i++)
		{
			transform.Translate(0, -0.05f, 0);
			yield return null;
		}
	}
}
