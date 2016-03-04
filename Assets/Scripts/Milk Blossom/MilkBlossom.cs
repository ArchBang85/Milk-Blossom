﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MilkBlossom : MonoBehaviour {

    // Making a Hey That's My Fish variant with a custom controller
    // Basic gameplay: Hex grid
    // Each hex has 1 to 3 points
    // Player has one unit to control
    // When they LEAVE a tile, they pick up the points
    // The tile that is left gets removed from play
    // On their turn they can move in a straight unobstructed line as long a distance as they like
    // Players cannot move over empty tiles or tiles with other player units
    // The player who has most points once not more moves can be made wins 

    // Adapting this to the rotator controller:
    // mid wheel could control orientation
    // inner wheel could control distance like a wind-up spring
    // outer wheel could control... a monster at the edges, or a tilting of the tile map that pulls either
    // the AIs or the points from tile to tile

    // In order to make this game I need to implement:
    // A hex grid
    // Points on the grids
    // States for starting and player turn selection and turn movement
    // Player characters
    // Points counting
    // Timer
    // Overall aesthetic

    // AI for enemies: 
    // first scan straight line options
    // for the tiles with three points, also check the next move for whether there are more
    // tiles with three points available, in which case prioritise those for the move
    // if no three point tiles are available, do the same for two point tiles, 
    // else do it for one point tiles
    public GameObject hexTile;
    hexGrid liveHexGrid;
    public int hexGridx = 5;
    public int hexGridy = 5;
    public float hexRadius = 0.5f;
    public bool useAsInnerCircleRadius = true;
    static List<tile> tileList = new List<tile>();
    
    private int targetRange = 2;

    private enum states {starting, planning, moving};
    states currentState = states.starting;

    Vector3[] directions = new Vector3[6];
    class player
    {
        [Range(1,4)]
        int Playernumber;
        bool AI = true;
      
        Vector3 position;


    }

    class tile
    {
        public Vector3 cubePosition;
        [Range(1, 3)]
        public int points;
        public bool active = true;
        public bool occupied = false;
        bool highlighted = false;
        public GameObject tileObject;

        void drawPoints()
        {
            //
        }

        public void setHighlight(bool isOn)
        {
            highlighted = isOn;
            if(highlighted)
            {
                tileObject.GetComponent<Renderer>().material.color = Color.blue;
            } 
                else
            {
                tileObject.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

    public class hexGrid
    {
        public int x = 5;
        public int y = 5;
        public float radius = 0.5f;
        public bool useAsInnerCircleRadius = true;
        int tileCount;

        private float offsetX, offsetY;

        // list of positions
        new Vector3 maxBounds = new Vector3(0, 0, 0);
        new Vector3 minBounds = new Vector3(0, 0, 0);

        public void CreateHexShapedGrid(GameObject hexTile, int gridRadius = 3)
        {

            int[] pointsPool = new int[3];
            pointsPool[0] += tileCount - pointsPool[0] - pointsPool[1] - pointsPool[2];


        }

        public void AllocatePoints(int[] pool)
        {
            for (int i = 0; i < pool.Length; i++)
            // if there are 60 tiles, 10 are triples, 20 are doubles and 30 are singles, i.e. 1/6, 1/3 and 1/2
            {
                pool[i] = Mathf.FloorToInt(tileCount / (2 + i * i));
            }
            List<tile> choosableTiles = new List<tile>(tileList);
            for (int t = 0; t < tileList.Count; t++)
            {

                // randomly choose tile that hasn't been chosen before
                tile chosenTile = choosableTiles[Random.Range(0, choosableTiles.Count - 1)];

                // create points in tiles
                while (chosenTile.points <= 0)
                {

                    int attempts = 100;
                    int r = Random.Range(0, pool.Length);
                    for (int k = 0; k < pool.Length; k++)
                    {
                        if (r == k)
                        {
                            if (pool[k] > 0)
                            {
                                Debug.Log("Adding point to tile " + chosenTile.tileObject + "  " + k);
                                pool[k] -= 1;
                                // this tile gets 1 point
                                chosenTile.points = k + 1;
                                // AddDebugText(chosenTile.tileObject, chosenTile.points.ToString());
                                choosableTiles.Remove(chosenTile);

                            }
                        }
                    }
                    attempts -= 1;
                    if (attempts < 0)
                    {
                        break;
                    }
                }
            }
        }
        public void CreateGrid(GameObject hexTile)
        {
            tileCount = x * y;

            float unitLength = (useAsInnerCircleRadius)? (radius / Mathf.Sqrt(3) / 2) : radius;

            offsetX = unitLength * Mathf.Sqrt(3);
            offsetY = unitLength * 1.5f;

            // create tiles themselves
            for ( int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++ )
                {
                    Vector2 hexPos = HexOffset(i, j);
                    tile newTile = new tile();

                    // convert coords to cube format based on whether row amount is odd or even
                    newTile.cubePosition = OddRToCube(i, j);
                    // randomly choose points amount
                    tileList.Add(newTile);
                    Vector3 pos = new Vector3(hexPos.x, hexPos.y, 0);
                    GameObject newTileObject = (GameObject)Instantiate(hexTile, pos, Quaternion.identity);
                    newTile.tileObject = newTileObject;
                    try
                    {
                        newTileObject.transform.parent = GameObject.Find("HexGrid").transform;
                   
                    }
                    catch
                    {
                        Debug.Log("Was not able to add hex to hex grid");
                    }
                        
                        // AddDebugText(newTileObject, newTile.cubePosition.x.ToString() + "," + newTile.cubePosition.y.ToString() + "," + newTile.cubePosition.z.ToString());

                }
            }
        }

          
        

        void AddDebugText(GameObject targetObject, string inputText)
        {
            try
            {
                //string existingText = targetObject.transform.FindChild("debugtext").gameObject.GetComponent<DebugTooltip>().debugText;
                targetObject.transform.FindChild("debugtext").gameObject.GetComponent<DebugTooltip>().debugText = inputText; 
            } catch
            {
                Debug.Log("Failed to add text");
            }
            
        }

        Vector2 HexOffset( int x, int y)
        {
            Vector2 position = Vector2.zero;

            if(y % 2 == 0)
            {
                position.x = x * offsetX;
                position.y = y * -offsetY;
            } else
            {
                position.x = (x + 0.5f) * offsetX;
                position.y = y * -offsetY;
            }
            return position;
        }

        // hex tile position conversion helper functions
        // odd r to cube
        Vector3 OddRToCube(int x, int y)
        {
            Vector3 cubeCoordinates = new Vector3();
            cubeCoordinates.x = x - (y - (y & 1)) / 2;
            cubeCoordinates.z = y;
            cubeCoordinates.y = -cubeCoordinates.x- cubeCoordinates.z;


            return cubeCoordinates;
        }
        // even r to cube

        Vector3 EvenRToCube(int x, int y)
        {
            Vector3 cubeCoordinates = new Vector3();
            cubeCoordinates.x = x - (y + (y & 1)) / 2;
            cubeCoordinates.z = y;
            cubeCoordinates.y = - cubeCoordinates.x - cubeCoordinates.z;

            return cubeCoordinates;
        }

        public void DisplayIndices()
        {
            // simply number tiles
            for (int i = 0; i < tileList.Count - 1; i++)
            {
                AddDebugText(tileList[i].tileObject, i.ToString());
            }
        }
        public void DisplayCoords()
        {
            // cubic coords
            for (int i = 0; i < tileList.Count - 1; i++)
            {
                string coordText = tileList[i].cubePosition.x.ToString() + ", " + tileList[i].cubePosition.y.ToString() + ", " + tileList[i].cubePosition.z.ToString();
                AddDebugText(tileList[i].tileObject, coordText);
            }
        }
        public void DisplayPoints()
        {
            // points
            for (int i = 0; i < tileList.Count - 1; i++)
            {
                AddDebugText(tileList[i].tileObject, tileList[i].points.ToString());
            }
        }

        public void DisplayClear()
        {
            // clear all
            for (int i = 0; i < tileList.Count - 1; i++)
            {
                AddDebugText(tileList[i].tileObject, "");
            }
        }
    }
    
	// Use this for initialization
	void Start () {

        gridGenerator();

        directions[0] = new Vector3(+1, -1,  0);
        directions[1] = new Vector3(+1,  0, -1);
        directions[2] = new Vector3( 0, +1, -1);
        directions[3] = new Vector3(-1, +1,  0);
        directions[4] = new Vector3(-1,  0, +1);
        directions[5] = new Vector3( 0, -1, +1);

        foreach(tile t in tileList )
        {
            Debug.Log(t.cubePosition);
        }
	}
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.F1))
        {
            liveHexGrid.DisplayIndices();
        }
        if (Input.GetKey(KeyCode.F2))
        {
            liveHexGrid.DisplayCoords();
        }
        if (Input.GetKey(KeyCode.F3))
        {
            liveHexGrid.DisplayPoints();
        }
        if (Input.GetKey(KeyCode.F4))
        {
            liveHexGrid.DisplayClear();
        }


        if (Input.GetKey(KeyCode.H))
        {
            LinearHighlighter(tileList[15], 4, 5);
        }

        if(Input.GetKey(KeyCode.W))
        {
            targetRange++;
        }
        if(Input.GetKey(KeyCode.X))
        {
            targetRange--;
        }

        // Rudimentary highlight controls
        if(Input.GetKey(KeyCode.Q))
        {
            LinearHighlighter(tileList[15], 2 ,  targetRange);
        }

        if (Input.GetKey(KeyCode.E))
        {
            LinearHighlighter(tileList[15], 1, targetRange);
        }

        if (Input.GetKey(KeyCode.A))
        {
            LinearHighlighter(tileList[15], 3, targetRange);
        }
        if (Input.GetKey(KeyCode.D))
        {
            LinearHighlighter(tileList[15], 0, targetRange);
        }
        if (Input.GetKey(KeyCode.Z))
        {
            LinearHighlighter(tileList[15], 4, targetRange);
        }
        if (Input.GetKey(KeyCode.C))
        {
            LinearHighlighter(tileList[15], 5, targetRange);
        }


    }

    void gridGenerator()
    {
        liveHexGrid = new hexGrid();
        liveHexGrid.x = hexGridx;
        liveHexGrid.y = hexGridy;
        liveHexGrid.radius = hexRadius;
        liveHexGrid.useAsInnerCircleRadius = useAsInnerCircleRadius;

        liveHexGrid.CreateGrid(hexTile);
    }

    void LinearHighlighter(tile sourceTile, int direction, int range)
    {
        Mathf.Clamp((float)direction, 0, 5);

        // there should be a way to know from the cubic coordinates whether the tile is on a line
        
        // first, unhighlight all tiles
        foreach (tile t in tileList)
        {
            t.setHighlight(false);
        }

        sourceTile.setHighlight(true);

        for (int r = 1; r <= range; r++)
        {
            Vector3 relativeTargetPosition = directions[direction] * r;
            // try and step to the next tile in the direction
            // does the tile exist
            foreach (tile t in tileList)
            {
                if(t.cubePosition == sourceTile.cubePosition + relativeTargetPosition)
                {
                    t.setHighlight(true);
                }
            }

        }



    }

    void MakeMove(GameObject unit, tile targetTile)
    {
        unit.transform.Translate(targetTile.tileObject.transform.position);
    }

    Vector3 PseudoAIMove(GameObject unit)
    {
        Vector3 target = new Vector3(0, 0, 0);
        List<GameObject> potentialTiles = new List<GameObject>();

        // brute forcing it
        // unit contains own position?


        // for each direction, go along the grid for as far as possible


        // for each tile check the points it contains

        // 


        return target; 
    }

    Vector3 CubeDirection(int dir)
    {
        return directions[dir];
    }

    Vector3 CubeNeighbour(tile hex, int dir)
    {
        Vector3 newPosition = hex.cubePosition + directions[dir];
        return newPosition;
    }
}