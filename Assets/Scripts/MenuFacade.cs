﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuFacade : MonoBehaviour
{

    public Button startEditButton;
    public Button stopSaveEditButton;
    public Button stopDiscardEditButton;

    private AppState _appState;

    // Start is called before the first frame update
    void Start()
    {
        _appState = AppState.instance;

        startEditButton.interactable = true;
        stopSaveEditButton.interactable = false;
        stopDiscardEditButton.interactable = false;

        _appState.AddStartEditSessionListener(OnEditSessionStart);
        _appState.AddEndEditSessionListener(OnEditSessionEnd);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Visible(bool thisEvent) 
    {
        gameObject.SetActive(thisEvent);
    }

    public void HandleKeyInput(InputAction.CallbackContext context) {
        InputAction action = context.action;
        if (action.name == "ShowMenu") {
            bool isActive = gameObject.activeSelf;
            gameObject.SetActive(!isActive);
        }
    }

    public void OnStartEditButtonClicked() {
        _appState.StartEditSession();
    }

    public void OnStopSaveEditButtonClicked() {
        _appState.StopSaveEditSession();
    }

    public void OnStopDiscardEditButtonClicked() {
        _appState.StopDiscardEditSession();
    }

    // Changes the state of menu buttons when edit session starts.
    // 1) Disable Start Edit button
    // 2) Enable both Stop Edit buttons
    //
    // This method is triggered when:
    // 1) StartEdit action is triggered
    // 2) Start Edit button is clicked
    private void OnEditSessionStart() {
        startEditButton.interactable = false;
        stopSaveEditButton.interactable = true;
        stopDiscardEditButton.interactable = true;
    }

    // Changes the state of menu buttons when edit session ends.
    // 1) Enable Start Edit button
    // 2) Disable both Stop Edit buttons
    //
    // This method is triggered when:
    // 1) EndEdit action is triggered
    // 2) One of the Stop Edit buttons is clicked
    private void OnEditSessionEnd() {
        startEditButton.interactable = true;
        stopSaveEditButton.interactable = false;
        stopDiscardEditButton.interactable = false;
    }

}
