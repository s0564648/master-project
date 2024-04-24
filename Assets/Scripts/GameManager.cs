using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private string csvSeparator = ",";
    [SerializeField]
    private float waitTimeBeforeNextQuestion = 1f;
    [SerializeField]
    private int pointsPerCorrectAnswer, negativePointsPerWrongAnswer, pointsPerMemoryGame = 150, pointsOnStreak=100;
    [SerializeField]
    private bool randomizeQuestions = true;
    [SerializeField]
    private Color correctColor = Color.green, wrongColor = Color.red, neutralColor = Color.white;
    [SerializeField]
    private float timerDuration = 60f;
    [SerializeField]
    private int numberOfLives = 3, showEffectAfterCorrectAnswers = 3;
    [SerializeField]
    private int additionalScorePer10Seconds = 20;
    [SerializeField]
    private Animator thriceEffectAnim;

    [Header("UI")]
    [SerializeField]
    private TMP_Text questionText;
    [SerializeField]
    private Button[] optionButtons;
    [SerializeField]
    private TMP_Text scoreText;
    [SerializeField]
    private ProgressBar progressBar;
    [SerializeField]
    private Button mediaButton;
    [SerializeField]
    private TMP_Text mediaButtonText;
    [SerializeField]
    private GameObject optionsContainer, streakUI;


    [Header("Game Over UI")]
    [SerializeField]
    private GameObject gameOverUI;
    [SerializeField]
    private TMP_Text gameOverScoreText, gameOverTitleText, highScoreText, timerText, livesText;

    private List<Question> questions;
    private int score, currentQuestionIndex;

    public const string HIGH_SCORE = "HIGH_SCORE";
    public const int GAME_SCENE_INDEX = 1;

    private AudioSource audioSource;

    private float timer;
    private bool isGameOver, isTimerRunning;
    private int remainingLives;
    private int continuousCorrectAnswers;
    private int wrongAnswerCount;
    private bool isStreakActive;

    public Question CurrentQuestion
    {
        get
        {
            return questions[currentQuestionIndex];
        }
    }

    private void Awake()
    {
        //Import all the questions from the csv file into the game
        questions = new List<Question>();
        ImportQuestions();

        //If questions are to be randomzied, then shuffle the questions list
        if (randomizeQuestions)
        {
            ShuffleQuestions(questions);
        }

        //Show the First Question
        currentQuestionIndex = 0;
        UpdateQuestionUI();

        timer = timerDuration;
        continuousCorrectAnswers = 0;
        wrongAnswerCount = 0;
        remainingLives = numberOfLives;
        score = 0;
        isTimerRunning = true;
        isStreakActive = false;

        UpdateLivesText();


        isGameOver = false;
        UpdateScoreText();
        UpdateProgressBar();
        UpdateStreakUI();

        gameOverUI.SetActive(false);
    }

    private void Start()
    {
        ShowMemoryGame();
    }

    private void Update()
    {
        //If game is over there isn't any need to update timer, so stop running the code below
        if (isGameOver) return;

        CheckTimer();
    }

    private void UpdateLivesText()
    {
        livesText.text = $"Leben: {remainingLives}";
    }

    private void UpdateStreakUI()
    {
        streakUI.SetActive(isStreakActive);
    }

    private void CheckTimer()
    {
        
        if (!isTimerRunning) return;

        
        if(timer <= 0)
        {
            GameOver("Zeit vorbei!");
        }
        
        else
        {
            timer -= Time.deltaTime;
            //Round timer value to integer values to prevent showing the numbers after decimal
            timerText.text = Mathf.RoundToInt(timer).ToString();
        }
    }

    //Changes the order of the questions in the list
    private void ShuffleQuestions(List<Question> questions)
    {
        int last = questions.Count - 1;

        for (int i = 0; i < last; i++)
        {
            int randomIndex = Random.Range(i, last);
            Question temp = questions[randomIndex];
            questions[randomIndex] = questions[i];
            questions[i] = temp;
        }
    }

    private void ImportQuestions()
    {
        //Load the CSV file and convert it into a list of Questions
        var questionsCSV = Resources.Load("Questions");

        //Divides the csv file into an array containing all the lines
        List<string> questionStrings = questionsCSV.ToString().Split("\n").ToList();

        //Remove Index 0 as it is the first row with labels
        questionStrings.RemoveAt(0);

        int numberOfOptions = 4;
        int numberOfOtherFields = 6;
        int totalNumberOfElements = numberOfOptions + numberOfOtherFields;

        //Divide each line into all the elements of the question, create a question and add to the list of questions
        foreach (string questionString in questionStrings)
        {
            string[] questionElements = questionString.Split(csvSeparator);

            if(questionElements.Length < totalNumberOfElements)
            {
                continue;
            }

            Question question = new Question();
            question.question = questionElements[0];
            question.options = new string[numberOfOptions];

            for(int i = 1; i < 1 + numberOfOptions; i++)
            {
                question.options[i - 1] = questionElements[i];
            }

            int correctAnswerIndex = 5;
            question.correctAnswer = questionElements[correctAnswerIndex];

            //Check for Media Files
            string imageFileName = questionElements[6];
            string videoFileName = questionElements[7];
            string audioFileName = questionElements[8];

            //Load media file from Resources folder and add it to question
            if (imageFileName != "")
            {
                Sprite image = Resources.Load<Sprite>($"Image/{imageFileName.Trim()}");
                question.image = image;
            } 
            else if(videoFileName != "")
            {
                VideoClip clip = Resources.Load<VideoClip>($"Video/{videoFileName.Trim()}");
                question.videoClip = clip;
            } else if(audioFileName != "")
            {
                AudioClip clip = Resources.Load<AudioClip>($"Audio/{audioFileName.Trim()}");
                question.audioClip = clip;
            }

            //Check Memory Game Index
            int memoryGameIndex = 9;
            string memoryGameIndexString = questionElements[memoryGameIndex];

            if (memoryGameIndexString != "" && int.TryParse(memoryGameIndexString, out int parsedInt))
            {
                question.memoryGameIndex = parsedInt;
            }

            questions.Add(question);
        }
    }

    private void AddAdditionalScore()
    {
        //When timer is less than 10, scoreMultiplier would be 0
        //When it is less than 20, scoreMultiplier would be 1 and so on
        int scoreMultiplier = Mathf.RoundToInt(timer) / 10;

        //Increase score multiplier to account for the value of 0
        scoreMultiplier++;

        int additionalScore = scoreMultiplier * additionalScorePer10Seconds;
        score += additionalScore;
    }


    private void UpdateQuestionUI()
    {
        if(currentQuestionIndex >= questions.Count)
        {
            AddAdditionalScore();
            GameOver();
            return;
        }

        Question currentQuestion = questions[currentQuestionIndex];

        questionText.text = currentQuestion.question;

        for(int i = 0; i < optionButtons.Length; i++)
        {
            if(i >= optionButtons.Length)
            {
                break;
            }

            optionButtons[i].GetComponentInChildren<TMP_Text>().text = currentQuestion.options[i];
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].interactable = true;

            UpdateMediaButton(currentQuestion);

            Button optionButton = optionButtons[i];
            string optionText = currentQuestion.options[i];

            //Run the following function when button is clicked
            optionButtons[i].onClick.AddListener(delegate
            {
                OnOptionClicked(optionButton, optionText);
            });

            SetButtonColor(optionButtons[i], neutralColor);
        }

        if(currentQuestion.memoryGameIndex >= 0)
        {
            ShowMemoryGame();
        }
    }


    private void UpdateMediaButton(Question question)
    {
        //Remove all old functions assigned to the button click
        mediaButton.onClick.RemoveAllListeners();
        mediaButton.gameObject.SetActive(true);
        mediaButton.interactable = true;

        if(question.image != null)
        {
            //Set the function to show image when media button is clicked
            mediaButton.onClick.AddListener(delegate { ImageManager.instance.SetImage(question.image); ImageManager.instance.ShowUI(true); });
            mediaButtonText.text = "Zeige Bild";
        } else if(question.videoClip != null)
        {
            //Set the function to show video when media button is clicked
            mediaButton.onClick.AddListener(delegate { VideoPlayerManager.instance.SetVideoClip(question.videoClip); VideoPlayerManager.instance.ShowUI(true); });
            mediaButtonText.text = "Zeige Video";
        } 
        else if(question.audioClip != null)
        {
            //Set the function to play audio when media button is clicked
            mediaButton.onClick.AddListener(delegate { PlayAudio(question.audioClip); });
            mediaButtonText.text = "Spiele Audio";
        }
        else
        {
            //Hide Media button if question isn't a media type question
            mediaButton.gameObject.SetActive(false);
        }
    }

    public void PlayAudio(AudioClip audioClip)
    {
        StopAudioSource();

        audioSource = new GameObject(audioClip.name, typeof(AudioSource)).GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.loop = false;
        audioSource.Play();
    }

    public void StopAudioSource()
    {
        if (audioSource != null)
        {
            Destroy(audioSource.gameObject);
        }
    }


    private void UpdateProgressBar()
    {
        progressBar.SetMaxValue(questions.Count);
        progressBar.SetValue(currentQuestionIndex);
    }

    private void UpdateScoreText()
    {
        scoreText.text = $"Score: {score}";
    }

    //Runs when an option is clicked
    private void OnOptionClicked(Button optionButton, string optionText)
    {
        DisableAllOptions();
        StopAudioSource();

        Question currentQuestion = questions[currentQuestionIndex];
        if (currentQuestion.IsAnswerCorrect(optionText))
        {
            //Correct Answer
            //Add Streak points if streak is active or add usual points
            score += isStreakActive ? pointsOnStreak : pointsPerCorrectAnswer;
            SetButtonColor(optionButton, correctColor);
            continuousCorrectAnswers++;
            CheckContinuousCorrectAnswers();
        }
        else
        {
            //Wrong Answer
            score -= negativePointsPerWrongAnswer;
            isStreakActive = false;
            SetButtonColor(optionButton, wrongColor);
            DecrementLives();
            UpdateStreakUI();
            wrongAnswerCount++;
            continuousCorrectAnswers = 0;
        }

        //Wait and Show the next question
        StartCoroutine(ShowNextQuestionCoroutine());
    }

    private void CheckContinuousCorrectAnswers()
    {
        //Play the Thrice Effect if number of continuous correct answers is greather than the decided value
        if (continuousCorrectAnswers >= showEffectAfterCorrectAnswers)
        {
            isStreakActive = true;
            UpdateStreakUI();
            continuousCorrectAnswers = 0;
            AudioManager.instance.PlayAudioClip(AudioManager.instance.thriceEffectSound);
            thriceEffectAnim.SetTrigger("thriceAnimationTrigger");
        }
    }


    public void OnMemoryGameCompleted()
    {
        score += pointsPerMemoryGame;
        continuousCorrectAnswers++;
        CheckContinuousCorrectAnswers();

        //Wait and Show the next question
        StartCoroutine(ShowNextQuestionCoroutine());
    }


    public void DecrementLives()
    {
        remainingLives--;
        UpdateLivesText();

        if(remainingLives <= 0)
        {
            GameOver("Keine Leben mehr!");
        }
    }

    public void DisableAllOptions()
    {
        foreach(Button optionButton in optionButtons)
        {
            optionButton.interactable = false;
        }

        mediaButton.interactable = false;
    }

    IEnumerator ShowNextQuestionCoroutine()
    {
        //Stop timer until the next question is shown
        isTimerRunning = false;
        yield return new WaitForSeconds(waitTimeBeforeNextQuestion);
        HideMemoryGame();
        ShowNextQuestion();
        isTimerRunning = true;
    }

    private void ShowNextQuestion()
    {
        //If game is already over, no need to show next question, so skip running the code below
        if (isGameOver)
            return;

        currentQuestionIndex++;
        UpdateProgressBar();
        UpdateScoreText();
        UpdateQuestionUI();
    }

    private void SetButtonColor(Button button, Color color)
    {
        button.GetComponent<Image>().color = color;
    }

    private void GameOver(string gameOverText="Quiz beendet!")
    {
        isGameOver = true;

        HideMemoryGame();

        gameOverTitleText.text = gameOverText;
        int highScore = PlayerPrefs.GetInt(HIGH_SCORE, 0);

        //New High Score
        if(score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HIGH_SCORE, highScore);
        }

        gameOverScoreText.text = $"Dein Score: {score}";
        highScoreText.text = $"High Score: {highScore}";

        gameOverUI.SetActive(true);
    }

    public void ShowMemoryGame()
    {
        if(SceneManager.loadedSceneCount <= 1)
            SceneManager.LoadSceneAsync(MemoryGameManager.MEMORY_GAME_SCENE, LoadSceneMode.Additive);

        optionsContainer.gameObject.SetActive(false);
    }

    public void HideMemoryGame()
    {
        if (SceneManager.loadedSceneCount >= 2)
            SceneManager.UnloadSceneAsync(MemoryGameManager.MEMORY_GAME_SCENE, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        optionsContainer.gameObject.SetActive(true);
    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene(MainMenuManager.MAIN_MENU_SCENE_INDEX);
    }

    public void ReplayButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

