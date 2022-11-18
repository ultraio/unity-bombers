using UnityEngine;
using UnityEditor;
using System.IO;

public class RemoteBuildSettings : MonoBehaviour
{
    public static void SetRemoteBuildSettings()
    {
        string appId = GetArg("-appId");
        string appSecret = GetArg("-appSecret");
        string appAuthUrl = GetArg("-url");

        if(!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(appSecret))
        {
            string path = "Assets/Resources/BCSettings.txt";
            StreamWriter writer = new StreamWriter(path, true);

            writer.WriteLine(appAuthUrl);
            writer.WriteLine(appId);
            writer.WriteLine(appSecret);
            writer.Close();

            AssetDatabase.ImportAsset(path);

            TextAsset bcsettings = Resources.Load<TextAsset>("BCSettings");


            Debug.Log($"Successfully set the appID and appSecret to: {bcsettings}");
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
