using UnityEngine;

[CreateAssetMenu (fileName = "New Item", menuName = "Quest System/Create New Item")]
public class QuestItem : ScriptableObject
{
    public int itemID;
    public string itemName;
    public Sprite icon;
}


