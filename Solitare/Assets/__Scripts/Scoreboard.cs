using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public static Scoreboard S;

    [Header("Set in Inspector")] public GameObject prefabFloatingScore;

    [Header("Set Dynamically")] 
    [SerializeField] private int _score = 0;
    [SerializeField] private string _scoreString;
    
    public int score
    {
        get => (_score);
        set
        {
            _score = value;
            _scoreString = _score.ToString("N0");
            GetComponent<Text>().text = _scoreString;
        }
    }
    public string scoreString
    {
        get => (_scoreString);
        set
        {
            _scoreString = value;
            GetComponent<Text>().text = _scoreString;
        }
    }

    private Transform canvasTransform;

    private void Awake()
    {
        if (S == null)
            S = this;
        else
            Debug.LogError("ERROR: Scoreboard.Awake(): S is already set");

        canvasTransform = transform.parent;
    }

    public void FSCallback(FloatingScore floatingScore)
    {
        score += floatingScore.score;
    }

    public FloatingScore CreateFloatingScore(int scoreSum, List<Vector2> points)
    {
        GameObject go = Instantiate(prefabFloatingScore, canvasTransform, true);
        FloatingScore floatingScore = go.GetComponent<FloatingScore>();
        floatingScore.score = scoreSum;
        floatingScore.reportFinishTo = this.gameObject;
        floatingScore.Init(points);

        return floatingScore;
    }
}