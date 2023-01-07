using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreResult : MonoBehaviour
{
    static public int scoreResult = 0;

    void Awake()
    {
        if (PlayerPrefs.HasKey("HighScore"))
        {
            scoreResult = PlayerPrefs.GetInt("HighScore");
        }
        PlayerPrefs.SetInt("HighScore", scoreResult);
    }

    void Update()
    {
        Text gt = this.GetComponent<Text>();
        gt.text = "HighScore: " + scoreResult;

        if (scoreResult > PlayerPrefs.GetInt("HighScore"))
        {
            PlayerPrefs.SetInt("HighScore", scoreResult);
        }
    }
}