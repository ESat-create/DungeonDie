using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DiceThrower : MonoBehaviour
{
    public Dice diceToThrow;
    public int amountOfDice = 5;
    public float throwForce = 5f;
    public float rollForce = 10f;

    private List<Dice> _spawnedDice = new List<Dice>();

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
        {
			int[] ass = new int[] { 1, 2, 3 };
		    //RollBattleDice(ass, 3);
            RollDice();
        }
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			RerollDice();
		}
	}
    public async void RollDice()
    {
        if (diceToThrow == null) return;

        foreach(var die in _spawnedDice)
        {
			Destroy(die.gameObject);
        }
        _spawnedDice = new List<Dice> ();
        for(int i = 0; i < amountOfDice; i++)
        {
            Dice dice = Instantiate(diceToThrow, transform.position, transform.rotation);
            _spawnedDice.Add(dice);
            dice.RollDice(throwForce, rollForce, i);
            await Task.Yield();
        }
    }
	public async void RollBattleDice(int[] id, int amount)
	{
		if (diceToThrow == null) return;
		foreach (var die in _spawnedDice)
		{
			Destroy(die.gameObject);
		}
		_spawnedDice = new List<Dice>();
		for (int i = 0; i < amount; i++)
		{
			Dice dice = Instantiate(diceToThrow, transform.position, transform.rotation);
			_spawnedDice.Add(dice);
			dice.RollDice(throwForce, rollForce, id[i]);
			await Task.Yield();
		}
	}

	public async void RollSingleDice(int id)
	{
		if (diceToThrow == null) return;

		foreach (var die in _spawnedDice)
		{
			Destroy(die.gameObject);
		}
		_spawnedDice = new List<Dice>();
		Dice dice = Instantiate(diceToThrow, transform.position, transform.rotation);
		_spawnedDice.Add(dice);
		dice.RollDice(throwForce, rollForce, id);
		await Task.Yield();
	}
	
	public async void RerollDice()
	{
		if (diceToThrow == null) return;

		foreach (var die in _spawnedDice)
		{
            int i = die._diceIndex;
			die.RerollDice(throwForce, rollForce, i);
			await Task.Yield();
		}
	}
	
}
