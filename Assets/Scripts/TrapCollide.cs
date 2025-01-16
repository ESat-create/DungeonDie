using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapCollide : MonoBehaviour
{
	public AudioClip hit;
	public int damage;
	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			other.GetComponent<PlayerController>().TakeDamage(damage);
			AudioManager.instance.PlaySound(hit);
		}
	}
}
