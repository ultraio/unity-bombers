using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraio;
using Gameframework;
using BrainCloud;
using BrainCloud.Common;
using System;

public class UltraManager : SingletonBehaviour<UltraManager>
{
    public Action<string, string> OnUltraLoginSuccess;
    public Action<string> OnUltraLoginFailure;

    private void Start()
    {
        Ultra.UseBrowser = true;
    }

    public void Init()
    {
        Ultra.Init(OnInitSuccess, OnInitFailure);
    }


    void OnInitSuccess(string username, string idToken)
    {
        OnUltraLoginSuccess?.Invoke(username, idToken);
    }

    void OnInitFailure(UltraError error)
    {
        OnUltraLoginFailure?.Invoke(error.Message);
    }
}
