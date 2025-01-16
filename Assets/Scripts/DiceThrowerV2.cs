using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class DiceThrowerV2 : MonoBehaviour
{
	public DieDatabase data;
	public DieRollerV2 diceToThrow;
	public int amountOfDice = 2;
	public float throwForce = 5f;
	public float rollForce = 10f;

	private List<DieRollerV2> _spawnedDice = new List<DieRollerV2>();
	private void Start()
	{
		data = GetComponent<DieDatabase>();
	}
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			//int[] ass = new int[] { 1, 2, 3 };
			//RollBattleDice(ass, 3);
			//RollDice();
		}
		//if (Input.GetKeyDown(KeyCode.Escape))
		//{
		//	RerollDice();
		//}
	}
	// public async void RollDice()
	// {
	//     if (diceToThrow == null) return;

	//     foreach(var die in _spawnedDice)
	//     {
	//Destroy(die.gameObject);
	//     }
	//     _spawnedDice = new List<DieRoller> ();
	//     for(int i = 0; i < amountOfDice; i++)
	//     {
	//DieRoller dice = Instantiate(diceToThrow, transform.position, transform.rotation);
	//dice.diceId = i;
	//_spawnedDice.Add(dice);
	//         dice.RollDie();
	//         await Task.Yield();
	//     }
	// }
	public async void RollBattleDice(int[] ids, int amount)
	{
		if (diceToThrow == null) return;

		foreach (var die in _spawnedDice)
		{
			Destroy(die.gameObject);
		}
		_spawnedDice = new List<DieRollerV2>();
		for (int i = 0; i < amount; i++)
		{
			DieRollerV2 dice = Instantiate(diceToThrow, transform.position, transform.rotation);
			_spawnedDice.Add(dice);
			dice.RollDice(throwForce, rollForce, ids[i]);
			await Task.Yield();
		}
	}
	public async void RollSelectBattleDice(int[] ids, int amount, bool friendly)
	{
		//if (diceToThrow == null) return;

		foreach (var die in _spawnedDice)
		{
			Destroy(die.gameObject);
		}
		_spawnedDice = new List<DieRollerV2>();


		DieRollerV2[] waitDice = new DieRollerV2[amount];
		for (int i = 0; i < amount; i++)
		{
			if (friendly)
				diceToThrow = data.GetPlayerDice(ids[i]);
			else
				diceToThrow = data.GetEnemyDice(ids[i]);
			Vector3 spawnLocation = new Vector3(transform.position.x + Random.Range(-0.5f, 0.5f), transform.position.y, transform.position.z + Random.Range(-0.5f, 0.5f));
			DieRollerV2 dice = Instantiate(diceToThrow, spawnLocation, transform.rotation);
			waitDice[i] = dice;
			_spawnedDice.Add(dice);
			//await Task.Delay(2000);
		}
		await Task.Delay(500);
		for (int i = 0; i < amount; i++)
		{
			waitDice[i].RollDice(throwForce, rollForce, ids[i]);
			//await Task.Delay(2000);
			await Task.Yield();
		}
		//for (int i = 0; i < amount; i++)
		//{
		//	diceToThrow = data.GetPlayerDice(ids[i]);
		//	DieRoller dice = Instantiate(diceToThrow, transform.position, transform.rotation);
		//	_spawnedDice.Add(dice);
		//	dice.RollDie();
		//	await Task.Delay(2000);
		//	await Task.Yield();
		//}
	}

	//public async void RollSingleDice(int id)
	//{
	//	if (diceToThrow == null) return;

	//	foreach (var die in _spawnedDice)
	//	{
	//		Destroy(die.gameObject);
	//	}
	//	_spawnedDice = new List<DieRoller>();
	//	DieRoller dice = Instantiate(diceToThrow, transform.position, transform.rotation);
	//	dice.diceId = id;
	//	_spawnedDice.Add(dice);
	//	dice.RollDie();
	//	await Task.Yield();
	//}

	//public async void RerollDice()
	//{
	//	if (diceToThrow == null) return;

	//	foreach (var die in _spawnedDice)
	//	{
	//           int i = die._diceIndex;
	//		die.RerollDie(throwForce, rollForce, i);
	//		await Task.Yield();
	//	}
	//}

}





//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using UnityEngine;
//using Random = UnityEngine.Random;

