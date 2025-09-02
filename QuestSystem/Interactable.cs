using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private QuestSystemManager questSystemManager;

    private bool playerDetection; 
    public GameObject promptManager;
    public QuestItem questItem;

    private void Start()
    {
        uiManager.HidePrompt(promptManager);
    }

    public void Update()
    {
        //Check if the player can collect the objective and 'E' key is pressed 
        if (playerDetection)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                CollectObjective();
            }
        }
        else
        {
            uiManager.HidePrompt(promptManager);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        { 
            //Show the prompt when the player enters the interaction zone 
            playerDetection = true;
            uiManager.ShowPrompt(promptManager);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Hide the prompt 
            playerDetection = false;
        }
    }
    public void CollectObjective()
    {
        if (questItem != null)
        {
            //Call the quest system maanager to handle objective completion 
            questSystemManager.CompleteObjective(questItem);

            //Hide the prompt
            uiManager.HidePrompt(promptManager);

            //Set the quest item for quest system 
            questSystemManager.AddQuestItem(questItem);

            //Disable the interactable
            gameObject.SetActive(false);
       
        }
    }

}
