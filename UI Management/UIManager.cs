using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class UIManager : MonoBehaviour
{
    //Dialogue UI elements
    public TMP_InputField inputField;
    public GameObject dialoguePanel;
    public TMP_Text xpText;

    //Dialogue lines functionality
    private APIManager apiManager;
    private int currentNPCID;

    //Reference to scripts 
    private QuestMenuController questMenuController;
    private DialogueUIController dialogueUIController;
    private InventoryController inventoryController;

    void Start()
    {
        //Hide the interaction prompt and canvas at the start
        HideCanvas();
        DisableInputField();
        apiManager = GetComponent<APIManager>();
        questMenuController = GetComponent<QuestMenuController>();
        dialogueUIController = GetComponent<DialogueUIController>();
        inventoryController = GetComponent<InventoryController>();  

    }

    public void Update()
    {
        //Exit interaction using the Escape Key 
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscape();
        }
    }
    public void SetNPCID(int npcID)
    {
        currentNPCID = npcID;
    }
    public void InitializeUIElements(params GameObject[] uiElements)
    {
        foreach (GameObject uiElement in uiElements)
        {
            uiElement.SetActive(false);
        }
    }

    public void ChangeColourToGray(Image imageComponent)
    {
        //Define a color for grayed out objectives and change image color
        Color grayColor = new(0.5f, 0.5f, 0.5f, 0.5f);
        imageComponent.color = grayColor;
    }

    public void ChangeColourBack(Image imageComponent, Color originalColor)
    {
        imageComponent.color = originalColor;
    }

    public void SetDialogueText(List<string> lines)
    {
        dialogueUIController.SetPanelText(lines);
    }

    public void DisplayQuestUI(QuestData quest)
    {
        questMenuController.DisplayQuestUI(quest);
    }

    public void DisplayInventoryUI(QuestItem questItem)
    {
        inventoryController.DisplayInventoryItem(questItem);
    }

    public void DeleteCompletedObjective(QuestData questCompleted)
    {
        questMenuController.DeleteObjectives(questCompleted);
    }
    public void SetChatMessage()
    {
        dialogueUIController.SetChatMessage(apiManager, currentNPCID);
    }

    public void RemoveInventoryItem(QuestItem questItem)
    {
        inventoryController.RemoveItemFromInventory(questItem);
    }

    public void SetXPText(int xp)
    {
        xpText.text = xp.ToString() + " XP";
    }
    public void HandleEscape()
    {
        HideCanvas();
        dialogueUIController.CompleteTyping();

        //Allow the player to move again
        ThirdPersonMovement.isInDialogue = false;

        PlayerInputController.isTyping = false;
    }

    public void EnableInputField()
    {
        if (inputField != null)
        {
            inputField.interactable = true;
            inputField.Select();
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    public void DisableInputField()
    {
        if (inputField != null)
        {
            inputField.interactable = false;
        }
    }
    public void ShowCanvas()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
    }

    public void HideCanvas()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void ShowPrompt(GameObject prompt)
    {
        if (prompt != null)
        {
            prompt.SetActive(true);
        }
    }

    public void HidePrompt(GameObject prompt)
    {
        if (prompt != null)
        {
            prompt.SetActive(false);
        }
    }
}
