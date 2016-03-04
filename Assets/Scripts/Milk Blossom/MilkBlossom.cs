using UnityEngine;
using UnityEngine.UI;
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
    // A hex grid                                                           x
    // Points on the grids                                                  x
    // States for starting and player turn selection and turn movement      x
    // Player characters                                                    x
    // Points counting                                                      x
    // Timer    
    // Overall aesthetic

    // AI for enemies: 
    // first scan straight line options
    // for the tiles with three points, also check the next move for whether there are more
    // tiles with three points available, in which case prioritise those for the move
    // if no three point tiles are available, do the same for two point tiles, 
    // else do it for one point tiles

    // further ideas
    // just let players do as many turns as feasible, don't worry about isolated areas (though
    // they need to be taken into account in calculating when the game ends?
    // game ends when a player has no further valid moves
    // player who can't move does a shaky-shake and perishes


    public GameObject hexTile;
    hexGrid liveHexGrid;
    public int hexGridx = 5;
    public int hexGridy = 5;
    public float hexRadius = 0.5f;
    public bool useAsInnerCircleRadius = true;
    static List<tile> tileList = new List<tile>();
    static tile activeTile;
    private int targetRange = 2;
    private float turnCooldown = 0.5f;
    public GameObject[] scoreObjects;


    private enum states {starting, planning, live, moving};
    states currentState = states.starting;
    Vector3[] directions = new Vector3[6];
    [Range(0, 5)]
    private int currentDir;

    // player info
    public GameObject playerObject;
    [Range(1,4)]
    private int players = 3;
    static List<player> playerList = new List<player>();
    private int activePlayer = 1;

    class player
    {
        [Range(1,4)]
        public int playerNumber;
        bool AI = false;
        Vector3 cubePosition;
        Vector2 offsetPosition;
        public tile playerTile;
        public GameObject playerGameObject;
        private int points;
        public void AddPoints(int p)
        {
            points += p;
        }
        public int GetPoints()
        {
            return points;
        }
    }

    public class tile
    {
        public Vector3 cubePosition;
        public Vector2 offsetPosition;
        [Range(1, 3)]
        public int points;
        bool active = true;
        public bool occupied = false;
        bool highlighted = false;
        public GameObject tileObject;

        void drawPoints()
        {
            //
        }

        public void SetHighlight(bool isOn)
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
        public bool GetHighlight()
        {
            return highlighted;
        }

        public void SetOccupied(bool occupyFlag)
        {
            occupied = occupyFlag;
        }
        public bool GetOccupied()
        {
            return occupied;
        }

        public void SetActive(bool activeFlag)
        {
            active = activeFlag;

            if(!active)
            {
                // what should happen when the tile is deactivated?
                tileObject.GetComponent<Renderer>().enabled = false;
            }
                   

        }

        public bool GetActive()
        {
            return active;
        }

    }
    static IEnumerator basicDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
    }

    public class hexGrid
    {
        public int x = 5;
        public int y = 5;
        public float radius = 0.5f;
        public bool useAsInnerCircleRadius = true;
        int tileCount;
        public int playerCount = 3;
        public GameObject playerObj;

        private float offsetX, offsetY;
        private float standardDelay = 0.03f;

        // list of positions
        new Vector3 maxBounds = new Vector3(0, 0, 0);
        new Vector3 minBounds = new Vector3(0, 0, 0);

        public IEnumerator CreateHexShapedGrid(GameObject hexTile, int gridRadius = 3)
        {

            float delayModifier = 1.0f;
            float unitLength = (useAsInnerCircleRadius) ? (radius / Mathf.Sqrt(3) / 2) : radius;

            offsetX = unitLength * Mathf.Sqrt(3);
            offsetY = unitLength * 1.5f;

            // create in a shape of a hexagon

            for (int q = -gridRadius; q <= gridRadius; q++)
            {
                int r1 = Mathf.Max(-gridRadius, -q - gridRadius);
                int r2 = Mathf.Min(gridRadius, -q + gridRadius);
                for (int r = r1; r <= r2; r++)
                {
                    // create tile class
                    tile newTile = new tile();
                    tileList.Add(newTile);
                    tileCount++;

                    newTile.cubePosition = new Vector3(q, r, -q- r);
                    Vector2 offset = CubeToOddR(newTile.cubePosition);
                    Vector2 hexPos = HexOffset((int)offset.x, (int)offset.y);
                    Vector3 pos = new Vector3(hexPos.x, hexPos.y, 0);
                    newTile.offsetPosition = pos;

                }

            }
            foreach (tile t in tileList)
            {

                GameObject newTileObject = (GameObject)Instantiate(hexTile, t.offsetPosition, Quaternion.identity);
                t.tileObject = newTileObject;
                t.tileObject.transform.parent = GameObject.Find("HexGrid").transform;
                yield return new WaitForSeconds(standardDelay);
            }

            // do remaining setup things within ienumerator to ensure sequence is correct
            AllocatePoints();
            yield return new WaitForSeconds(1.6f);
            AllocatePlayers(playerObj);
            activeTile = SelectPlayer(1);

        }

        public void AllocatePoints()
        {

            int[] pool = new int[3];
            for (int i = 0; i < pool.Length; i++)
            // if there are 60 tiles, 10 are triples, 20 are doubles and 30 are singles, i.e. 1/6, 1/3 and 1/2
            {
                pool[i] = Mathf.FloorToInt(tileCount / (2 + i * i));
            }
            pool[0] += tileCount - pool[0] - pool[1] - pool[2];

            List<tile> choosableTiles = new List<tile>(tileList);
            for (int t = 0; t < tileList.Count; t++)
            {

                // randomly choose tile that hasn't been chosen before
                tile chosenTile = choosableTiles[Random.Range(0, choosableTiles.Count - 1)];
                int attempts = 20;
                // create points in tiles
                while (chosenTile.points <= 0)
                {

         
                    int r = Random.Range(0, pool.Length);
                    for (int k = 0; k < pool.Length; k++)
                    {
                        if (r == k)
                        {
                            if (pool[k] > 0)
                            {
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
        public void AllocatePlayers(GameObject playerObject)
        {
            // legitimate tiles are the ones with one point only
            List<tile> validAllocationTiles = new List<tile>();

            foreach (tile t in tileList)
            {
                if (t.points == 1)
                {
                    validAllocationTiles.Add(t);
                }
            }

            for (int p = 1; p <= playerCount; p++)
            {
                int attempts = 20;
                while (true)
                {
                    attempts -= 1;
                    if(attempts < 0)
                    {
                        break;
                    }
                    tile chosenTile = validAllocationTiles[Random.Range(0, validAllocationTiles.Count)];
                    if (!chosenTile.GetOccupied())
                    {
                        // allocate player on tile
                        Debug.Log("allocating player");
                        //yield return new WaitForSeconds(standardDelay);
                        GameObject newPlayer = (GameObject)Instantiate(playerObject, new Vector3(chosenTile.tileObject.transform.position.x, chosenTile.tileObject.transform.position.y, -0.5f), Quaternion.identity);
                        chosenTile.SetOccupied(true);
                        player pl = new player();
                        playerList.Add(pl);
                        pl.playerNumber = p;
                        pl.playerTile = chosenTile;
                        pl.playerGameObject = newPlayer;
                        break;
                    }
                }
            }




        }

        public void CreateGrid(GameObject hexTile)
        {
            tileCount = x * y;

            // create tiles themselves
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
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
        
        public void leaveTile(tile tileToLeave)
        {
            tileToLeave.SetOccupied(false);
            tileToLeave.SetActive(false);

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
                position.y = y * +offsetY;
            } else
            {
                position.x = (x + 0.5f) * offsetX;
                position.y = y * +offsetY;
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

        // cube to odd r
        Vector2 CubeToOddR(Vector3 cubeCoords)
        {
            Vector2 oddR = new Vector2();
            oddR.x = (int)cubeCoords.x + ((int)cubeCoords.z - ((int)cubeCoords.z &1))/2;
            oddR.y = (int)cubeCoords.z;

            return oddR;
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
            for (int i = 0; i < tileList.Count; i++)
            {
                AddDebugText(tileList[i].tileObject, i.ToString());
            }
        }
        public void DisplayCoords()
        {
            // cubic coords
            for (int i = 0; i < tileList.Count; i++)
            {
                string coordText = tileList[i].cubePosition.x.ToString() + ", " + tileList[i].cubePosition.y.ToString() + ", " + tileList[i].cubePosition.z.ToString();
                AddDebugText(tileList[i].tileObject, coordText);
            }
        }
        public void DisplayPoints()
        {
            // points
            for (int i = 0; i < tileList.Count; i++)
            {
                AddDebugText(tileList[i].tileObject, tileList[i].points.ToString());
            }
        }

        public void DisplayClear()
        {
            // clear all
            for (int i = 0; i < tileList.Count; i++)
            {
                AddDebugText(tileList[i].tileObject, "");
            }
        }
    }
    
	// Use this for initialization
	void Start () {

        InitGame();

        directions[0] = new Vector3(+1, -1,  0);
        directions[1] = new Vector3(+1,  0, -1);
        directions[2] = new Vector3( 0, +1, -1);
        directions[3] = new Vector3(-1, +1,  0);
        directions[4] = new Vector3(-1,  0, +1);
        directions[5] = new Vector3( 0, -1, +1);

    
	}
    // Update is called once per frame
    void Update()
    {

        // timer before live

        if (currentState == states.live)
        {
            turnCooldown -= Time.deltaTime;
            if (turnCooldown < 0)
            {
                turnCooldown = 0.1f;
                // CONTROLS
                // debug visualisations 
                if (Input.GetKey(KeyCode.F1))
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

                // DEBUG TURN SWITCHING
                if (Input.GetKey(KeyCode.N))
                {
                    IncrementActivePlayer();
                }

                if (Input.GetKey(KeyCode.W))
                {
                    targetRange++;
                }
                if (Input.GetKey(KeyCode.X))
                {
                    targetRange--;
                }

                // Rudimentary highlight controls
                if (Input.GetKey(KeyCode.Q))
                {
                    currentDir = 4;
                }

                if (Input.GetKey(KeyCode.E))
                {
                    currentDir = 5;
                }

                if (Input.GetKey(KeyCode.A))
                {
                    currentDir = 3;

                }
                if (Input.GetKey(KeyCode.D))
                {
                    currentDir = 0;
                }
                if (Input.GetKey(KeyCode.Z))
                {
                    currentDir = 2;
                }
                if (Input.GetKey(KeyCode.C))
                {
                    currentDir = 1;
                }

                tile targetTile = LinearHighlighter(activeTile, currentDir, targetRange);

                if (Input.GetKey(KeyCode.Return))
                {
                    if (activeTile != targetTile)
                    {

                        MakeMove(playerList[activePlayer - 1], targetTile);
                        IncrementActivePlayer();
                    }
                }
            }

            
        }
    }

    void IncrementActivePlayer()
    {
        activePlayer++;
        if (activePlayer > players)
        {
            activePlayer = 1;
        }
        activeTile = SelectPlayer(activePlayer);
    }
    IEnumerator SetupPlayers()
    {

        for(int p = 0; p < players; p++)
        {

        }
        yield return new WaitForSeconds(1.0f);
    }

    void InitGame()
    {
        // create grid, allocate points and allocate players
        liveHexGrid = new hexGrid();
        liveHexGrid.x = hexGridx;
        liveHexGrid.y = hexGridy;
        liveHexGrid.radius = hexRadius;
        liveHexGrid.useAsInnerCircleRadius = useAsInnerCircleRadius;
        liveHexGrid.playerCount = players;
        liveHexGrid.playerObj = playerObject;

        StartCoroutine(liveHexGrid.CreateHexShapedGrid(hexTile));
        StartCoroutine(basicDelay(1.0f));

        // once game is setup, set it to live
        StartCoroutine(switchState(states.live, 3.0f));
        
        // set player amounts
        for (int i = 0; i < players; i++)
        {
            if (i < 2)
            {
                scoreObjects[i].transform.GetComponent<Text>().text = "P"+ (i + 1).ToString()+"\n"+"0";
            }
            else
            {
                scoreObjects[i].transform.GetComponent<Text>().text = "0\n" + "P" + (i + 1).ToString();
            }  
        }
    }

    IEnumerator switchState(states s, float delay)
    {
        yield return new WaitForSeconds(delay);
        currentState = s;
        Debug.Log("Switced to " + s);
    }


    static tile SelectPlayer(int playerNumber = 1)
    {
        Debug.Log("playernumber" + playerNumber);
        foreach(player p in playerList)
        {
            Debug.Log(p.playerNumber);
            if (p.playerNumber == playerNumber)
            {
                Debug.Log("Setting active tile");
                return p.playerTile;
            }
        }
        return null;
    }

    tile LinearHighlighter(tile sourceTile, int direction, int range)
    {
        Mathf.Clamp((float)direction, 0, 5);

        // there should be a way to know from the cubic coordinates whether the tile is on a line
        
        // first, unhighlight all tiles
        foreach (tile t in tileList)
        {
            t.SetHighlight(false);
        }

        try
        {
            sourceTile.SetHighlight(true);
        } catch
        {

        }

        tile targetTile = sourceTile;
        for (int r = 1; r <= range; r++)
        {
            Vector3 relativeTargetPosition = directions[direction] * r;
            // try and step to the next tile in the direction
            // does the tile exist
            foreach (tile t in tileList)
            {
                if (t.cubePosition == sourceTile.cubePosition + relativeTargetPosition)
                {
                    if (t.GetActive() && !t.GetOccupied())
                    {
                        t.SetHighlight(true);
                        targetTile = t;
                    }
                    else
                    {
                        // if it's not a valid tile then it is either deactivated or occupied and the last tile should be the one before the obstacle
                        range = r; // bad practice, setting an int within the function that's returning a type
                        return targetTile;

                    }
                }          
            }

        }
        return targetTile;
    }

    void UpdateScores()
    {
        
    }


    void MakeMove(player p, tile targetTile)
    {
        // acquire points
        p.AddPoints(p.playerTile.points);

        // leave current tile
        liveHexGrid.leaveTile(p.playerTile);
        
        // move player unit to new tile
        p.playerGameObject.transform.position = new Vector3(targetTile.tileObject.transform.position.x, targetTile.tileObject.transform.position.y, p.playerGameObject.transform.position.z);

        // set player tile as the target tile
        p.playerTile = targetTile;

        activeTile = targetTile;
        activeTile.SetOccupied(true);
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
