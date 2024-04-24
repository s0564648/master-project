using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ImageManager : MonoBehaviour
{
    public static ImageManager instance;

    [SerializeField]
    private Image image;
    [SerializeField]
    private GameObject ui;
    
    private void Awake()
    {
        //Static instance for easier access
        instance = this;
      
        ShowUI(false);
    }

    public void SetImage(Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void ShowUI(bool show)
    {
        ui.SetActive(show);
    }
}
