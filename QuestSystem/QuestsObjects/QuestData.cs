using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName ="New Quest", menuName = "Quest System/Quest Data")]
public class QuestData : ScriptableObject
{
    public int questID; 
    public string questName;
    public string questDescription;
    public bool isCompleted;
    public List<QuestObjective> objectives;
    public List<QuestReward> rewards;

}
