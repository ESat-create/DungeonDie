using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public enum ManyBattleState { START, PLAYERTURN, PLAYERSELECT, HEALSELECT, ATTACKSELECT, ENEMYUSE, ENEMYROLL, WON, LOST }

public class ManyBattleSystem : MonoBehaviour
{
	public AudioClip hit;
	public AudioClip shield;
	public AudioClip heal;
	public AudioClip die;
	


	public GameObject _Canvas;
	public CombatBuilder _CombatBuilder;
	public DieBuilder builder;
	public Button actionButton;
    public DiceThrowerV2 thrower;

	public int playerNum = 0;
	public int enemyNum = 0;

	public GameObject[] playerPrefab;
	public GameObject[] playerFrames;
	public Transform[] playerBattleStation;
	public BattleHUD[] playerHUD;

	public Unit[] playerUnits;

	public GameObject[] enemyPrefab;
	public GameObject[] enemyFrames;
    public Transform[] enemyBattleStation;
	public BattleHUD[] enemyHUD;
	public Image[] targetIndicator;
	public Unit[] enemyUnits;

	public int[] alt;
	public int deadEn = 0;
	public int aliveEnemies = 0;

	public int playerAliveCount;
	public int playerDeadCount = 0;

	public int[] playerDiceIds;
	public int[] enemyDiceIds;

	private int targeter;
    private int targeted;
	private int targeterModif;

	

	public int[] altSelect;
	public int[] altSelectFrame;
	public int[] altSelectEnemyFrame;
	public int[] altSelectEn;
    public TMP_Text dialogueText;

    public int[] playerResultDice;
	public int[] enemyResultDice;

	public string[] enemyActions;
	public int[] enemyTargets;
	public int test;
	public string action;
	public int actionValue;
    public ManyBattleState state;
    // Start is called before the first frame update
    void Start()
    {
		playerUnits = new Unit[playerPrefab.Length];
		enemyUnits = new Unit[playerPrefab.Length];
		playerDiceIds = new int[] { 101, 102, 103 };
		enemyDiceIds = new int[] { 201, 202, 203 };
		altSelect = new int[] { 0, 1, 2 };
		altSelectFrame = altSelect;
		altSelectEnemyFrame = altSelect;
		altSelectEn = new int[] { 0, 1, 2 };
		playerResultDice = new int[3];
		enemyResultDice = new int[3];
		state = ManyBattleState.START;
        StartCoroutine(SetupBattle());

    }
    IEnumerator SetupBattle()
    {
        GameObject playerGO;
        int i = 0;
        foreach (GameObject GO in playerPrefab)
        {
			playerGO = Instantiate(GO, playerBattleStation[i]);
			playerUnits[i] = playerGO.GetComponent<Unit>();
			i++;
		}

		GameObject enemyGO;
		i = 0;
		foreach (GameObject GO in enemyPrefab)
		{
			enemyGO = Instantiate(GO, enemyBattleStation[i]);
			enemyUnits[i] = enemyGO.GetComponent<Unit>();
			i++;
		}
		aliveEnemies = i;
        //dialogueText.text = "A wild " + enemyUnits.unitName + " approaches...";
        i = 0;
		foreach (BattleHUD hud in playerHUD)
		{
            hud.SetHUD(playerUnits[i]);
			i++;
		}
        i = 0;
		foreach (BattleHUD hud in enemyHUD)
		{
			hud.SetHUD(enemyUnits[i]);
			i++;
		}
		foreach (GameObject val in enemyFrames)
		{
			val.GetComponent<Button>().interactable = false;
		}
		foreach (GameObject val in playerFrames)
		{
			val.GetComponent<Button>().interactable = false;
		}
		playerAliveCount = playerUnits.Length;

		yield return new WaitForSeconds(1f);

        state = ManyBattleState.ENEMYROLL;
		
		StartCoroutine(EnemyRoll());
        //PlayerTurn();
    }
    void PlayerTurn()
    {
        dialogueText.text = "Your turn";
    }
	public void OnAttackButton()
    {
        if (state != ManyBattleState.PLAYERTURN)
            return;
		actionButton.interactable = false;
        StartCoroutine(PlayerRollDice());
    }    
    IEnumerator PlayerRollDice()
    {
        yield return RollDice();
		yield return new WaitForSeconds(2f);
		for(int i = 0; i < playerUnits.Length; i++)
		{
			string result;
			//string str = playerUnits[i].startingDie.GetSide(i+2);
			string str = playerUnits[i].startingDie.GetSide(playerResultDice[altSelect[playerUnits[i].unitId % 100 - 1]]);
			string action = str.Substring(0, 1);
			string actionValue = str.Substring(1, 1);
			if (!playerUnits[i].isDead)
			{
				playerHUD[i].SetDiceIcon(action[0], builder);
				//Debug.Log($"Bnana {action[0]} for {i}");
			}
			//switch (action)
			//{
			//	case "s":
			//		result = "Shield for " + actionValue;
			//		break;
			//	case "h":
			//		result = "Heal for " + actionValue;
			//		break;
			//	default:
			//		result = "Attack for " + actionValue;
			//		break;
			//}
			playerHUD[i].diceText.text = actionValue;
		}
        foreach (Unit val in playerUnits)
        {
            val.diceUseable = true;
        }
		foreach (GameObject val in playerFrames)
		{
			val.GetComponent<Button>().interactable = true;
		}

		state = ManyBattleState.PLAYERSELECT;
    }
    void EndBattle()
    {
		StopAllCoroutines();
        if (state == ManyBattleState.WON)
        {
			Debug.Log("You won!");
			_CombatBuilder.player.GetComponent<PlayerController>().HealResolve(5);
        }
        else if (state == ManyBattleState.LOST)
        {
			Debug.Log("You lost.");
			_CombatBuilder.player.GetComponent<PlayerController>().TakeDamage(5);
		}
		_CombatBuilder.SetDungeon();
		//_Canvas.SetActive(false);
    }
	
