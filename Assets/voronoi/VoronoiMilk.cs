using UnityEngine;

public class VoronoiMilk : MonoBehaviour {

    public float minX = -1;
    public float maxX = +1;

    public float minY = -1;
    public float maxY = +1;

    public int length = 100;
    
    public Vector2[] points;
    public Color[] colours;
    public Color[] colourChoices;
    public Vector2[] destinationPoints;

    private float moveCounter = 0.15f;

    private Material material;
	// Use this for initialization
	void Start () {
        material = GetComponent<Renderer>().sharedMaterial;
        points = new Vector2[length];
        destinationPoints = new Vector2[length];
        colours = new Color[length];

        for (int i = 0; i < length; i++)
        {
            points[i] = new Vector2
                (
                    transform.position.x + Random.Range(minX, maxX),
                    transform.position.y + Random.Range(minY, maxY)
                );

            destinationPoints[i] = new Vector2
                (
                    points[i].x += Random.Range(-0.5f, 0.5f),
                    points[i].y += Random.Range(-0.5f, 0.5f)
                );
                
            colours[i] = colourChoices[Random.Range(0, colourChoices.Length)];
            // shader 
            material.SetVector("_Points" + i.ToString(), points[i]);
            material.SetVector("_Colors" + i.ToString(), colours[i]);
        }
        material.SetInt("_Length", length);
	}

    [Range(0, 1)]
    public float amount = 0;
    float tParam = 0.0f;
    void FixedUpdate()
    {
 
            if (tParam < 1)
            {
                tParam += Time.deltaTime;
            } else
            {
               tParam = 0.0f;
               SetTargetPoints();
            }

            if (amount == 0)
                return;

            for (int i = 0; i < length; i++)
            {
                points[i].x = Mathf.Lerp(points[i].x, destinationPoints[i].x, tParam);//Random.Range(0.1f, 0.3f));
                points[i].y = Mathf.Lerp(points[i].y, destinationPoints[i].y, tParam);//Random.Range(0.1f, 0.3f));

                // Shader 
                material.SetVector("_Points" + i.ToString(), points[i]);
                material.SetVector("_Colors" + i.ToString(), colours[i]);
            }
       // }
    }

    void SetTargetPoints()
    {
        for (int i = 0; i < length; i++)
        {
            destinationPoints[i] = new Vector2
            (
                points[i].x += Random.Range(-0.5f, 0.5f),
                points[i].y += Random.Range(-0.5f, 0.5f)
            );
        }
    }
	
}
