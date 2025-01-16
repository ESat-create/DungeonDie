using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int _resolve;
    [SerializeField] private int _resolveMax;
    [SerializeField] public Slider healthBar;
	[SerializeField] private int[] keysIds;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void TakeDamage(int damage)
    {
        _resolve = _resolve - damage;

		if (_resolve <= 0)
			Application.Quit();
		UpdateBar();
		//Debug.Log(_resolve);
        
	}
	public void HealResolve(int amount)
	{
		_resolve = _resolve + amount;
        if (_resolve > _resolveMax) { _resolve = _resolveMax; }
        UpdateBar();
	}
    public void AddKey(int key)
    {
        if (keysIds != null)
        {
            int[] temp = keysIds;
            keysIds = new int[keysIds.Length + 1];
            for (int i = 0; i < temp.Length; i++)
            {
                keysIds[i] = temp[i];
            }
            keysIds[keysIds.Length - 1] = key;
        }
        else
        {
            keysIds = new int[1];
            keysIds[0] = key;
        }
    }
    public void UpdateBar()
    {
        healthBar.value = _resolve;
        healthBar.maxValue = _resolveMax;
    }
	public bool CheckForKey(int key)
	{
		return keysIds.Contains(key);
	}
	// Update is called once per frame
	void Update()
    {
        
    }
}
