using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class PrefabManager : MonoBehaviour
{
	public bool isClicked = false;
	private bool mouseHover = false;

    private float height, width;

    void Awake() {
        height = Camera.main.orthographicSize * 2;
        width = height * Screen.width/ Screen.height; // basically height * screen aspect ratio
    }

    void Update()
	{
        if( (mouseHover && Input.GetMouseButtonDown(0)) ) {
			isClicked = true;
		}
}

	void OnMouseEnter() {
    	mouseHover = true;
    }
    void OnMouseExit() {
    	mouseHover = false;
    	isClicked = false;
    }

    public IEnumerator Disappear() {
    	Destroy(gameObject);
    	yield return new WaitForSeconds(2f);
    	
    }

    
}
