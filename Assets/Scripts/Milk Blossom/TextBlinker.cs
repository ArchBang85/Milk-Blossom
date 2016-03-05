using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextBlinker : MonoBehaviour {

    public bool startingText = false;
    public GameObject nextText;
    public float initialDelay = 1.2f;
    public float preBlinkDelay = 3.0f;
    bool fired = false;
    int[] fibo =  {2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 232};
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        preBlinkDelay -= Time.deltaTime;
        if (preBlinkDelay < 0 && !fired)
        {
            fired = true;
            if (startingText)
            {
                FireText();
            }
        }
    }

    public void FireText()
    {
        StartCoroutine("Blink", initialDelay);
    }

    IEnumerator Blink(float delay)
    {
        for (int i = 0; i < fibo.Length; i++)
        {
            yield return new WaitForSeconds(delay / fibo[i]);
            RendererToggle();
        }
    }


    /*
    // Rather hand built method for making the instructional texts blink
    IEnumerator Blink(float delay)
    {
        this.transform.GetComponent<Text>().enabled = false;

        yield return new WaitForSeconds(delay);
        RendererToggle();
        for (int i = 0; i < fibo.Length; i++)
        {
            yield return new WaitForSeconds(delay / fibo[i]);
            RendererToggle();
        }
        RendererToggle();
        yield return new WaitForSeconds(delay);
        RendererToggle();

        for (int i = 0; i < 6; i++)
        {
            yield return new WaitForSeconds(delay / 5);
            RendererToggle();
        }

        yield return new WaitForSeconds(delay / 2);
        if(nextText != null)
        {
            nextText.GetComponent<TextBlinker>().FireText();
        }
    }
    */
    // Toggle text visibility
    void RendererToggle()
    {
        if (this.transform.GetComponent<Text>().enabled == true)
        {
            this.transform.GetComponent<Text>().enabled = false;
        }
        else
        {
            this.transform.GetComponent<Text>().enabled = true;
        }
    }
}
