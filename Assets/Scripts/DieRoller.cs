using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class DieRoller : MonoBehaviour
{
    public static UnityAction<int, int> OnDieRolled;
	public int diceId;

    float _rollForce = 10f;
    float _torqueAmount = 2f;
    [SerializeField] LayerMask _layerMask;
    //[SerializeField] GameObject _clickToRollText;

    Rigidbody _rigidbody;
    Transform _transform;
    bool _rolling;

	private void Awake()
	{
		diceId = GetComponent<CustomDie>().GetId();
		_transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
        //_clickToRollText.SetActive(true);
	}
	private void Update()
	{
	}
	//private void Start()
	//{
	//       _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
	//}

	//private void OnCollisionEnter(Collision collision)
	//{
	//	if (!_rolling) return;
	//	var volume = Mathf.Clamp(collision.relativeVelocity.magnitude / 5f, 0f, 1f);
	//}

	[ContextMenu("Roll Die")]
	public void RollDie()
	{
		StopAllCoroutines();
		_rigidbody.constraints = RigidbodyConstraints.None;
		_rolling = true;
		var targetPosition = new Vector3(Random.Range(-1f, 1f), 5f, Random.Range(-1f, 1f));
		var direction = targetPosition - _transform.up;
		_rigidbody.AddForce(direction * 1f, ForceMode.Impulse);
		_rigidbody.AddTorque(Random.insideUnitSphere * 20f, ForceMode.Impulse);
		StartCoroutine(WaitForDieToStop());
	}
	IEnumerator WaitForDieToStop()
	{
		var timeOut = Time.time + 10f;
		while(!_rigidbody.IsSleeping() && Time.time < timeOut)
		{
			yield return null;
		}

		yield return null;

		var dieValue = GetDieValue();
		Debug.Log($"You rolled a {dieValue} for {diceId}", gameObject);
		OnDieRolled?.Invoke(diceId, dieValue);
	}
	int GetDieValue()
	{
		_rolling = false;
		Vector3[] directions =
		{
			-_transform.forward, // 1
			-_transform.up,     // 2
			_transform.right,   // 3
			-_transform.right,  // 4
			_transform.up,      // 5
			_transform.forward  // 6
		};
		for (var i = 0; i < directions.Length; i++)
		{
			if (!Physics.Raycast(_transform.position, directions[i], 1f, _layerMask)) continue;
			return i + 1;
		}
		Debug.Log("Error getting die value.");
		return 0;
	}
}
