using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MemoryGameManager : MonoBehaviour
{
    [SerializeField]
    private Sprite tileBackgroundSprite, defaultForegroundSprite;
    [SerializeField]
    private float waitTimeOnWrongAnswer = 1.2f;
    [SerializeField]
    private GameObject gameTilePrefab;
    [SerializeField]
    private Transform tilesContainer;
    [SerializeField]
    private string csvSeparator = ",";
    [SerializeField]
    private bool isShuffleEnabled = true;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private Canvas canvas;
    private MemoryGameTile tileOne, tileTwo; //Selected Tiles

    private List<MemoryGameTile> tilesList;

    private int tilePairsMatched, totalTilePairs;
    private bool isWaiting = false;

    public const int MEMORY_GAME_SCENE = 2;

    public static MemoryGameManager instance;

    private GameManager gameManager;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        cam.gameObject.SetActive(false);
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        if(gameManager != null)
        {
            //Update Canvas settings according to current camera
            canvas.worldCamera = Camera.main;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

            if (gameManager.CurrentQuestion.memoryGameIndex >= 0)
            {
                //Load Question from Memory Game index of the current question
                ImportDataFromCSV(gameManager.CurrentQuestion.memoryGameIndex);
            }
            else
            {
                gameManager.HideMemoryGame();
            }
        }
    }

    public void ImportDataFromCSV(int questionIndex)
    {
        //Load the CSV file
        var memoryGameCSV = Resources.Load("MemoryGame");

        //Divides the csv file into an array containing all the lines
        List<string> csvStrings = memoryGameCSV.ToString().Split("\n").ToList();

        //Remove Index 0 as it is the first row with labels
        csvStrings.RemoveAt(0);

        //Load the correct question string using the question Index
        string questionString = csvStrings[questionIndex];

        string[] questionElements = questionString.Split(csvSeparator);
        int numberOfTiles = questionElements.Length / 2; //Since there are two columns for each tile - name and image
        int currentTileIndex = 0;

        List<TileData> tileDataList = new List<TileData>();

        for (int i = 0; i < numberOfTiles; i++)
        {
            //Add one tile for the name or text
            TileData tileData = new TileData();
            tileData.name = questionElements[currentTileIndex];
            tileData.tileCode = currentTileIndex;
            tileDataList.Add(tileData);

            //Add another tile for the image
            TileData tileDataImage = new TileData();
            string imageFileName = questionElements[currentTileIndex + 1].Trim();
            Sprite tileImage = GetSpriteFromResources(imageFileName);
            tileDataImage.foregroundTile = tileImage;
            tileDataImage.tileCode = currentTileIndex;
            tileDataList.Add(tileDataImage);

            //Increase the tileIndex by 2 to go to the next tile name
            currentTileIndex += 2;
        }

        //Shuffle the tile options if shuffle is enabled
        if (isShuffleEnabled)
            ShuffleTiles(tileDataList);

        //Update the UI
        SetupTiles(tileDataList.ToArray());
    }

    //Changes the order of the tiles in the tileDataList
    private void ShuffleTiles(List<TileData> tileDataList)
    {
        int last = tileDataList.Count - 1;

        for (int i = 0; i < last; i++)
        {
            int randomIndex = Random.Range(i, last);
            TileData temp = tileDataList[randomIndex];
            tileDataList[randomIndex] = tileDataList[i];
            tileDataList[i] = temp;
        }
    }

    private Sprite GetSpriteFromResources(string fileName)
    {
        //Loads Image file from Tiles folder
        return Resources.Load<Sprite>($"Tiles/{fileName}");
    }

    public void SetupTiles(TileData[] tileDataArr)
    {
        tilesList = new List<MemoryGameTile>();
        totalTilePairs = tileDataArr.Length/2;

        //Destroy existing tiles
        for(int i = 0; i < tilesContainer.childCount; i++)
        {
            Destroy(tilesContainer.GetChild(i).gameObject);
        }

        //Spawn tiles and update each tile's UI
        foreach(TileData tileData in tileDataArr)
        {
            GameObject tileObject = Instantiate(gameTilePrefab, tilesContainer);
            MemoryGameTile tile = tileObject.GetComponent<MemoryGameTile>();
            Sprite foregroundSprite = tileData.foregroundTile == null ? defaultForegroundSprite : tileData.foregroundTile;
            tile.SetTile(foregroundSprite, tileBackgroundSprite, tileData.name, tileData.tileCode);
            tile.tileButton.onClick.AddListener(() => SelectTile(tile));
            tilesList.Add(tile);
        }

        tilePairsMatched = 0;
        DeselectAllTiles();
    }

    public void SelectTile(MemoryGameTile tile)
    {
        //Return if waiting for the previous cards to turn back
        if (isWaiting) return;

        //No Other tiles are selected
        if (tileOne == null && tileTwo == null)
        {
            tileOne = tile;
        }

        //Only tile one is selected
        else if (tileTwo == null)
        {
            tileTwo = tile;
        }

        //Both tiles are selected
        else
        {
            //Deselect tiles and select the new tile as tileOne
            DeselectAllTiles();
            tileOne = tile;
        }

        tile.TurnTile();
        CheckAnswer();
    }

    private void DeselectAllTiles()
    {
        tileOne = null;
        tileTwo = null;
    }

    public void CheckAnswer()
    {
        //If Both tiles are selected
        if(tileOne != null && tileTwo != null)
        {
            //Deselect all tiles if both are the same tile and skip checking for the answer
            if(tileOne == tileTwo)
            {
                DeselectAllTiles();
                return;
            }

            //Correct Answer
            if(tileOne.tileCode == tileTwo.tileCode)
            {
                //Lock the cards
                tileOne.isTurnable = false;
                tileTwo.isTurnable = false;

                StartCoroutine(DisappearTiles(tileOne, tileTwo));

                tilePairsMatched++;
                //Check if all pairs of tiles have been matched
                CheckAllTilesMatched();
            }
            //Wrong Answer
            else
            {
                //For not allowing the player to click anything else until the cards flip back
                isWaiting = true;

                //Turn the tiles again after waiting for some time
                IEnumerator TurnTiles(MemoryGameTile tile1, MemoryGameTile tile2)
                {
                    yield return new WaitForSeconds(waitTimeOnWrongAnswer);
                    tile1.TurnTile();
                    tile2.TurnTile();
                    DeselectAllTiles();
                    isWaiting = false;
                }

                //Reduce PLayer lives if wrong answer selected
                gameManager?.DecrementLives();

                StartCoroutine(TurnTiles(tileOne, tileTwo));
            }
        }
    }

    //For making the tiles disappear when the answer is correct
    IEnumerator DisappearTiles(MemoryGameTile tile1, MemoryGameTile tile2)
    {
        yield return new WaitForSeconds(waitTimeOnWrongAnswer);

        float interval = 0.01f;
        float disappearSpeed = 0.05f;

        //Run the loop to reduce the scale while the scale of the tiles is greater than 0
        while(tile1.transform.localScale.magnitude > 0 || tile2.transform.localScale.magnitude > 0)
        {
            tile1.transform.localScale = Vector2.MoveTowards(tile1.transform.localScale, Vector2.zero, disappearSpeed);
            tile2.transform.localScale = Vector2.MoveTowards(tile1.transform.localScale, Vector2.zero, disappearSpeed);
            yield return new WaitForSeconds(interval);
        }
    }

    public void CheckAllTilesMatched()
    {
        if(tilePairsMatched >= totalTilePairs)
        {
            if(gameManager != null)
            {
                gameManager.OnMemoryGameCompleted();
            }
        }
    }
}

//For storing the data of the tiles in one variable
public struct TileData
{
    public string name;
    public Sprite foregroundTile;
    public int tileCode;
}
