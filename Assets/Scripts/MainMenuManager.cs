using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text highScoreText;

    public const int MAIN_MENU_SCENE_INDEX = 0;
    public const int MEMORY_GAME_SCENE_INDEX = 2;

    private void Start()
    {
        UpdateHighScoreText();
    }

    public void PlayButton()
    {
        SceneManager.LoadScene(GameManager.GAME_SCENE_INDEX);
    }

    public void MemoryGameButton()
    {
        SceneManager.LoadScene(MEMORY_GAME_SCENE_INDEX);
    }

    private void UpdateHighScoreText()
    {
        int highScore = PlayerPrefs.GetInt(GameManager.HIGH_SCORE, 0);
        highScoreText.text = $"High Score: {highScore}";
    }
}
