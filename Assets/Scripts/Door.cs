using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    private GameObject doorL;
    private GameObject doorR;
    private LockedObject thing;
	[SerializeField] private Animator animator;
	[SerializeField] private BoxCollider boxCollider;
	[SerializeField] UnityEvent onDoorOpen;
	[SerializeField] LockedObject lockedObject;
	void OnEnable()
	{
		lockedObject.OnOpen += HandleOpen;
		lockedObject.OnUnlock += HandleUnlock;
	}
	void OnDisable()
	{
		lockedObject.OnOpen -= HandleOpen;
		lockedObject.OnUnlock -= HandleUnlock;
	}
	void HandleOpen()
	{
		boxCollider.enabled = false;
		animator.SetTrigger("Open");
		Debug.Log("Banana");
	}
	void HandleUnlock()
	{
		Debug.Log("Ananas");
	}
}
