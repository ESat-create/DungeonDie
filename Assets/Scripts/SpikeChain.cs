using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeChain : MonoBehaviour
{
    public bool isActive = true;
    public GameObject[] spikes;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Chain());
    }
    IEnumerator Chain()
    {
        if (!isActive)
		    yield return null;
        for (int i = 0; i < spikes.Length; i++)
        {
			spikes[i].GetComponent<PressureSpike>().ShootAsync();
			yield return new WaitForSeconds(1f);
            if (i == spikes.Length - 1)
                i = -1;
		}
	}
}