    IEnumerator EnemyTurn()
    {
		foreach (GameObject val in playerFrames)
		{
			val.GetComponent<Button>().interactable = false;
		}
		yield return new WaitForSeconds(1f);
		int i = 0;
		foreach (GameObject val in enemyFrames)
		{
			string action = enemyActions[i].Substring(0, 1);
			string actionValue = enemyActions[i].Substring(1, 1);
			if(action == "a")
			{
				test = enemyTargets[i] - 1;
				if (test != -1)
				{
					//Debug.Log("max target" + playerUnits.Length);
					val.GetComponent<Button>().interactable = true;
					yield return new WaitForSeconds(1f);
					dialogueText.text = enemyUnits[i].unitName + " attacks the " + playerUnits[test].unitName + "!";

					AudioManager.instance.PlaySound(hit);

					playerUnits[test].TakeDamage(int.Parse(actionValue));
					//playerHUD[altSelectFrame[target - 1]].SetShields("" + playerUnits[altSelect[target - 1]].GetShields());
					//playerHUD[altSelectFrame[target - 1]].SetHP(playerUnits[altSelect[target - 1]].currentHP);
					playerHUD[test].UpdateBars(playerUnits[test].currentHP, "" + playerUnits[test].GetShields());

					if (playerUnits[test].isDead)
					{
						for (int j = 0; j < enemyTargets.Length; j++)
						{
							if (enemyTargets[j] - 1 == test)
								enemyTargets[j] = -1;
						}
						//Debug.Log("Removing " + playerUnits[target - 1].name);
						RemovePlayer(playerUnits[test].unitId);
					}
					if (playerAliveCount == playerDeadCount)
					{
						state = ManyBattleState.LOST;
						EndBattle();
						break;
					}

					yield return new WaitForSeconds(1f);
					val.GetComponent<Button>().interactable = false;
					yield return new WaitForSeconds(1f);
					i++;
				}
				
			}
			
		}
		
		bool isDead = false;
		//yield return new WaitForSeconds(1f);

        if (isDead)
        {
            state = ManyBattleState.LOST;
            EndBattle();
        }
        else
        {
            state = ManyBattleState.ENEMYROLL;
			StartCoroutine(EnemyRoll());
		}
	}
	IEnumerator EnemyRoll()
	{
		yield return RollDice();
		yield return new WaitForSeconds(2f);
		enemyTargets = new int[enemyUnits.Length];
		enemyActions = new string[enemyUnits.Length];
		for (int i = 0; i < enemyUnits.Length; i++)
		{
			string result;
			//string str = playerUnits[i].startingDie.GetSide(i+2);
			string str = enemyUnits[i].startingDie.GetSide(enemyResultDice[i]);
			string action = str.Substring(0, 1);
			string actionValue = str.Substring(1, 1);
			enemyActions[i] = str;
			enemyTargets[i] = Random.Range(1, playerUnits.Length + 1);
			targetIndicator[altSelectEnemyFrame[i]].color = playerUnits[enemyTargets[i] - 1].unitColor;
			//if (!enemyUnits[i].isDead){
				enemyHUD[altSelectEnemyFrame[i]].SetDiceIcon(action[0], builder);
				//Debug.Log($"Boo {action[0]} for {i}");
			//}
			//switch (action)
			//{
			//	case "s":
			//		result = "Shield for " + actionValue;
			//		break;
			//	case "h":
			//		result = "Heal for " + actionValue;
			//		break;
			//	default:
			//		result = "Attack for " + actionValue;
			//		break;
			//}
			enemyHUD[i].diceText.text = actionValue;
		}
		foreach (Unit val in enemyUnits)
		{
			val.diceUseable = true;
		}

		state = ManyBattleState.PLAYERTURN;
		actionButton.interactable = true;
		PlayerTurn();
		yield return null;
	}
	private void GetResult(int diceIndex, int diceResult)
	{
		//Debug.Log("pls "+diceIndex);
		if(state == ManyBattleState.PLAYERTURN)
			//if(playerDiceIds.Contains(diceIndex))
				playerResultDice[altSelect[diceIndex % 100 - 1]] = diceResult;
		if (state == ManyBattleState.ENEMYROLL)
			//if (enemyDiceIds.Contains(diceIndex))
				enemyResultDice[altSelectEn[diceIndex % 100 - 1]] = diceResult;
		
	}
	// What the name implies
	IEnumerator RollDice()
    {
		if (state == ManyBattleState.PLAYERTURN)
			thrower.RollSelectBattleDice(playerDiceIds, playerDiceIds.Length, true);
		if(state == ManyBattleState.ENEMYROLL)
			thrower.RollSelectBattleDice(enemyDiceIds, enemyDiceIds.Length, false);

		//thrower.RollBattleDice(playerDiceIds, playerDiceIds.Length);
		//thrower.RollSingleDice(enemyDiceIds[0]);
		yield return new WaitForSeconds(2f);
		//Dice.OnDiceResult += GetResult;
		yield return null;
    }
	private void OnEnable()
	{
		DieRollerV2.OnDiceResult += GetResult;
	}
	private void OnDisable()
	{
		DieRollerV2.OnDiceResult -= GetResult;
	}
	//private void OnEnable()
	//{
	//	DieRoller.OnDieRolled += GetResult;
	//}
	//private void OnDisable()
	//{
	//	DieRoller.OnDieRolled -= GetResult;
	//}
	// Calls action handlers when trying to use dice
	public void Select(int buttonName)
	{
		//Debug.Log("Selected " + buttonName);
		switch (state)
		{
			case ManyBattleState.PLAYERSELECT:
				targeter = buttonName;
				if (playerUnits[altSelect[targeter % 100 - 1]].diceUseable)
				{
					//string str = playerUnits[altSelect[targeter % 100 - 1]].startingDie.GetSide(resultDice[altSelect[targeter % 100 - 1]]);
					string str = playerUnits[altSelect[targeter % 100 - 1]].startingDie.GetSide(playerResultDice[targeter % 100 - 1]);
					//Debug.Log("Targeter" + targeter % 100 + "with result of " + resultDice[altSelect[altSelect[targeter % 100 - 1]]]);
					action = str.Substring(0, 1);
					actionValue = int.Parse(str.Substring(1, 1));
					//Debug.Log("Letter " + action);
					//Debug.Log("Number " + actionValue);
					switch (action)
					{
						case "a":
							state = ManyBattleState.ATTACKSELECT;
							foreach (GameObject val in playerFrames)
							{
								val.GetComponent<Button>().interactable = false;
							}
							foreach (GameObject val in enemyFrames)
							{
								val.GetComponent<Button>().interactable = true;
							}
							break;
						case "s":
							foreach (GameObject val in playerFrames)
							{
								val.GetComponent<Button>().interactable = true;
							}
							state = ManyBattleState.HEALSELECT;
							dialogueText.text = "Choose who to shield";
							break;
						default:
							foreach (GameObject val in playerFrames)
							{
								val.GetComponent<Button>().interactable = true;
							}
							state = ManyBattleState.HEALSELECT;
							dialogueText.text = "Choose who to heal";
							break;
					}
				}
				break;
			case ManyBattleState.HEALSELECT:
				targeted = buttonName;
				if (action == "s")
				{
					ShieldHandler();
				}
				else if (action == "h")
				{
					HealingHandler();
				}
				break;
			case ManyBattleState.ATTACKSELECT:
				targeted = buttonName;
				dialogueText.text = "Choose who to attack";
				StartCoroutine(AttackHandler());
				break;
			default:
				targeter = buttonName;
				SelectionHandler();
				break;

		}
	}
	// When the player selects a player character to use, what do
	private void SelectionHandler()
    {
		if (playerUnits[altSelect[targeter % 100 - 1]].diceUseable)
		{
			string str = playerUnits[altSelect[targeter % 100 - 1]].startingDie.GetSide(playerResultDice[targeter % 100 - 1]);
			//Debug.Log("Targeter" + targeter % 100 + "with result of " + resultDice[altSelect[altSelect[targeter % 100 - 1]]]);
			action = str.Substring(0, 1);
			actionValue = int.Parse(str.Substring(1, 1));
			switch (action)
			{
				case "a":
					state = ManyBattleState.ATTACKSELECT;
					foreach (GameObject val in enemyFrames)
					{
						val.GetComponent<Button>().interactable = true;
					}
					break;
				case "s":
					state = ManyBattleState.HEALSELECT;
					break;
				default:
					state = ManyBattleState.HEALSELECT;
					break;
			}
		}
	}
	// When the player wants to heal, what do
	private void HealingHandler()
	{
		if (playerUnits[altSelect[targeter % 100 - 1]].diceUseable)
		{
            playerUnits[altSelect[targeted % 100 - 1]].Heal(actionValue);
            playerUnits[altSelect[targeter % 100 - 1]].diceUseable = false;
			AudioManager.instance.PlaySound(heal);
			playerHUD[targeted % 100 - 1].SetHP(playerUnits[altSelect[targeted % 100 - 1]].currentHP);
			dialogueText.text = "Healing successfull!";
			int i = 0;
			foreach (Unit var in playerUnits)
			{
				if (var.diceUseable)
					i++;
			}
			if (i > 0)
			{
				state = ManyBattleState.PLAYERSELECT;
				dialogueText.text = "Choose a character";
				for (int j = 0; j < altSelect.Length; j++)
				{
					if (!playerUnits[altSelect[j]].diceUseable)
						playerFrames[altSelect[j]].GetComponent<Button>().interactable = false;
				}
			}
			else
			{
				state = ManyBattleState.ENEMYUSE;
				StartCoroutine(EnemyTurn());
				dialogueText.text = "Enemy turn";
			};
		}
	}
	// When the player wants to shield, what do
	private void ShieldHandler()
	{
		if (playerUnits[altSelect[targeter % 100 - 1]].diceUseable)
		{
			playerUnits[altSelect[targeted % 100 - 1]].SetShields(playerUnits[altSelect[targeted % 100 - 1]].GetShields() + actionValue);
			playerHUD[altSelect[targeted % 100 - 1]].SetShields(""+playerUnits[altSelect[targeted % 100 - 1]].GetShields());
			AudioManager.instance.PlaySound(shield);
			playerUnits[altSelect[targeter % 100 - 1]].diceUseable = false;
			dialogueText.text = "Shielding successfull!";
			int i = 0;
			foreach (Unit var in playerUnits)
			{
				if (var.diceUseable)
					i++;
			}
			if (i > 0)
			{
				state = ManyBattleState.PLAYERSELECT;
				dialogueText.text = "Choose a character";
				for (int j = 0; j < playerUnits.Length; j++)
				{
					if (!playerUnits[altSelect[j]].diceUseable)
						playerFrames[altSelect[j]].GetComponent<Button>().interactable = false;
						//playerUnits[altSelectFrame[j]].GetComponent<Button>().interactable = false;
				}
			}
			else
			{
				state = ManyBattleState.ENEMYUSE;
				dialogueText.text = "Enemy turn";
				StartCoroutine(EnemyTurn());
			}
		}
	}
	// When the player attacks, what do
	IEnumerator AttackHandler()
	{
		if (playerUnits[altSelect[targeter % 100 - 1]].diceUseable)
		{
			playerUnits[altSelect[targeter % 100 - 1]].diceUseable = false;
			enemyUnits[altSelectEn[targeted % 100 - 1]].TakeDamage(actionValue);

			AudioManager.instance.PlaySound(hit);

			enemyHUD[targeted % 100 - 1].SetHP(enemyUnits[altSelectEn[targeted % 100 - 1]].currentHP);
			dialogueText.text = "The attack is successfull!";
			if(enemyUnits[altSelectEn[targeted % 100 - 1]].isDead)
				RemoveEnemy(targeted);
			//if (enemyUnits == null)
			if (aliveEnemies == 0)
			{
				state = ManyBattleState.WON;
				EndBattle();
			}
			foreach (GameObject val in enemyFrames)
			{
				val.GetComponent<Button>().interactable = false;
			}
			foreach (GameObject val in playerFrames)
			{
				val.GetComponent<Button>().interactable = true;
			}
			
			int i = 0;
			foreach(Unit var in playerUnits)
			{
				if (var.diceUseable)
					i++;
			}
			if (i > 0)
			{
				state = ManyBattleState.PLAYERSELECT;
				dialogueText.text = "Choose a character";

				for (int j = 0; j < playerUnits.Length; j++)
				{
					if (!playerUnits[altSelect[j]].diceUseable)
						playerFrames[altSelect[j]].GetComponent<Button>().interactable = false;
					//playerUnits[altSelectFrame[j]].GetComponent<Button>().interactable = false;
				}
			}
			else
			{
				state = ManyBattleState.ENEMYUSE;
				StartCoroutine(EnemyTurn());
				dialogueText.text = "Enemy turn";
			}
			yield return new WaitForSeconds(2f);
		}
	}
	// When the player dies, what do
	public void RemovePlayer(int id)
	{
		AudioManager.instance.PlaySound(die);
		//Debug.Log("removing " + id);
		int changeId = id % 100 - 1;
		bool isNotFour(Unit n)
		{
			return n != playerUnits[altSelect[id % 100 - 1]];
		}
		playerUnits = Array.FindAll(playerUnits, isNotFour).ToArray();
		int i = 0;
		//foreach (Unit var in playerUnits)
		//{
		//	if(i >= changeId)
		//		var.unitId-=1;
		//	i++;
		//}
		bool isNotFourHUD(BattleHUD n)
		{
			return n != playerHUD[altSelect[id % 100 - 1]];
		}
		playerHUD = Array.FindAll(playerHUD, isNotFourHUD).ToArray();
		bool isNotFourFrame(GameObject n)
		{
			return n != playerFrames[altSelect[id % 100 - 1]];
		}
		playerFrames[altSelect[id % 100 - 1]].GetComponent<Button>().interactable = false;
		playerFrames = Array.FindAll(playerFrames, isNotFourFrame).ToArray();
		bool isNotFourId(int n)
		{
			return n != id;
		}
		bool isNotFourFrameId(int n)
		{
			return n != id % 100;
		}
		altSelectFrame = Array.FindAll(altSelectFrame, isNotFourFrameId).ToArray();
		playerDiceIds = Array.FindAll(playerDiceIds, isNotFourId).ToArray();
		for (int j = 0; j < altSelect.Length; j++)
		{
			if (j > changeId)
			{
				altSelect[j] -= 1;
			}
		}
		playerDeadCount++;
	}
	// When the enemy dies, what do
	public void RemoveEnemy(int id)
	{
		AudioManager.instance.PlaySound(die);
		//int changeId = altSelectEn[id % 100 - 1];
		int changeId = id % 100 - 1;
		bool isNotFour(Unit n)
		{
			return n != enemyUnits[altSelectEn[changeId]];
		}
		enemyUnits = Array.FindAll(enemyUnits, isNotFour).ToArray();
		//foreach (Unit var in enemyUnits)
		//{
		//	if (i >= changeId)
		//		var.unitId -= 1;
		//	i++;
		//}
		bool isNotFourFrame(GameObject n)
		{
			return n != enemyFrames[altSelectEn[changeId]];
		}
		enemyFrames[altSelectEn[changeId]].GetComponent<Button>().interactable = false;
		enemyFrames = Array.FindAll(enemyFrames, isNotFourFrame).ToArray();
		bool isNotFourFrameId(int n)
		{
			return n != id % 100 - 1;
		}
		bool isNotFourTarget(int n)
		{
			return n != 0;
		}
		bool isNotFourId(int n)
		{
			return n != id;
		}
		enemyTargets[altSelectEn[changeId]] = 0;
		enemyTargets = Array.FindAll(enemyTargets, isNotFourTarget).ToArray();
		altSelectEnemyFrame = Array.FindAll(altSelectEnemyFrame, isNotFourFrameId).ToArray();
		enemyDiceIds = Array.FindAll(enemyDiceIds, isNotFourId).ToArray();
		for (int j = 0; j < altSelectEn.Length; j++)
		{
			if (j > changeId)
			{
				altSelectEn[j] -= 1;
			}
		}
		aliveEnemies--;
	}
	private void PlayHit()
	{	

	}
}
