using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour {

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

    public GameObject prefabSprite;
    public GameObject prefabCard;

    [Header("__________________")]
    public PT_XMLReader xmlr;

    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;

    public void InitDeck(string deckXMLText)
    {
        if(GameObject.Find("_Deck") == null)
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }

        dictSuits = new Dictionary<string, Sprite>()
        {
            { "C", suitClub },
            { "D", suitDiamond },
            { "H", suitHeart },
            { "S", suitSpade },
        };

        ReadDeck(deckXMLText);
        MakeCards();
    }

    public void ReadDeck(string deckXMLText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(deckXMLText);

        string s = "xml[0] decorator[0]";

        s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
        s += " x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        s += " y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        s += " scale=" + xmlr.xml["xml"][0]["decorator"][0].att("scale");

        decorators = new List<Decorator>();

        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];

        Decorator deco;

        for(int i = 0; i < xDecos.Count; i++)
        {
            deco = new Decorator();

            deco.type = xDecos[i].att("type");
            deco.flip = (xDecos[i].att("flip") == "1");
            deco.scale = float.Parse(xDecos[i].att("scale"));

            deco.loc.x = float.Parse(xDecos[i].att("x"));
            deco.loc.y = float.Parse(xDecos[i].att("y"));
            deco.loc.z = float.Parse(xDecos[i].att("z"));

            decorators.Add(deco);
        }

        cardDefs = new List<CardDefinition>();

        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];

        for(int i = 0; i < xCardDefs.Count; i++)
        {
            CardDefinition cDef = new CardDefinition();

            cDef.rank = int.Parse(xCardDefs[i].att("rank"));

            PT_XMLHashList xPips = xCardDefs[i]["pip"];

            if(xPips != null)
            {
                for(int j = 0; j < xPips.Count; j++)
                {
                    deco = new Decorator();

                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");

                    deco.loc.x = float.Parse(xPips[j].att("x"));
                    deco.loc.y = float.Parse(xPips[j].att("y"));
                    deco.loc.z = float.Parse(xPips[j].att("z"));

                    if(xPips[j].HasAtt("scale"))
                    {
                        deco.scale = float.Parse(xPips[j].att("scale"));
                    }

                    cDef.pips.Add(deco);
                }
            }

            if(xCardDefs[i].HasAtt("face"))
            {
                cDef.face = xCardDefs[i].att("face");
            }

            cardDefs.Add(cDef);
        }
    }

    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        foreach(CardDefinition  cd in cardDefs)
        {
            if(cd.rank == rnk)
            {
                return cd;
            }
        }

        return null;
    }

    public void MakeCards()
    {
        cardNames = new List<string>();

        string[] letters = new string[] { "C", "D", "H", "S" };

        foreach(string s in letters)
        {
            for(int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }

        cards = new List<Card>();
        Sprite tS = null;
        GameObject tGo = null;
        SpriteRenderer tSR = null;

        for (int i = 0; i < cardNames.Count; i++)
        {
            GameObject cgo = Instantiate(prefabCard) as GameObject;

            cgo.transform.parent = deckAnchor;

            Card card = cgo.GetComponent<Card>();

            cgo.transform.localPosition = new Vector3((i % 13) * 3, i / 13 * 4, 0);

            card.name = cardNames[i];
            card.suit = card.name[0].ToString();
            card.rank = int.Parse(card.name.Substring(1));

            if (card.suit == "D" || card.suit == "H")
            {
                card.colS = "Red";
                card.color = Color.red;
            }

            card.def = GetCardDefinitionByRank(card.rank);

            foreach(Decorator deco in decorators)
            {
                if(deco.type == "suit")
                {
                    tGo = Instantiate(prefabSprite) as GameObject;
                    tSR = tGo.GetComponent<SpriteRenderer>();
                    tSR.sprite = dictSuits[card.suit];
                } 
                else
                {
                    tGo = Instantiate(prefabSprite) as GameObject;
                    tSR = tGo.GetComponent<SpriteRenderer>();
                    tS = rankSprites[card.rank];

                    tSR.sprite = tS;
                    tSR.color = card.color;
                }

                tSR.sortingOrder = 1;
                tGo.transform.parent = cgo.transform;
                tGo.transform.localPosition = deco.loc;

                if(deco.flip)
                {
                    tGo.transform.rotation = Quaternion.Euler(0, 0, 180);
                }

                if (deco.scale != 1)
                {
                    tGo.transform.localScale = Vector3.one * deco.scale;
                }

                tGo.name = deco.type;
                card.decoGos.Add(tGo);
            }

            // pips //

            foreach(Decorator pip in card.def.pips)
            {
                tGo = Instantiate(prefabSprite) as GameObject;

                tGo.transform.parent = cgo.transform;
                tGo.transform.localPosition = pip.loc;

                if(pip.flip)
                {
                    tGo.transform.rotation = Quaternion.Euler(0, 0, 180);
                }

                if(pip.scale != 1)
                {
                    tGo.transform.localScale = Vector3.one * pip.scale;
                }

                tGo.name = "pip";

                tSR = tGo.GetComponent<SpriteRenderer>();

                tSR.sprite = dictSuits[card.suit];
                tSR.sortingOrder = 1;

                card.pipGos.Add(tGo);
            }

            if(card.def.face != "")
            {
                tGo = Instantiate(prefabSprite) as GameObject;
                tSR = tGo.GetComponent<SpriteRenderer>();
                tS = GetFace(card.def.face + card.suit);

                tSR.sprite = tS;
                tSR.sortingOrder = 1;
                tGo.transform.parent = card.transform;
                tGo.transform.localPosition = Vector3.zero;
                tGo.name = "face";
            }

            tGo = Instantiate(prefabSprite) as GameObject;
            tSR = tGo.GetComponent<SpriteRenderer>();
            tSR.sprite = cardBack;

            tGo.transform.parent = card.transform;
            tGo.transform.localPosition = Vector3.zero;

            tSR.sortingOrder = 2;
            tGo.name = "back";

            card.back = tGo;

            card.faceUp = false;

            cards.Add(card);
        }
    }

    public Sprite GetFace(string faceS)
    {
        foreach(Sprite tS in faceSprites)
        {
            if(tS.name == faceS)
            {
                return tS;
            }
        }

        return null;
    }

    public static void Shuffle(ref List<Card> oCards)
    {
        List<Card> tCards = new List<Card>();

        int ndx;
        tCards = new List<Card>();

        while(oCards.Count > 0)
        {
            ndx = Random.Range(0, oCards.Count);
            tCards.Add(oCards[ndx]);
            oCards.RemoveAt(ndx);
        }

        oCards = tCards;
    }
}
