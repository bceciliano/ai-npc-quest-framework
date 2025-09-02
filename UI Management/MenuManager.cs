using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public class MenuManager : MonoBehaviour
{
    [Header("Camera and SFX")]
    [SerializeField] private CinemachineFreeLook freeLook;
    [SerializeField] private AudioSource audioSource;
    
    private bool isMenuActive;
    private MainMenuController mainMenuController;

    public CinemachineFreeLook GetCamera()
    {
        return freeLook;
    }
    public void ToggleMenu(GameObject menuPanel)
    {
        isMenuActive = menuPanel.activeSelf;
        menuPanel.SetActive(!isMenuActive);
        PlaySFX();
    }

    public void PlaySFX()
    {
        audioSource.Play();
    }
   
    public void LockCamera()
    {
        freeLook.enabled = isMenuActive;
    }
}
