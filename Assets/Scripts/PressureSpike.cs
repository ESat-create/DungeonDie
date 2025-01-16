using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PressureSpike : MonoBehaviour
{
    public bool isTrapped = true;
    public Animator spikeAnimator;
    public Collider triggerBox;
    public Collider playerCollision;
    public async Task ShootAsync()
    {
		spikeAnimator.SetTrigger("Shoot");
		await Task.Delay(600);
		spikeAnimator.SetTrigger("Retract");
	}
	void OnTriggerEnter(Collider collision)
	{
		if (collision.tag == "Player" && isTrapped)
		{
			Debug.Log("Entering");
			ShootAsync();
		}
	}
}
