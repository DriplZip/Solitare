using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotDefinition
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerId = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}

public class Layout : MonoBehaviour
{
    public PT_XMLReader XMLReader;
    public PT_XMLHashtable XMLHashtable;
    public Vector2 multiplier;

    public List<SlotDefinition> slotDefinitions;
    public SlotDefinition drawPile;
    public SlotDefinition discardPile;
    public string[] sortingLayersNames = new[] {"Row0", "Row1", "Row2", "Row3", "Discard", "Draw"};

    public void ReadLayout(string xmlText)
    {
        XMLReader = new PT_XMLReader();
        XMLReader.Parse(xmlText);
        XMLHashtable = XMLReader.xml["xml"][0];

        multiplier.x = float.Parse(XMLHashtable["multiplier"][0].att("x"));
        multiplier.y = float.Parse(XMLHashtable["multiplier"][0].att("y"));

        SlotDefinition tempSlotDefinition;
        PT_XMLHashList slotsX = XMLHashtable["slot"];

        for (int i = 0; i < slotsX.Count; i++)
        {
            tempSlotDefinition = new SlotDefinition();
            if (slotsX[i].HasAtt("type"))
            {
                tempSlotDefinition.type = slotsX[i].att("type");
            }
            else
            {
                tempSlotDefinition.type = "slot";
            }

            tempSlotDefinition.x = float.Parse(slotsX[i].att("x"));
            tempSlotDefinition.y = float.Parse(slotsX[i].att("y"));
            tempSlotDefinition.layerId = int.Parse(slotsX[i].att("layer"));
            tempSlotDefinition.layerName = sortingLayersNames[tempSlotDefinition.layerId];

            switch (tempSlotDefinition.type)
            {
                case "slot":
                    tempSlotDefinition.faceUp = (slotsX[i].att("faceup") == "1");
                    tempSlotDefinition.id = int.Parse(slotsX[i].att("id"));

                    if (slotsX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach (string s in hiding)
                        {
                            tempSlotDefinition.hiddenBy.Add(int.Parse(s));
                        }
                    }

                    slotDefinitions.Add(tempSlotDefinition);
                    break;
                case "drawpile":
                    tempSlotDefinition.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tempSlotDefinition;
                    break;
                case "discardpile":
                    discardPile = tempSlotDefinition;
                    break;
            }
        }
    }
}