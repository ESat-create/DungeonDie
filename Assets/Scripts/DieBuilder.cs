using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DieBuilder : MonoBehaviour
{
    [Header("Die side images")]
    public Material attackSideMat;
    public Material healSideMat;
    public Material shieldSideMat;
    public Material nullSideMat;

	public Sprite attackSide;
	public Sprite healSide;
	public Sprite shieldSide;
	public Sprite nullSide;
	public Sprite GetDieSideImg(char action)
    {
        switch (action)
        {
            case 'a':
                return attackSide;
            case 'h':
                return healSide;
            case 's':
                return shieldSide;
            default: 
                return nullSide;
        }
    }
	public Material GetDieSideMat(char action)
	{
		switch (action)
		{
			case 'a':
				return attackSideMat;
			case 'h':
				return healSideMat;
			case 's':
				return shieldSideMat;
			default:
				return nullSideMat;
		}
	}
}
