using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Deck : MonoBehaviour
{
    [Header("Set in Inspector")] public bool startFaceUp = false;
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;
    public Sprite[] faceSprites;
    public Sprite[] rankSprites;
    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;
    public GameObject prefabCard;
    public GameObject prefabSprite;

    [FormerlySerializedAs("xmlr")] [Header("Set Dynamically")]
    public PT_XMLReader XMLReader;

    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;

    public CardDefinition GetCardDefinitionByRank(int rank)
    {
        foreach (CardDefinition cardDef in cardDefs)
        {
            if (cardDef.rank == rank) return cardDef;
        }

        return null;
    }

    static public void Shuffle(ref List<Card> otherCards)
    {
        List<Card> tempCards = new List<Card>();
        int idx;
        while (otherCards.Count > 0)
        {
            idx = Random.Range(0, otherCards.Count);
            tempCards.Add(otherCards[idx]);
            otherCards.RemoveAt(idx);
        }

        otherCards = tempCards;
    }

    public void InitDeck(string deckXMLText)
    {
        if (GameObject.Find("_Deck") == null)
        {
            GameObject anchorGameObject = new GameObject("_Deck");
            deckAnchor = anchorGameObject.transform;
        }

        dictSuits = new Dictionary<string, Sprite>()
        {
            {"C", suitClub},
            {"D", suitDiamond},
            {"H", suitHeart},
            {"S", suitSpade}
        };

        ReadDeck(deckXMLText);

        MakeCards();
    }

    public void ReadDeck(string deckXMLText)
    {
        XMLReader = new PT_XMLReader();
        XMLReader.Parse(deckXMLText);

        decorators = new List<Decorator>();
        PT_XMLHashList xDecorators = XMLReader.xml["xml"][0]["decorator"];

        Decorator decorator;
        for (int i = 0; i < xDecorators.Count; i++)
        {
            decorator = new Decorator();

            decorator.type = xDecorators[i].att("type");
            decorator.flip = (xDecorators[i].att("flip") == "1");
            decorator.scale = float.Parse(xDecorators[i].att("scale"));
            decorator.location.x = float.Parse(xDecorators[i].att("x"));
            decorator.location.y = float.Parse(xDecorators[i].att("y"));
            decorator.location.z = float.Parse(xDecorators[i].att("z"));

            decorators.Add(decorator);
        }

        cardDefs = new List<CardDefinition>();

        PT_XMLHashList xCardDefs = XMLReader.xml["xml"][0]["card"];
        for (int i = 0; i < xCardDefs.Count; i++)
        {
            CardDefinition cardDefinition = new CardDefinition();
            cardDefinition.rank = int.Parse(xCardDefs[i].att(("rank")));

            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if (xPips != null)
            {
                for (int j = 0; j < xPips.Count; j++)
                {
                    decorator = new Decorator();
                    decorator.type = "pip";
                    decorator.flip = (xPips[j].att("flip") == "1");
                    decorator.location.x = float.Parse(xPips[j].att("x"));
                    decorator.location.y = float.Parse(xPips[j].att("y"));
                    decorator.location.z = float.Parse(xPips[j].att("z"));

                    if (xPips[j].HasAtt("scale"))
                    {
                        decorator.scale = float.Parse(xPips[j].att("scale"));
                    }

                    cardDefinition.pips.Add(decorator);
                }
            }

            if (xCardDefs[i].HasAtt("face"))
            {
                cardDefinition.face = xCardDefs[i].att("face");
            }

            cardDefs.Add(cardDefinition);
        }
    }

    public void MakeCards()
    {
        cardNames = new List<string>();
        string[] letters = new[] {"C", "D", "H", "S"};

        foreach (string s in letters)
        {
            for (int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }

        cards = new List<Card>();

        for (int i = 0; i < cardNames.Count; i++)
        {
            cards.Add(MakeCard(i));
        }
    }

    private Card MakeCard(int cardNum)
    {
        GameObject cardGameObject = Instantiate(prefabCard, deckAnchor, true);
        Card card = cardGameObject.GetComponent<Card>();

        cardGameObject.transform.localPosition = new Vector3((cardNum % 13) * 3, cardNum / 13 * 4, 0);

        card.name = cardNames[cardNum];
        card.suit = card.name[0].ToString();
        card.rank = int.Parse(card.name.Substring(1));

        if (card.suit == "D" || card.suit == "H")
        {
            card.colorSight = Color.red;
            card.colorName = "Red";
        }

        card.definition = GetCardDefinitionByRank(card.rank);

        AddDecorators(card);
        AddPips(card);
        AddFace(card);
        AddBack(card);

        return card;
    }

    private Sprite _tempSprite = null;

    private GameObject _tempGameObject = null;

    private SpriteRenderer _tempSpriteRenderer = null;

    private void AddDecorators(Card card)
    {
        foreach (Decorator decorator in decorators)
        {
            if (decorator.type == "suit")
            {
                _tempGameObject = Instantiate(prefabSprite);
                _tempSpriteRenderer = _tempGameObject.GetComponent<SpriteRenderer>();
                _tempSpriteRenderer.sprite = dictSuits[card.suit];
            }
            else
            {
                _tempGameObject = Instantiate(prefabSprite);
                _tempSpriteRenderer = _tempGameObject.GetComponent<SpriteRenderer>();
                _tempSprite = rankSprites[card.rank];
                _tempSpriteRenderer.sprite = _tempSprite;
                _tempSpriteRenderer.color = card.colorSight;
            }

            _tempSpriteRenderer.sortingOrder = 1;
            _tempGameObject.transform.SetParent(card.transform);
            _tempGameObject.transform.localPosition = decorator.location;

            if (decorator.flip) _tempGameObject.transform.rotation = Quaternion.Euler(0, 0, 180);

            if (decorator.scale != 1) _tempGameObject.transform.localScale = Vector3.one * decorator.scale;

            _tempGameObject.name = decorator.type;

            card.decoratorGameObjects.Add(_tempGameObject);
        }
    }

    private void AddPips(Card card)
    {
        foreach (Decorator pip in card.definition.pips)
        {
            _tempGameObject = Instantiate(prefabSprite, card.transform, true);
            _tempGameObject.transform.localPosition = pip.location;

            if (pip.flip) _tempGameObject.transform.rotation = Quaternion.Euler(0, 0, 180);
            if (pip.scale != 1) _tempGameObject.transform.localScale = Vector3.one * pip.scale;

            _tempGameObject.name = "pip";
            _tempSpriteRenderer = _tempGameObject.GetComponent<SpriteRenderer>();
            _tempSpriteRenderer.sprite = dictSuits[card.suit];
            _tempSpriteRenderer.sortingOrder = 1;

            card.pipGameObjects.Add(_tempGameObject);
        }
    }

    private void AddFace(Card card)
    {
        if (card.definition.face == "") return;

        _tempGameObject = Instantiate(prefabSprite, card.transform, true);
        _tempSpriteRenderer = _tempGameObject.GetComponent<SpriteRenderer>();
        _tempSprite = GetFace(card.definition.face + card.suit);
        _tempSpriteRenderer.sprite = _tempSprite;
        _tempSpriteRenderer.sortingOrder = 1;
        _tempGameObject.transform.localPosition = Vector3.zero;
        _tempGameObject.name = "face";
    }

    private Sprite GetFace(string faceSprite)
    {
        foreach (Sprite sprite in faceSprites)
        {
            if (sprite.name == faceSprite) return sprite;
        }

        return null;
    }

    private void AddBack(Card card)
    {
        _tempGameObject = Instantiate(prefabSprite, card.transform, true);
        _tempSpriteRenderer = _tempGameObject.GetComponent<SpriteRenderer>();
        _tempSpriteRenderer.sprite = cardBack;
        _tempGameObject.transform.localPosition = Vector3.zero;
        _tempSpriteRenderer.sortingOrder = 2;
        _tempGameObject.name = "back";
        card.back = _tempGameObject;

        card.faceUp = startFaceUp;
    }
}