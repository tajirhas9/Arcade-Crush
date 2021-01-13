using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
	public GameObject scoreTextObject;
	public GameObject milestoneTextObject;
	public GameObject timeObject;
	public TextMeshProUGUI bonusText;
	public bool timerRunning;
	public float timeRemaining;
	public long milestone;
	private long currentMilestone;
	private long score;
    public event Action OnGameOver;
    private AdManager adManager;
    private FirebaseManager firebaseManager;
    private bool gameRunning;

    void Awake()
    {
    	milestoneTextObject.GetComponent<TMP_Text>().text = currentMilestone.ToString();
        adManager = FindObjectOfType<AdManager>();
        firebaseManager = FindObjectOfType<FirebaseManager>();
    }

    void Update()
    {
        if(firebaseManager.updated) {
            if(gameRunning) {
                UpdateTimer();
                GetScore();
                UpdateScore();
                UpdateMilestone();
            } else {
                Debug.Log("firebaseManager.milestone : " + firebaseManager.milestone);
                milestone = firebaseManager.milestone;
                currentMilestone = milestone;
                FirstTimeUpdate();
                gameRunning = true;
            }
        } else {
            milestoneTextObject.GetComponent<TMP_Text>().text = "Loading";
        }
    }

    private void FirstTimeUpdate() {
        milestoneTextObject.GetComponent<TMP_Text>().text = currentMilestone.ToString();
    }

    private void UpdateTimer() {
    	if (timerRunning) {
            if (timeRemaining > 0) {
                timeRemaining -= Time.deltaTime;
            } else {
                timeRemaining = 0;
                timerRunning = false;
                InfoClass.Score = score.ToString();
                
                adManager.DestroyBannerView();
            //    adManager.ShowInterstitialAd();

            //    adManager.interstitialAd.OnAdClosed += LoadGameOverScene;

                SceneManager.LoadScene("GameOverScene");
            }

            // Change UI
            timeObject.GetComponent<TMP_Text>().text = Mathf.FloorToInt(timeRemaining % 60).ToString() + " s";

            if(timeRemaining <= 5) {
            	ChangeTimerColor();
            }
        }
    }

    private void UpdateScore() {
    	scoreTextObject.GetComponent<TMP_Text>().text = score.ToString();
    }

    private void UpdateMilestone() {
        int inc = 0;
    	if(score >= currentMilestone) {
            
            while(currentMilestone < score) {
        		currentMilestone += milestone;
        		milestoneTextObject.GetComponent<TMP_Text>().text = currentMilestone.ToString();
        		timeRemaining += 10;
                inc += 10;
            }

            StartCoroutine(BonusFade(bonusText));
            AudioManagerScript.PlayBonusSound();
    	}
    }

    private void GetScore() {
    	GameObject gridManagerObject = GameObject.Find("Grid Manager");
    	GridManager gridManagerScript = gridManagerObject.GetComponent<GridManager>();

    	int nodeCount = gridManagerScript.GetCurrentAdjacentCount();

    	score += (2 * nodeCount * nodeCount);
    }


    private IEnumerator BonusFade (TextMeshProUGUI textToUse) {
		yield return StartCoroutine(FadeInText(2f, textToUse));
		yield return new WaitForSeconds(1f);
		yield return StartCoroutine(FadeOutText(2f, textToUse));
	}


	private IEnumerator FadeInText(float timeSpeed, TextMeshProUGUI text)
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        while (text.color.a < 1.0f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a + (Time.deltaTime * timeSpeed));
            yield return null;
        }
    }
    private IEnumerator FadeOutText(float timeSpeed, TextMeshProUGUI text)
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
        while (text.color.a > 0.0f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (Time.deltaTime * timeSpeed));
            yield return null;
        }
    }

    private void ChangeTimerColor() {
    	timeObject.GetComponent<TMP_Text>().color = new Color(220, 20, 60, 1);
    }

    private void LoadGameOverScene(object sender, EventArgs args) {
        SceneManager.LoadScene("GameOverScene");
    }

}