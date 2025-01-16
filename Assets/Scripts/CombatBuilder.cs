using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatBuilder : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject canvasDungeon;
	[SerializeField] GameObject cam;
	[SerializeField] GameObject manyBattleSystem;
    [SerializeField] public GameObject player;
	CapsuleCollider collider;
	// Start is called before the first frame update

	void Start()
    {
        collider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            SetCombat();
        if (Input.GetKeyDown(KeyCode.X))
		{
			SetDungeon();
			player.GetComponent<PlayerController>().TakeDamage(5);
		}
	}
    public void SetCombat()
    {
        canvas.SetActive(true);
		cam.SetActive(true);
		manyBattleSystem.SetActive(true);
		player.SetActive(false);
		canvasDungeon.SetActive(false);
	}
	public void SetDungeon()
	{
		canvas.SetActive(false);
		cam.SetActive(false);
		manyBattleSystem.SetActive(false);
		player.SetActive(true);
		canvasDungeon.SetActive(true);

		this.gameObject.SetActive(false);
	}
	private void OnTriggerEnter(Collider other)
	{
		PlayerController player = other.GetComponent<PlayerController>();
		if (player == null)
		{
			return;
		}

		SetCombat();
	}
}
