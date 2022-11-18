using UnityEngine;
using UnityEditor;

public class RemoteBuildSettings : MonoBehaviour
{
    public static void SetRemoteBuildSettings()
    {
        string appId = GetArg("-appId");
        string appSecret = GetArg("-appSecret");

        if(!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(appSecret))
        {
            PlayerPrefs.SetInt("IsRemoteBuild", 1);
            PlayerPrefs.SetString("AppID", appId);
            PlayerPrefs.SetString("AppSecret", appSecret);
        }
    }

    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
