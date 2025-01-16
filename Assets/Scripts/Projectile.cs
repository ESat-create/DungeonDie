using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField] private float speed;
	[SerializeField] private float resetTime;
	private string direction = "x";
	private float lifetime;
	Rigidbody body;
	public void ActivateProjectile(string pass)
	{
		body = GetComponent<Rigidbody>();
		direction = pass;
		lifetime = 0;
		gameObject.SetActive(true);
	}
	private void Update()
	{
		float movementSpeed = speed * Time.deltaTime;
		switch (direction)
		{
			case "-x":
				transform.Translate(-movementSpeed, 0, 0);
				
				break;
			case "y":
				transform.Translate(0, movementSpeed, 0);
				break;
			case "-y":
				transform.Translate(0, -movementSpeed, 0);
				break;
			case "z":
				transform.Translate(0, 0, movementSpeed);
				break;
			case "-z":
				transform.Translate(0, 0, -movementSpeed);
				break;
			default:
				transform.Translate(movementSpeed, 0, 0);
				break;
		}



		lifetime += Time.deltaTime;
		if (lifetime > resetTime)
		{
			gameObject.SetActive(false);
		}
	}
	private void OnTriggerEnter(Collider other)
	{
		//base.OnTriggerEnter(other);
		gameObject.SetActive(false);
	}
}
