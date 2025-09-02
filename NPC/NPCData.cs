using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Data", menuName ="NPC Data")]
public class NPCData : ScriptableObject
{
    public int id;
    public string npcName;
    public string species;
    public string characteristics;
    public string[] introductions;

    public List<QuestData> availableQuests = new();

    public void InitializeQuests(List<QuestData> questData)
    {
        availableQuests  = questData;
    }
}
