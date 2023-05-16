using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Set Dynamically")] public string suit;
    public int rank;
    public Color colorSight = Color.black;
    public string colorName = "Black"; // "Red"
    public List<GameObject> decoratorGameObjects = new List<GameObject>();
    public List<GameObject> pipGameObjects = new List<GameObject>();

    public GameObject back;

    public CardDefinition definition;

    public bool faceUp
    {
        get { return !back.activeSelf; }
        set { back.SetActive(!value); }
    }
}

[System.Serializable]
public class Decorator
{
    public string type;
    public Vector3 location;
    public bool flip = false;
    public float scale = 1f;
}

[System.Serializable]
public class CardDefinition
{
    public string face;
    public int rank;
    public List<Decorator> pips = new List<Decorator>();
}