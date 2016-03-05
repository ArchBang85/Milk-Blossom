using UnityEngine;
using System.Collections;

public class HairCreator : MonoBehaviour {
    float timer;
    public GameObject goldenHair;
    public GameObject ashenHair;
	// Use this for initialization
	void Start () {
        timer = Random.Range(2, 4);
	}
	
	// Update is called once per frame
	void Update () {
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            timer = Random.Range(2, 4);
            if(Random.Range(1,4) < 3)
            {
                Instantiate(ashenHair, new Vector3(transform.position.x + Random.Range(-5, 5), transform.position.y, transform.position.z), Quaternion.identity);
            } else
            {
                Instantiate(goldenHair, new Vector3(transform.position.x + Random.Range(-5, 5), transform.position.y, transform.position.z), Quaternion.identity);
            }

        }
	}
}
