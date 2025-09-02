using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest Rewards")]
public class QuestReward : ScriptableObject
{
    public int experiencePoints;
    public string loreDescription;

}
