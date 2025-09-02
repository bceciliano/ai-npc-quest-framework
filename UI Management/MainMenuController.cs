using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private GameObject settingsMenuPanel;
    [SerializeField] private GameObject optionsMenuPanel;
    private CinemachineFreeLook freeLookCamera;

    private MenuManager menuManager;
    private UIManager uiManager;

    private float currentSensitivity = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        //Get the manager components in the same gameObject
        menuManager = GetComponent<MenuManager>();
        uiManager = GetComponent<UIManager>();

        //Initialize UI Elements
        uiManager.InitializeUIElements(settingsMenuPanel, optionsMenuPanel);
    }

    public void ToggleSettingsMenu()
    {
        menuManager.ToggleMenu(settingsMenuPanel);
        menuManager.LockCamera();
    }

    public void OpenOptions()
    {
        menuManager.ToggleMenu(optionsMenuPanel);
        menuManager.LockCamera();

        freeLookCamera =  menuManager.GetCamera();
        //Set sensitivity slider
        sensitivitySlider.value = currentSensitivity;

        //Add listener to slider 
        sensitivitySlider.onValueChanged.AddListener(AdjustCameraSensitivity);
    }
    private void AdjustCameraSensitivity(float sensitivity)
    {
        //Update the current sensitivity value 
        currentSensitivity = sensitivity;

        //Calculate the new sensitivity 
        float xSensitivity = Mathf.Lerp(80f, 500f, sensitivity);

        //Update the cinemachine settings 
        freeLookCamera.m_XAxis.m_MaxSpeed = xSensitivity;

    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = !isFullScreen;
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
