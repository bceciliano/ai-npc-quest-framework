# AI-Driven NPC Quest Framework (Unity/C#)

This repository contains the **Scripts** from my MSc dissertation project:  
an AI-driven NPC and quest framework built in Unity.  

The system demonstrates modular NPC behavior, dynamic quest pipelines, and integration with OpenAI’s API for generating dialogue.  

## Features
- **NPC System**  
  - `NPCSystem`, `NPCController`, `NPCManager`, `NPCData`  
  - Handles player interaction, dialogue, and quest initialization.  

- **Quest System**  
  - `QuestSystemManager`, `QuestData`, `QuestObjective`, `QuestItem`, `QuestReward`  
  - Event-driven quest progression and reward delivery.  

- **UI System**  
  - `UIManager`, `DialogueUIController`, `QuestMenuController`, `InventoryController`, `MenuManager`, `MainMenuController`  
  - Dialogue panels, quest tracking UI, inventory display, and menu handling.  

- **Player & World**  
  - `ThirdPersonMovement`, `PlayerInputController`, `Interactable`  
  - Player movement, input, and world interaction logic.  

- **Integration**  
  - `DatabaseManager`: SQLite persistence layer.  
  - `APIManager`: Safe API calls (reads `OPENAI_API_KEY` from environment variable).  


## Notes
- `APIManager` uses environment variables for API key
- Database is handled with **SQLite** (`Mono.Data.Sqlite`).  

---

## License
This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.
