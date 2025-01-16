using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CustomDie : MonoBehaviour
{
	[SerializeField] int customId;


	[Header("Current die")]
	[SerializeField] string top;
	[SerializeField] string left;
	[SerializeField] string middle;
	[SerializeField] string right;
	[SerializeField] string rightmost;
	[SerializeField] string bottom;

	[Header("Original die")]
	[SerializeField] string topOrig;
	[SerializeField] string leftOrig;
	[SerializeField] string middleOrig;
	[SerializeField] string rightOrig;
	[SerializeField] string rightmostOrig;
	[SerializeField] string bottomOrig;


	[Header("Number sides")]
	[SerializeField] TextMeshPro topT;
	[SerializeField] TextMeshPro leftT;
	[SerializeField] TextMeshPro middleT;
	[SerializeField] TextMeshPro righT;
	[SerializeField] TextMeshPro rightmostT;
	[SerializeField] TextMeshPro bottomT;

	string[] sides = new string[6];
	private void Start()
	{
		SetSide(1, topT);
		SetSide(2, leftT);
		SetSide(3, middleT);
		SetSide(4, righT);
		SetSide(5, rightmostT);
		SetSide(6, bottomT);
	}
	public string GetSide(int sideValue)
    {
        switch(sideValue)
        {
			case 1:
                return top;
            case 2:
                return left;
            case 3:
                return middle;
            case 4:
                return right;
            case 5:
                return rightmost;
			case 6:
				return bottom;
            default:
                return "a8";
        }
    }
	public void AddSide(int sideValue, int value)
	{

	}
	public void SetSide(int sideValue, TextMeshPro text) 
	{
		string side = GetSide(sideValue);
		string action = side.Substring(0, 1);
		int actionValue = int.Parse(side.Substring(1, 1));
		text.SetText(""+actionValue);
	}
	public int GetId()
	{
		return customId;
	}
}
