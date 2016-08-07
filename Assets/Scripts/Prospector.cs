﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public enum ScoreEvent {
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}


public class Prospector : MonoBehaviour {

    public static Prospector S;

    public static int SCORE_FROM_PREV_ROUND = 0;
    public static int HIGH_SCORE = 0;

    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;

    public Deck deck;
    public TextAsset deckXML;

    public Layout layout;
    public TextAsset layoutXML;
    public Vector3 layoutCenter;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Transform layoutAnchor;

    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;

    public List<CardProspector> drawPile;

    void Awake()
    {
        S = this; // Singleton. 

        if(PlayerPrefs.HasKey("ProspectorHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }

        score += SCORE_FROM_PREV_ROUND;

        SCORE_FROM_PREV_ROUND = 0;
    }

    void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);

        drawPile = ConvertListCardsToListCardProspectors(deck.cards);

        LayoutGame();
    }

    CardProspector Draw()
    {
        CardProspector cd = drawPile[0];
        drawPile.RemoveAt(0);
        return cd;
    }

    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach(CardProspector tCP in tableau)
        {
            if(tCP.layoutID == layoutID)
            {
                return tCP;
            }
        }

        return null;
    }

    void LayoutGame()
    {
        if(layoutAnchor == null)
        {
            GameObject tGo = new GameObject("_LayoutAnchor");

            layoutAnchor = tGo.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        CardProspector cp;

        foreach(SlotDef tSD in layout.slotDefs)
        {
            cp = Draw();
            cp.faceUp = tSD.faceUp;
            cp.transform.parent = layoutAnchor;
            cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y, -tSD.layerID);
            cp.layoutID = tSD.id;
            cp.state = CardState.tableau;

            cp.SetSortingLayerName(tSD.layerName);

            tableau.Add(cp);
        }

        foreach(CardProspector tCP in tableau)
        {
            foreach(int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        MoveToTarget(Draw());
        UpdateDrawPile();
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;

        foreach(Card tCD in lCD)
        {
            tCP = tCD as CardProspector;
            lCP.Add(tCP);
        }

        return lCP;
    }

    public void CardClicked(CardProspector cd)
    {
        switch(cd.state)
        {
            case CardState.target:
                break;

            case CardState.drawpile:
                MoveToDiscard(target);
                MoveToTarget(Draw());
                UpdateDrawPile();
                ScoreManager(ScoreEvent.draw);
                break;

            case CardState.tableau:

                bool validMatch = true;

                if(!cd.faceUp)
                {
                    validMatch = false;
                }

                if(!AdjacentRank(cd, target)) 
                {
                    validMatch = false;
                }

                if(!validMatch)
                {
                    return;
                }

                tableau.Remove(cd);
                MoveToTarget(cd);
                SetTableauFaces();
                ScoreManager(ScoreEvent.mine);
                break;
        }

        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        if(tableau.Count == 0)
        {
            GameOver(true); // Win! //
            return;
        }

        if(drawPile.Count > 0)
        {
            return;
        }

        foreach(CardProspector cd in tableau)
        {
            if(AdjacentRank(cd, target))
            {
                return;
            }
        }

        GameOver(false); // lose. //
    }

    void GameOver(bool won)
    {
        if(won)
        {
            ScoreManager(ScoreEvent.gameWin);
        }
        else
        {
            ScoreManager(ScoreEvent.gameLoss);
        }

        SceneManager.LoadScene("Prospector_Scene_0");
    }

    void ScoreManager(ScoreEvent sEvt)
    {
        switch(sEvt)
        {
            case ScoreEvent.draw:
            case ScoreEvent.gameWin:
            case ScoreEvent.gameLoss:

                chain = 0;
                score += scoreRun;
                scoreRun = 0;
                break;

            case ScoreEvent.mine:
                chain++;
                scoreRun += chain;
                break;
        }

        switch(sEvt)
        {
            case ScoreEvent.gameWin:
                Prospector.SCORE_FROM_PREV_ROUND = score;
                print("You won this round! Round score: " + score);
                break;

            case ScoreEvent.gameLoss:

                if (Prospector.HIGH_SCORE <= score)
                {
                    print("You got the High score: " + score);
                    Prospector.HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                else
                {
                    print("Your final score  for the game was: " + score);
                }

                break;

            default:
                print("score: " + score + ", scoreRun: " + ", chain: " + chain);
                break;
        }
    }

    void SetTableauFaces()
    {
        foreach(CardProspector cd in tableau)
        {
            bool fup = true;

            foreach(CardProspector cover in cd.hiddenBy)
            {
                if(cover.state == CardState.tableau)
                {
                    fup = false;
                }
            }

            cd.faceUp = fup;
        }
    }

    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        if (!c0.faceUp || !c1.faceUp)
        {
            return false;
        }

        if(Mathf.Abs(c0.rank - c1.rank) == 1)
        {
            return true;
        }

        if((c0.rank == 1 && c1.rank == 13) || (c0.rank == 13 && c1.rank == 1))
        {
            return true;
        }

        return false;
    }

    void MoveToDiscard(CardProspector cd)
    {
        cd.state = CardState.discard;

        discardPile.Add(cd);

        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + 0.5f);
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    void MoveToTarget(CardProspector cd)
    {
        if(target != null)
        {
            MoveToDiscard(target);
        }

        target = cd;

        cd.state = CardState.target;
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID); // This might be wrong. 
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    void UpdateDrawPile()
    {
        CardProspector cd;

        for(int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;

            Vector2 dpStagger = layout.drawPile.stagger;

            cd.transform.localPosition = new Vector3(layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x), layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y), -layout.drawPile.layerID + 0.1f * i);
            cd.faceUp = false;
            cd.state = CardState.drawpile;
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }
}
