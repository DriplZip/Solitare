using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Set in Inspector")] 
    public TextAsset deckXML;
    public TextAsset layoutXML;

    [Header("Set Dynamically")] 
    public Deck deck;
    public Layout layout;

    void Awake()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
        S = this;
    }

    void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);
    }
}