//public class DiceThrowerV2 : MonoBehaviour
//{
//	public DieDatabase data;
//    public DieRoller diceToThrow;
//    public int amountOfDice = 2;
//    public float throwForce = 5f;
//    public float rollForce = 10f;

//    private List<DieRoller> _spawnedDice = new List<DieRoller>();
//	private void Start()
//	{
//		data = GetComponent<DieDatabase>();
//	}
//	private void Update()
//	{
//		if (Input.GetKeyDown(KeyCode.Space))
//        {
//			//int[] ass = new int[] { 1, 2, 3 };
//		    //RollBattleDice(ass, 3);
//            //RollDice();
//        }
//		//if (Input.GetKeyDown(KeyCode.Escape))
//		//{
//		//	RerollDice();
//		//}
//	}
//   // public async void RollDice()
//   // {
//   //     if (diceToThrow == null) return;

//   //     foreach(var die in _spawnedDice)
//   //     {
//			//Destroy(die.gameObject);
//   //     }
//   //     _spawnedDice = new List<DieRoller> ();
//   //     for(int i = 0; i < amountOfDice; i++)
//   //     {
//			//DieRoller dice = Instantiate(diceToThrow, transform.position, transform.rotation);
//			//dice.diceId = i;
//			//_spawnedDice.Add(dice);
//   //         dice.RollDie();
//   //         await Task.Yield();
//   //     }
//   // }
//	public async void RollBattleDice(int[] ids, int amount)
//	{
//		if (diceToThrow == null) return;

//		foreach (var die in _spawnedDice)
//		{
//			Destroy(die.gameObject);
//		}
//		_spawnedDice = new List<DieRoller>();
//		for (int i = 0; i < amount; i++)
//		{
//			DieRoller dice = Instantiate(diceToThrow, transform.position, transform.rotation);
//			dice.diceId = ids[i];
//			_spawnedDice.Add(dice);
//			dice.RollDie();
//			await Task.Yield();
//		}
//	}
//	public async void RollSelectBattleDice(int[] ids, int amount)
//	{
//		if (diceToThrow == null) return;

//		foreach (var die in _spawnedDice)
//		{
//			Destroy(die.gameObject);
//		}
//		_spawnedDice = new List<DieRoller>();


//		DieRoller[] waitDice = new DieRoller[amount];
//		for (int i = 0; i < amount; i++)
//		{
//			diceToThrow = data.GetPlayerDice(ids[i]);
//			Vector3 spawnLocation = new Vector3(transform.position.x + Random.Range(-0.5f, 0.5f), transform.position.y, transform.position.z + Random.Range(-0.5f, 0.5f));
//			DieRoller dice = Instantiate(diceToThrow, spawnLocation, transform.rotation);
//			waitDice[i] = dice;
//			_spawnedDice.Add(dice);
//			//await Task.Delay(2000);
//		}
//		await Task.Delay(500);
//		for (int i = 0; i < amount; i++)
//		{
//			waitDice[i].RollDie();
//			//await Task.Delay(2000);
//			await Task.Yield();
//		}
//		//for (int i = 0; i < amount; i++)
//		//{
//		//	diceToThrow = data.GetPlayerDice(ids[i]);
//		//	DieRoller dice = Instantiate(diceToThrow, transform.position, transform.rotation);
//		//	_spawnedDice.Add(dice);
//		//	dice.RollDie();
//		//	await Task.Delay(2000);
//		//	await Task.Yield();
//		//}
//	}

//	public async void RollSingleDice(int id)
//	{
//		if (diceToThrow == null) return;

//		foreach (var die in _spawnedDice)
//		{
//			Destroy(die.gameObject);
//		}
//		_spawnedDice = new List<DieRoller>();
//		DieRoller dice = Instantiate(diceToThrow, transform.position, transform.rotation);
//		dice.diceId = id;
//		_spawnedDice.Add(dice);
//		dice.RollDie();
//		await Task.Yield();
//	}

//	//public async void RerollDice()
//	//{
//	//	if (diceToThrow == null) return;

//	//	foreach (var die in _spawnedDice)
//	//	{
//	//           int i = die._diceIndex;
//	//		die.RerollDie(throwForce, rollForce, i);
//	//		await Task.Yield();
//	//	}
//	//}

//}
