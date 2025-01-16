using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] public CustomDie startingDie;
    
    public string unitName;
    public int unitId;
	public Color unitColor = new Color(0, 0, 0, 1);

    public int diceId;
    public int damage;

    public int shields = 0;

    public int maxHP;
    public int currentHP;

    public bool diceUseable = false;
    public bool isDead = false;
	private void Start()
	{
		shields = 0;
	}
	public void TakeDamage(int damage)
    {
		if (shields > damage)
		{
			shields = shields - damage;
		}
		else
		{
			currentHP = currentHP - (damage - shields);
			shields = 0;
		}
		

        if (currentHP <= 0)
            isDead = true;
    }
	public void Heal(int health)
	{
		currentHP += health;

		if (currentHP > maxHP)
			currentHP = maxHP;
	}
	public void SetShields(int incoming)
	{
		shields = incoming;
	}
	public int GetShields()
	{
        return shields;
	}
}
