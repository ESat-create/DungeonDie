using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    public Text nameText;
    public Text shieldText;
    public Image frameOutline;
    public Image diceIcon;
    public Text diceText;
    public Slider hpSlider;

    public void SetHUD(Unit unit)
    {
        nameText.text = unit.unitName;
		shieldText.text = ""+unit.GetShields();
		hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;
        frameOutline.color = unit.unitColor;
    }

    public void SetHP(int hp)
    {
        hpSlider.value = hp;
    }
	public void SetShields(string shields)
	{
		shieldText.text = shields;
	}
	public void SetResult(string res)
	{
		diceText.text = res;
	}
	public void SetDiceIcon(char action, DieBuilder builder)
	{
        diceIcon.GetComponent<Image>().sprite = builder.GetDieSideImg(action);
	}
    public void UpdateBars(int hp, string shields)
    {
        SetHP(hp);
        SetShields(shields);

	}
}
