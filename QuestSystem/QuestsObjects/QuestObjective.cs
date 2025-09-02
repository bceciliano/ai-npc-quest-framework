using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest Objectives")]

public class QuestObjective : ScriptableObject
{
    public int objectiveID;
    public string objectiveTitle;
    public string description;
    public bool isCompleted;
}
