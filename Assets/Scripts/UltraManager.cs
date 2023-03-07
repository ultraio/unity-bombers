using Gameframework;
using System;
using Ultraio;
using UnityEngine;

public class UltraManager : SingletonBehaviour<UltraManager>
{
    [SerializeField] private bool UseBrowserLogin = false;

    public Action<string, string> OnUltraLoginSuccess;
    public Action<string> OnUltraLoginFailure;

    public bool UseBrowser
    {
        get => Ultra.UseBrowser; set => Ultra.UseBrowser = value;
    }

    public override void Awake()
    {
        base.Awake();

        UseBrowser = UseBrowserLogin;
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
