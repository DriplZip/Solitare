using System;
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

    public SpriteRenderer[] spriteRenderers;

    public bool faceUp
    {
        get { return !back.activeSelf; }
        set { back.SetActive(!value); }
    }

    public virtual void OnMouseUpAsButton()
    {
        print(name);
    }

    public void SetSortingLayerName(string sortingLayerName)
    {
        PopulateSpriteRenderers();

        foreach (SpriteRenderer tSpriteRenderer in spriteRenderers)
        {
            tSpriteRenderer.sortingLayerName = sortingLayerName;
        }
    }

    public void SetSortOrder(int sortingOrder)
    {
        PopulateSpriteRenderers();

        foreach (SpriteRenderer tSpriteRenderer in spriteRenderers)
        {
            if (tSpriteRenderer.gameObject == this.gameObject)
            {
                tSpriteRenderer.sortingOrder = sortingOrder;
                continue;
            }

            switch (tSpriteRenderer.gameObject.name)
            {
                case "back":
                    tSpriteRenderer.sortingOrder = sortingOrder + 2;
                    break;
                case "face":
                default:
                    tSpriteRenderer.sortingOrder = sortingOrder + 1;
                    break;
            }
        }
    }

    private void Start()
    {
        SetSortOrder(0);
    }

    private void PopulateSpriteRenderers()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
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