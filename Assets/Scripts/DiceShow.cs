using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class DiceShow : MonoBehaviour
{
	public CustomDie customDie;
	public TMP_Text[] diceStrength;
	public Image[] diceSide;
	public Sprite[] diceSides;
	public Sprite shield, sword, heart;
	public void Start()
	{
		string str;
		string action;
		string actionValue;
		for (int i = 0; i < 6; i++)
		{
			str = customDie.GetSide(i+1);
			action = str.Substring(0, 1);
			actionValue = str.Substring(1, 1);
			SetImage(action, i);
			SetText(actionValue, i);
		}
		
		//Debug.Log("Targeter" + targeter % 100 + "with result of " + resultDice[altSelect[altSelect[targeter % 100 - 1]]]);
		
	}

	public void SetImage(string action, int i)
	{
        switch(action)
		{
			case "s":
				diceSide[i].sprite = shield;
				break;
			case "h":
				diceSide[i].sprite = heart;
				break;
			default:
				diceSide[i].sprite = sword;
				break;
		}
	}
	public void SetText(string strength, int i)
	{
		diceStrength[i].text = strength;
	}
}
