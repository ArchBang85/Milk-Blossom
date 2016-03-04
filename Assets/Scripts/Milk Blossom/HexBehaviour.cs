﻿using UnityEngine;
using System.Collections;

public class HexBehaviour : MonoBehaviour {
    float myTime = 0f;
    bool paused = false;
	// Use this for initialization
	void Start () {
	    iTween.MoveBy(this.gameObject, iTween.Hash("z", 0.15, "easeType", "easeInOutCubic", "loopType", "pingPong", "delay", .02));
	}
	
	// Update is called once per frame
	void Update () {
        if(myTime < 2.1f)
            myTime += Time.deltaTime;

        if (!paused)
        {
            if (myTime > 2.1f)
            {
                iTween.Pause(this.gameObject);
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
                paused = true;
            }
        }
	}
}
