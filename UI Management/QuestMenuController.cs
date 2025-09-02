using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestMenuController : MonoBehaviour
{
    [Header("UI Elements and SFX")]
    public GameObject questSidePanel; 
    public Transform questListContent; 
    public GameObject questUIPrefab; 
    public GameObject detailsPanel; 
    public GameObject notificationImage; 
    public TMP_Text notificationCount;
    public AudioSource audioSource;

    //Reference to script that handles camera locking and buttonclicks 
    private MenuManager menuManager; 
    private UIManager uiManager;

    //Notifications Functionality
    private int activeObjectiveCount = 0;
    private bool isFirstNotificationClick = true;
    private bool objectivesUpdated = false;
    private Color originalNotificationColor;
    private Image notificationImageComponent;

    //Dicitionary to store reference to instantiated questUIPrefab objects
    private readonly Dictionary<QuestObjective, GameObject> questUIObjects = new();

    void Start()
    {
        //Get the manager components in the same gameObject
        menuManager = GetComponent<MenuManager>();
        uiManager = GetComponent<UIManager>();

        InitalizeUI();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !PlayerInputController.isTyping)
        {
            ToggleObjectivesMenu();
        }
    }

    private void InitalizeUI()
    {
        //Initialize UI Elements 
        uiManager.InitializeUIElements(questSidePanel, detailsPanel, notificationImage);

        //Get the color and image for the notification functionality 
        originalNotificationColor = notificationImage.GetComponent<Image>().color;
        notificationImageComponent = notificationImage.GetComponent<Image>();
    }
    public void ToggleObjectivesMenu()
    {
        menuManager.ToggleMenu(questSidePanel);

        //Check if its the first click and objectives has been updated on the UI 
        if (isFirstNotificationClick && objectivesUpdated)
        { 
            uiManager.ChangeColourToGray(notificationImageComponent);
            isFirstNotificationClick = false;
        }
    }

    public void ToggleDetailsMenu()
    {
        menuManager.ToggleMenu(detailsPanel);

        //Lock the camera when the more details menu is active 
        menuManager.LockCamera();
    }
    public void DisplayQuestUI(QuestData quest)
    {
        //Play new quest sound
        audioSource.Play();
        UpdateObjectiveUI(quest);
    }

    private void UpdateObjectiveUI(QuestData quest)
    {
        //Objective UI has been updated 
        objectivesUpdated = true;

        if (objectivesUpdated)
        {
            isFirstNotificationClick = true;
        }

        foreach (QuestObjective objective in quest.objectives)
        {
            //Check if a UI Object for this objective already exists
            if (questUIObjects.TryGetValue(objective, out GameObject ExistingQuestUI))
            {
                //The UI object already exists, update instead of instantiating a new one 
                UpdateObjectives(quest, objective, ExistingQuestUI);
            }
            else
            {
                //Instantiate a copy of the questUIPrefab
                GameObject questUI = Instantiate(questUIPrefab, questListContent);

                //Add it to the dictionary 
                questUIObjects.Add(objective, questUI);
                UpdateObjectives(quest, objective, questUI);
                activeObjectiveCount++;

            }
        }
        UpdateNotifiationStatus();
    }
    private void UpdateNotifiationStatus()
    {
        if (activeObjectiveCount == 0)
        {
            notificationImage.SetActive(false);
        }
        else
        {
            notificationImage.SetActive(true);
            notificationCount.text = activeObjectiveCount.ToString();
        }
        uiManager.ChangeColourBack(notificationImageComponent, originalNotificationColor);
    }
    private void UpdateObjectives(QuestData quest, QuestObjective objective, GameObject questUIPrefab)
    {
        //Access the UI elements to set text and isCompletedMark
        TMP_Text titleText = questUIPrefab.transform.Find("Quest Title").GetComponent<TMP_Text>();
        TMP_Text objectiveText = questUIPrefab.transform.Find("Quest Objective").GetComponent<TMP_Text>();
        Image isCompletedMark = questUIPrefab.transform.Find("IsCompletedMark").GetComponent<Image>();
        Button detailsButton = questUIPrefab.GetComponentInChildren<Button>();
        detailsButton.onClick.AddListener(ToggleDetailsMenu);
        Image backgroundImage = questUIPrefab.GetComponentInChildren<Image>();

        //Populate UI with quest information 
        titleText.text = quest.questName;
        objectiveText.text = objective.objectiveTitle;

        //Set objective status(complete or not)
        if (objective.isCompleted)
        {
            activeObjectiveCount--;
            isCompletedMark.enabled = true;
            uiManager.ChangeColourToGray(backgroundImage);
            detailsButton.interactable = false;
        }
        else
        {
            isCompletedMark.enabled = false;
        }
    }
    public void DeleteObjectives(QuestData questCompleted) 
    {
        foreach (var completed in questUIObjects.ToList())
        {
            if(questCompleted.objectives.Contains(completed.Key))
            {
                questUIObjects.Remove(completed.Key);
                Destroy(completed.Value);
            }
        }
    }
}
