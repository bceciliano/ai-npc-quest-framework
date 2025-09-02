using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestSystemManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Interactable interactableItem;

    //List of all available quests for a given NPC
    private List<QuestData> quests;

    //Tracking active and previous quests
    private int currentQuestIndex = 0;
    private QuestData previousQuest; 
    private QuestData activeQuest;
    private Dictionary<int, int> npcQuestIndices = new();
    private List<QuestItem> collectedQuestItems = new();

    //Reference to Scripts
    private UIManager uiManager;
    private DatabaseManager databaseManager;

    void Start()
    {
        uiManager = GetComponent<UIManager>();
        databaseManager = GetComponent<DatabaseManager>();
        activeQuest = null;
        previousQuest = null;
    }
    public void InitializeAllAvailableQuests(int currentNPCID)
    {
        //Check if the NPCID is in the dictionary 
        if(!npcQuestIndices.ContainsKey(currentNPCID))
        {
            //if it doesn't, initialize it to 0 
            npcQuestIndices[currentNPCID] = 0;

        }
        //Get the available quests from the database 
        quests = databaseManager.GetQuestsForNPC(currentNPCID);
        currentQuestIndex = npcQuestIndices[currentNPCID];
    }

    public bool IsPreviousQuestComplete()
    {
        return previousQuest != null && previousQuest.isCompleted;
    }

    //Retrieve the next quest in all of the available quests for the NPC
    public QuestData GetNextQuest(int currentNPCID)
    {
        if (activeQuest == null)
        {
            //If there is no active quest then we are starting a new interaction 
            if (currentQuestIndex < quests.Count)
            {
                QuestData firstQuest = quests[currentQuestIndex];
                SetActiveQuest(firstQuest);
                npcQuestIndices[currentNPCID] = currentQuestIndex;
                return firstQuest;
            }
        }
        else if (activeQuest.isCompleted)
        {
            //Update database 
            databaseManager.SetQuestCompleted(activeQuest.questID, currentNPCID);

            //Move to the next quest
            currentQuestIndex++; 
            if(currentQuestIndex < quests.Count)
            {
                QuestData nextQuest = quests[currentQuestIndex];
                SetPreviousQuest(activeQuest);
                SetActiveQuest(nextQuest);
                npcQuestIndices[currentNPCID] = currentQuestIndex;
                return nextQuest;
            }
        }    
        return null;
    }
    private void SetActiveQuest(QuestData quest)
    {
        activeQuest = quest;
    }

    private void SetPreviousQuest(QuestData quest)
    {
        previousQuest = quest;
    }
    public void CompleteObjective(QuestItem questItem)
    {
        if (activeQuest != null)
        {
            foreach (QuestObjective objective in activeQuest.objectives)
            {
                if (objective.objectiveID == questItem.itemID && !objective.isCompleted)
                {
                    //Mark the objective as completed
                    objective.isCompleted = true;

                    //Update the UI to reflect the objective completion
                    uiManager.DisplayInventoryUI(questItem);

                    //Update the quest ui 
                    uiManager.DisplayQuestUI(activeQuest);

                    //Check if the entire quest is completed
                    CheckQuestCompletion();
                    return;
                }
            }
        }
    }

    private void CheckQuestCompletion()
    {
        if(activeQuest != null)
        {
            bool allObjectivesCompleted = true;

            foreach(QuestObjective objective in activeQuest.objectives)
            {
                if(!objective.isCompleted)
                {
                    allObjectivesCompleted = false;
                    break;
                }
            }
            if(allObjectivesCompleted)
            {
                //Mark the quest as completed 
                activeQuest.isCompleted = true;
            }
        }
    }

    public void RemoveCompletedUIandInventory()
    {
        //Remove the objective UI
        uiManager.DeleteCompletedObjective(previousQuest);

        foreach(QuestItem questItem in collectedQuestItems)
        {
            //Remove the UI
            uiManager.RemoveInventoryItem(questItem);

        }

        //Clear the list 
        collectedQuestItems.Clear();

    }

    public void AddQuestItem(QuestItem item)
    {
        if(item != null)
        {
            //Add the wuest item to the list of collected items
            collectedQuestItems.Add(item);
        }
    }
    public void TriggerQuestRewards() 
    {
        //Remove quest-related UI 
        RemoveCompletedUIandInventory();

        //Set XP for player 
        foreach (QuestReward reward in previousQuest.rewards)
        {
            uiManager.SetXPText(reward.experiencePoints);
        }    
        
    }
}
