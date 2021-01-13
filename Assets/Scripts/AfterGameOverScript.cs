using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class AfterGameOverScript : MonoBehaviour
{
	public GameObject scoreTextObject;

    void Start()
    {
    	scoreTextObject.GetComponent<TMP_Text>().text = InfoClass.Score.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount > 0 || Input.GetMouseButtonDown(0)) {
        	SceneManager.LoadScene("MainScene");
        }
    }
}
