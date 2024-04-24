using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemoryGameTile : MonoBehaviour
{
    public Button tileButton;
    [SerializeField]
    private Sprite tileBackground, tileForeground;
    [SerializeField]
    private Image tileImage;
    [SerializeField]
    private TMP_Text tileText;
    [SerializeField]
    private float turnStep = 0.01f, turnSpeed;

    private bool isTurnedUp = false;
    private bool isTurning = false;//For preventing multiple turn animations while one is running

    public int tileCode; //For checking the answers as each pair would have the same tile code
    public bool isTurnable = true; //For disabling the card once it has been matched

    private void Awake()
    {
        tileBackground = tileImage.sprite;
    }

    //Updates all the details of the tile
    public void SetTile(Sprite tileSprite, Sprite background, string text, int code)
    {
        if(tileSprite != null)
        {
            tileForeground = tileSprite;
        }

        if(background != null)
        {
            tileBackground = background;
        }

        if (text != null)
        {
            tileText.text = text;
        }
        else
        {
            tileText.text = "";
        }

        tileCode = code;
        isTurnedUp = false;

        //Update Tile Image to reflect the changes
        UpdateTileImage();
    }

    public void TurnTile()
    {
        if (isTurning) return;
        StartCoroutine(TurnTileCoroutine());
    }

    IEnumerator TurnTileCoroutine()
    {
        isTurning = true;
        tileButton.interactable = false;

        //Rotate the tile 90 degrees
        while(tileImage.transform.rotation.eulerAngles.y != 90)
        {
            tileImage.transform.rotation = Quaternion.Euler(0, Mathf.MoveTowards(tileImage.transform.rotation.eulerAngles.y, 90, turnSpeed), 0);
            yield return new WaitForSeconds(turnStep);
        }

        isTurnedUp = !isTurnedUp;
        UpdateTileImage();

        //Rotate the tile to 0 degrees
        while (tileImage.transform.rotation.eulerAngles.y != 0)
        {
            tileImage.transform.rotation = Quaternion.Euler(0, Mathf.MoveTowards(tileImage.transform.rotation.eulerAngles.y, 0, turnSpeed), 0);
            yield return new WaitForSeconds(turnStep);
        }

        tileButton.interactable = true;
        isTurning = false;
    }

    //Changes the image and visibility of text depending on whether the card is turned up or not
    private void UpdateTileImage()
    {
        tileImage.sprite = !isTurnedUp ? tileBackground : tileForeground;
        tileText.gameObject.SetActive(isTurnedUp);
    }
}
