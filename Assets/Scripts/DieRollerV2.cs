using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class DieRollerV2 : MonoBehaviour
{
	public Transform[] diceFaces;
	public Rigidbody rb;

	private int _diceId;

	private bool _hasStopper;
	private bool _delayFinished;

	public static UnityAction<int, int> OnDiceResult;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}
	private void Update()
	{
		if (!_delayFinished) return;

		if (!_hasStopper && rb.velocity.sqrMagnitude == 0f)
		{
			_hasStopper = true;
			Debug.Log($"Dice result {GetNumberOnTopFace()}");
		}
	}
	[ContextMenu("Get Top Face")]
	private int GetNumberOnTopFace()
	{
		if (diceFaces == null) return -1;

		var topFace = 0;
		var lastYPos = diceFaces[0].position.y;

		for (int i = 0; i < diceFaces.Length; i++)
		{
			if (diceFaces[i].position.y > lastYPos)
			{
				lastYPos = diceFaces[i].position.y;
				topFace = i;
			}
		}
		//Debug.Log($"Dice result {topFace + 1}");

		OnDiceResult?.Invoke(_diceId, topFace + 1);

		return topFace + 1;
	}

	public void RollDice(float throF, float roll, int i)
	{
		_diceId = i;
		var randVar = Random.Range(-1f, 1f);
		//rb.AddForce(transform.forward * (throF + randVar), ForceMode.Impulse);

		var randX = Random.Range(0f, 1f);
		var randY = Random.Range(0f, 1f);
		var randZ = Random.Range(0f, 1f);


		var targetPosition = new Vector3(Random.Range(-1f, 1f), 5f, Random.Range(-1f, 1f));
		var direction = targetPosition - transform.up;
		rb.AddForce(direction * 1.4f, ForceMode.Impulse);
		rb.AddTorque(new Vector3(randX, randY, randZ) * (roll + randVar), ForceMode.Impulse);
		//rb.AddTorque(Random.insideUnitSphere * 30f, ForceMode.Impulse);



		DelayResult();
	}

	private async void DelayResult()
	{
		await Task.Delay(1000);
		_delayFinished = true;
	}
}
