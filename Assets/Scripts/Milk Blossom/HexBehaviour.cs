using UnityEngine;
using System.Collections;

public class HexBehaviour : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
	    iTween.MoveBy(this.gameObject, iTween.Hash("z", 0.2, "easeType", "easeInOutExpo", "loopType", "pingPong", "delay", .1));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
