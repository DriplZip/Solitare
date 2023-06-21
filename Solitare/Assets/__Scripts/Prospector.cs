using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Set in Inspector")] public TextAsset deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;
    public Vector2 fsPosMid = new Vector2(0.5f, 0.9f);
    public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
    public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);

    [Header("Set Dynamically")] public Deck deck;
    public Layout layout;
    public List<CardProspector> drawPile;
    public Transform layoutAnchor;
    public CardProspector target;
    public List<CardProspector> table;
    public List<CardProspector> discardPile;
    public FloatingScore fsRun;

    public void CardClicked(CardProspector cardProspector)
    {
        switch (cardProspector.state)
        {
            case eCardState.target:
                break;

            case eCardState.drawpile:
                MoveToDiscard(target);
                MoveToTarget(Draw());
                UpdateDrawPile();

                ScoreManager.EVENT(eScoreEvent.draw);
                FloatingScoreHandler(eScoreEvent.draw);

                break;

            case eCardState.table:
                bool validMatch = cardProspector.faceUp;
                if (!AdjacentRank(cardProspector, target)) validMatch = false;
                if (!validMatch) return;
                table.Remove(cardProspector);
                MoveToTarget(cardProspector);
                SetTableFaces();

                ScoreManager.EVENT(eScoreEvent.mine);
                FloatingScoreHandler(eScoreEvent.mine);

                break;
        }

        CheckForGameOver();
    }

    private void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPoints;

        switch (evt)
        {
            case eScoreEvent.draw:
            case eScoreEvent.gameWin:
            case eScoreEvent.gameLoss:
                if (fsRun != null)
                {
                    fsPoints = new List<Vector2>();
                    fsPoints.Add(fsPosRun);
                    fsPoints.Add(fsPosMid2);
                    fsPoints.Add(fsPosEnd);

                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPoints, 0, 1);
                    fsRun.fontSizes = new List<float>(new float[] {28, 36, 4});
                    fsRun = null;
                }
                break;
            
            case eScoreEvent.mine:
                FloatingScore floatingScore;
                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;

                fsPoints = new List<Vector2>();
                fsPoints.Add(p0);
                fsPoints.Add(fsPosMid);
                fsPoints.Add(fsPosRun);

                floatingScore = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPoints);
                floatingScore.fontSizes = new List<float>(new float[] {4, 50, 28});

                if (fsRun == null)
                {
                    fsRun = floatingScore;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    floatingScore.reportFinishTo = fsRun.gameObject;
                }
                break;
        }
    }

    private void CheckForGameOver()
    {
        if (table.Count == 0)
        {
            GameOver(true);
            return;
        }

        if (drawPile.Count > 0)
        {
            return;
        }

        foreach (CardProspector cardProspector in table)
        {
            if (AdjacentRank(cardProspector, target))
            {
                return;
            }
        }

        GameOver(false);
    }

    private void GameOver(bool win)
    {
        if (win)
        {
            ScoreManager.EVENT(eScoreEvent.gameWin);
            FloatingScoreHandler(eScoreEvent.gameWin);
        }
        else
        {
            ScoreManager.EVENT(eScoreEvent.gameLoss);
            FloatingScoreHandler(eScoreEvent.gameLoss);
        }

        SceneManager.LoadScene("__Prospector_Scene_0");
    }

    private void SetTableFaces()
    {
        foreach (CardProspector cardProspector in table)
        {
            bool faceUp = true;

            foreach (CardProspector cover in cardProspector.hiddenBy)
            {
                if (cover.state == eCardState.table) faceUp = false;
            }

            cardProspector.faceUp = faceUp;
        }
    }

    private bool AdjacentRank(CardProspector card0, CardProspector card1)
    {
        if (!card0.faceUp || !card1.faceUp) return false;
        if (Mathf.Abs(card0.rank - card1.rank) == 1) return true;
        if (card0.rank == 1 && card1.rank == 13) return true;
        if (card0.rank == 13 && card1.rank == 1) return true;

        return false;
    }

    void Awake()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
        S = this;
    }

    void Start()
    {
        Scoreboard.S.score = ScoreManager.SCORE;

        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);
        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
    }

    private CardProspector Draw()
    {
        CardProspector cardProspector = drawPile[0];
        drawPile.RemoveAt(0);
        return cardProspector;
    }

    private void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tempGameObject = new GameObject("_LayoutAnchor");
            layoutAnchor = tempGameObject.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        CardProspector cardProspector;

        foreach (SlotDefinition slotDefinition in layout.slotDefinitions)
        {
            cardProspector = Draw();
            cardProspector.faceUp = slotDefinition.faceUp;
            cardProspector.transform.parent = layoutAnchor;
            cardProspector.transform.localPosition = new Vector3(layout.multiplier.x * slotDefinition.x,
                layout.multiplier.y * slotDefinition.y, -slotDefinition.layerId);
            cardProspector.layoutId = slotDefinition.id;
            cardProspector.slotDefinition = slotDefinition;
            cardProspector.state = eCardState.table;
            cardProspector.SetSortingLayerName(slotDefinition.layerName);
            table.Add(cardProspector);
        }

        foreach (CardProspector tempCardProspector in table)
        {
            foreach (int hidden in tempCardProspector.slotDefinition.hiddenBy)
            {
                cardProspector = FindCardByLayoutId(hidden);
                tempCardProspector.hiddenBy.Add(cardProspector);
            }
        }

        MoveToTarget(Draw());
        UpdateDrawPile();
    }

    private CardProspector FindCardByLayoutId(int layoutId)
    {
        foreach (CardProspector tempCardProspector in table)
        {
            if (tempCardProspector.layoutId == layoutId) return tempCardProspector;
        }

        return null;
    }

    private List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> cards)
    {
        List<CardProspector> listCardProspectors = new List<CardProspector>();
        //CardProspector cardProspector;

        foreach (Card tempCard in cards)
        {
            listCardProspectors.Add((CardProspector) tempCard);
        }

        return listCardProspectors;
    }

    private void MoveToDiscard(CardProspector cardProspector)
    {
        cardProspector.state = eCardState.discard;
        discardPile.Add(cardProspector);
        cardProspector.transform.parent = layoutAnchor;

        cardProspector.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerId + 0.5f);
        cardProspector.faceUp = true;
        cardProspector.SetSortingLayerName(layout.discardPile.layerName);
        cardProspector.SetSortOrder(-100 + discardPile.Count);
    }

    private void MoveToTarget(CardProspector cardProspector)
    {
        if (target != null) MoveToDiscard(target);
        target = cardProspector;

        cardProspector.state = eCardState.target;
        cardProspector.transform.parent = layoutAnchor;
        cardProspector.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerId);
        cardProspector.faceUp = true;
        cardProspector.SetSortingLayerName(layout.discardPile.layerName);
        cardProspector.SetSortOrder(0);
    }

    private void UpdateDrawPile()
    {
        CardProspector cardProspector;
        for (int i = 0; i < drawPile.Count; i++)
        {
            cardProspector = drawPile[i];
            cardProspector.transform.parent = layoutAnchor;

            Vector2 drawPileStagger = layout.drawPile.stagger;
            cardProspector.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * drawPileStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * drawPileStagger.y), -layout.drawPile.layerId + 0.1f * i);
            cardProspector.faceUp = false;
            cardProspector.state = eCardState.drawpile;
            cardProspector.SetSortingLayerName(layout.drawPile.layerName);
            cardProspector.SetSortOrder(-10 * i);
        }
    }
}