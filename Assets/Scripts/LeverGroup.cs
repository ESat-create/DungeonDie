using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LeverGroup : MonoBehaviour
{
	[SerializeField] Lever[] levers;
	[SerializeField] UnityEvent onAllLeversFlipped;
	[SerializeField] UnityEvent onAnyLeversUnflipped;

	void OnEnable()
	{
		// Subscribe to every associated button's OnPressed and OnReleased Action (so that this script is notified when those actions take place)
		foreach (Lever lever in levers)
		{
			lever.OnPressed += HandleButtonPressed;
			lever.OnReleased += HandleButtonReleased;
		}
	}
	void OnDisable()
	{
		// Unsubscribe from every associated button's Actions (as a safety measure)
		foreach (Lever lever in levers)
		{
			lever.OnPressed -= HandleButtonPressed;
			lever.OnReleased -= HandleButtonReleased;
		}
	}
	void HandleButtonPressed()
	{
		if (AllButtonsPressed())
			onAllLeversFlipped.Invoke();
	}
	void HandleButtonReleased() => onAnyLeversUnflipped.Invoke();
	bool AllButtonsPressed()
	{
		foreach (Lever lever in levers)
		{
			if (lever.IsActive == false)
				return false;
		}

		return true;
	}
}
