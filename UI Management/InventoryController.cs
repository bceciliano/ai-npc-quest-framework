using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryController : MonoBehaviour
{
    [Header("UI Elements and SFX")]
    public GameObject inventoryPanel;
    public GameObject inventoryItemPrefab;
    public Transform inventoryScrollContent;
    public AudioSource audioSource;

    //Reference to Scripts
    private MenuManager menuManager;
    private UIManager uiManager;

    //Create a dictionary to store items and prefabs 
    private readonly Dictionary<QuestItem, GameObject> inventoryItemUIObjects = new();
    void Start()
    {
        //Get the manager components in the same gameObject
        menuManager = GetComponent<MenuManager>();
        uiManager = GetComponent<UIManager>();

        //Initialize UI
        uiManager.InitializeUIElements(inventoryPanel);
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I) && !PlayerInputController.isTyping)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        menuManager.ToggleMenu(inventoryPanel);
    }

    //Add an item to the current inventory 
    public void AddItemToInventory(QuestItem item)
    {
        //Create a UI Prefab and add it to the dictionary 
        GameObject inventoryItemUI = Instantiate(inventoryItemPrefab, inventoryScrollContent);
        inventoryItemUIObjects.Add(item, inventoryItemUI);

        //Access the UI elements to set image 
        Image itemSprite = inventoryItemUI.transform.Find("InventoryItem").GetComponent<Image>();
        itemSprite.sprite = item.icon;

        //Show Inventory 
        inventoryPanel.SetActive(true);
    }

    public void DisplayInventoryItem(QuestItem item)
    {
        //Play new inventory item sound 
        audioSource.Play();

        //Add item to inventory list 
        AddItemToInventory(item);
    }

    public void RemoveItemFromInventory(QuestItem item)
    {
        //Check if the item exists in the dictionary 
        if(inventoryItemUIObjects.TryGetValue(item, out GameObject inventoryItemUI))
        {
            //Remove the item from the dictionary and destroy the associated UI
            inventoryItemUIObjects.Remove(item);
            Destroy(inventoryItemUI);
        }
    }
}
