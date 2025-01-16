using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieDatabase : MonoBehaviour
{
    [SerializeField] DieRollerV2[] PlayerDice;
    [SerializeField] DieRollerV2[] EnemyDice;
    public DieRollerV2 GetPlayerDice(int id)
    {
        foreach (var die in PlayerDice)
        {
            if (die.GetComponent<CustomDie>().GetId() == id)
                return die;
        }
        return null;
    }
	public DieRollerV2 GetEnemyDice(int id)
	{
		foreach (var die in EnemyDice)
		{
			if (die.GetComponent<CustomDie>().GetId() == id)
				return die;
		}
		return null;
	}
}
