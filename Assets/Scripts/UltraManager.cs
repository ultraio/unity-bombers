using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraio;
using Gameframework;
using BrainCloud;
using BrainCloud.Common;

public class UltraManager : SingletonBehaviour<UltraManager>
{
    private void Start()
    {
        Ultra.UseBrowser = true;
        Ultra.Init(OnInitSuccess, OnInitFailure);
    }

    void OnInitSuccess(string username, string idToken)
    {
        Debug.Log($"{username} is now playing!");
        //Authenticate with brainCloud

    }

    void OnInitFailure(UltraError error)
    {
        Debug.LogError($"Ultra initialization failed - {error.Message}");
    }
}
