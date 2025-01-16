using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager instance { get; private set; }

	private AudioSource audioPlayer;

	private void Awake()
	{

		audioPlayer = GetComponent<AudioSource>();

		if (instance != null)
			Destroy(gameObject);
		else
			instance = this;

		DontDestroyOnLoad(gameObject);
	}
	public void PlaySound(AudioClip soundToPlay)
	{
		audioPlayer.PlayOneShot(soundToPlay);
	}
}
