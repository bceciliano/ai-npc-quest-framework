using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCSystem : MonoBehaviour
{ 
    //Reference to scripts
    [Header("References")]
    [SerializeField] private DatabaseManager databaseManager;
    [SerializeField] private PlayerInputController playerInputController;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private QuestSystemManager questSystem;

    //Reference to Press 'E' Prompt
    public GameObject promptManager;

    //Reference to itself
    private NPCData currentNPC;
    private NPCController npcController;

    private bool isFirstInteraction = true;
    private bool playerDetection = false;

    private void Start()
    {
        npcController = GetComponentInParent<NPCController>();
        uiManager.HidePrompt(promptManager);
    }
    void Update()
    {
        //If player is colliding with detection sphere and is not in dialogue
        if(playerDetection && !ThirdPersonMovement.isInDialogue)
        {
            HandleNPCInteraction();
        }
        //The player has exited the player detection sphere 
        else
        {
            //Hide E to Interact Prompt
            uiManager.HidePrompt(promptManager); 
        }
    }

    //Handle NPC interaction when the player presses 'E' in the detection sphere 
    private void HandleNPCInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartNPCInteraction();
        }
        else
        {
            //Display E to Interact Prompt while in sphere
            uiManager.ShowPrompt(promptManager);
        }
    }

    //Starts the interaction with the NPC
    private void StartNPCInteraction()
    { 
        //Lock the player movemenet during the dialogue
        ThirdPersonMovement.isInDialogue = true;

        //Get the current NPC's data
        currentNPC = npcController.npcData;

        //Show dialogue panel and display text if NPC is not null
        if (currentNPC != null)
        {
            //Populate NPC Characteristics, Dialogue History, and Quests
            PopulateNPCData(currentNPC);

            //Initialise all the NPC quests in QuestSystemManager
            questSystem.InitializeAllAvailableQuests(currentNPC.id);

            //Connect NPCs so the appropriate scripts know which NPC is being interacted with 
            ConnectNPCComponents(currentNPC);

            //Get next available quest
            QuestData nextQuest = questSystem.GetNextQuest(currentNPC.id);

            //Handle the first interaction with the NPC
            if (isFirstInteraction)
            {
                ActivateInitialInteraction(nextQuest);
                isFirstInteraction = false;
            }
            else
            {
                //Handle return interactions with the NPC
                ActivateReturnInteraction(nextQuest);   
            }
        }
    }
    
    //Populates NPC data in the database and initializes associated dialogue and quests
    private void PopulateNPCData(NPCData npcData)
    {
        if (npcData != null)
        {
            //Insert NPC information with introductions into database 
            databaseManager.InsertNPC(npcData.id, npcData.npcName, npcData.species, npcData.characteristics);

            foreach (string introduction in npcData.introductions)
            {
                databaseManager.InsertDialogueHistory(npcData.id, introduction);
            }
            InitializeNPCQuests(npcData);
        }
    }

    //Initialize NPC quests associated with the current NPC
    private void InitializeNPCQuests(NPCData npcData)
    {
        List<QuestData> availableQuests = npcData.availableQuests;
        foreach (QuestData quest in availableQuests)
        {
            databaseManager.InsertQuest(
                quest.questID,
                npcData.id,
                quest.questName,
                quest.questDescription,
                quest.isCompleted,
                quest.objectives,
                quest.rewards);
        }

        npcData.InitializeQuests(availableQuests);
    }
    private void ConnectNPCComponents(NPCData currentNPC)
    {
        playerInputController.SetNPCController(npcController);
        uiManager.SetNPCID(currentNPC.id);
    }
  
    private void ActivateInitialInteraction(QuestData firstQuest)
    {
        //Display Intro Dialogue
        uiManager.ShowCanvas();
        uiManager.SetDialogueText(new List<string>(currentNPC.introductions)); 

        //Activate the first quest if its not completed
        if(firstQuest!= null && !firstQuest.isCompleted)
        {
            uiManager.DisplayQuestUI(firstQuest); 
        }
    }

    private void ActivateReturnInteraction(QuestData nextQuest)
    {
        bool activeQuestComplete = questSystem.IsPreviousQuestComplete();
       
        //Check if the current quest is completed and if there's a next quest available
        if (activeQuestComplete && nextQuest != null)
        {
            //Set a welcome back message
            uiManager.ShowCanvas();
            uiManager.SetDialogueText(new List<string> { "Thank you so much" });

            //Trigger quest rewards 
            questSystem.TriggerQuestRewards();
            
            //Display the next quest in the UI and remove previous objective UI
            uiManager.DisplayQuestUI(nextQuest);
        }
        else
        {
            //Set a welcome back message
            uiManager.ShowCanvas();
            uiManager.SetDialogueText(new List<string> { "Welcome Back" });
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerDetection = true;   
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerDetection = false;
            uiManager.HideCanvas();
            uiManager.HidePrompt(promptManager);

        }
    }
}
