using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
	[SerializeField] private float attackCooldown;
	[SerializeField] private string direction;
	[SerializeField] private Transform firePoint;
	[SerializeField] private GameObject[] projectile;
	private float cooldownTimer;

	private void Attack()
	{
		cooldownTimer = 0;

		projectile[FindProjectile()].transform.position = firePoint.position;
		projectile[FindProjectile()].GetComponent<Projectile>().ActivateProjectile(direction);
	}

	private int FindProjectile()
	{
		for (int i = 0; i < projectile.Length; i++)
		{
			if (!projectile[i].activeInHierarchy)
				return i;
		}
		return 0;
	}

	private void Update()
	{
		cooldownTimer += Time.deltaTime;

		if (cooldownTimer >= attackCooldown)
		{
			Attack();
		}
	}
}
