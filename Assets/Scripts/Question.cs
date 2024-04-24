using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Question
{
    public string question;
    public string category;
    public string[] options;
    public string correctAnswer;
    public int memoryGameIndex = -1;
    public Sprite image;
    public VideoClip videoClip;
    public AudioClip audioClip;

    //Compares the chosen answer to the correct answer
    public bool IsAnswerCorrect(string chosenAnswer)
    {
        return chosenAnswer.Trim().Equals(correctAnswer.Trim());
    }
}
