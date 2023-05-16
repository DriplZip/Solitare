using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCardState
{
    drawpile,
    table,
    target,
    discard
}

public class CardProspector : Card
{
    [Header("Set Dynamically: CardProspector")]
    public eCardState state = eCardState.drawpile;
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    public int layoutId;
    public SlotDefinition slotDefinition;
}
