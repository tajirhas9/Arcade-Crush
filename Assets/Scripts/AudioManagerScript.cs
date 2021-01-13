using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerScript : MonoBehaviour
{
	public static AudioClip[] scoreSound;
	public static AudioClip[] bonusSound;

	static AudioSource audioSource;

    void Start()
    {
    	scoreSound = new AudioClip[3];
    	bonusSound = new AudioClip[2];
    	
    	scoreSound[0] = Resources.Load<AudioClip>("score");
    	scoreSound[1] = Resources.Load<AudioClip>("score1");
    	scoreSound[2] = Resources.Load<AudioClip>("score2");

    	bonusSound[0] = Resources.Load<AudioClip>("bonus");
    	bonusSound[1] = Resources.Load<AudioClip>("bonus1");

    	audioSource = GetComponent<AudioSource>();
    	
    }

    public static void PlayScoreSound() {
    	audioSource.PlayOneShot( scoreSound[ Random.Range(0,3) ] );
    }

    public static void PlayBonusSound() {
    	audioSource.PlayOneShot( bonusSound[ Random.Range(0,2) ] );
    }
}
