using UnityEngine;

public class ItemData
{
    // The relic this entry represents. Carried through so a future click
    // handler on the cell (selecting it to open the relic detail popup)
    // can look the relic back up via RelicDatabaseClass.GetById(relicId)
    // without needing to re-derive it from displayed text.
    public string relicId;

    public string title;
    public string description;
    public int index;
}
