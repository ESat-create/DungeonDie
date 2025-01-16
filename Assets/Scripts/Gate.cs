using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class Gate : MonoBehaviour
{
    [SerializeField]
	public bool isOpen = false;

	private float openAmount = 1.7f;
	public void Open()
	{
		if (isOpen) { return; }

		isOpen = true;
		transform.position = new Vector3(transform.position.x, transform.position.y + openAmount, transform.position.z);
	}

	public void Close()
	{
		if (!isOpen) { return; }

		isOpen = false;
		transform.position = new Vector3(transform.position.x, transform.position.y - openAmount, transform.position.z);
	}
}
