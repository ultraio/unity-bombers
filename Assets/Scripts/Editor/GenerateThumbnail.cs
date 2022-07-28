using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class GenerateThumbnailTool : EditorWindow
{
    static bool showAssets = true;

    static List<GameObject> selectedAssets = new List<GameObject>();

    string outputPath = string.Empty;

    [MenuItem("Tools/Generate Thumbnail")]
    static void Init()
    {
        GenerateThumbnailTool toolWindow = (GenerateThumbnailTool)EditorWindow.GetWindow(typeof(GenerateThumbnailTool));
        toolWindow.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Thumbnail", EditorStyles.boldLabel);
        //EditorGUILayout.TextField("Selected asset: ", selectedAssets.name);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.GetControlRect(true, 16f, EditorStyles.foldout);

        Rect foldRect = GUILayoutUtility.GetLastRect();

        if(Event.current.type == EventType.MouseUp && foldRect.Contains(Event.current.mousePosition))
        {
            showAssets = !showAssets;
            GUI.changed = true;
            Event.current.Use();
        }

        showAssets = EditorGUI.Foldout(foldRect, showAssets, selectedAssets.Count + " Selected Assets");

        EditorGUI.EndChangeCheck();

        string placeholderPath = string.Empty;

        if (showAssets)
        {
            EditorGUI.indentLevel++;

            if (selectedAssets.Count > 0)
            {
                foreach (GameObject g in selectedAssets)
                {
                    EditorGUILayout.LabelField(g.name);
                }
            }
            else
            {
                EditorGUILayout.LabelField("none");
            }

            EditorGUI.indentLevel--;
        }

        if(selectedAssets.Count > 0 && string.IsNullOrEmpty(placeholderPath))
        {
            placeholderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selectedAssets[0]));
        }

        outputPath = EditorGUILayout.TextField("Output folder", outputPath == string.Empty ? placeholderPath : outputPath, EditorStyles.textField);
        
        if (string.IsNullOrEmpty(outputPath)) return;
        
        if(selectedAssets.Count > 0)
        {
            Debug.Log("Output folder: " + outputPath);

            if(GUILayout.Button("Generate " + selectedAssets.Count + " thumbnail"))
            {
                Directory.CreateDirectory(outputPath);

                foreach (GameObject g in selectedAssets)
                {
                    Texture2D thumb = AssetPreview.GetAssetPreview(g);
                    byte[] bytes = thumb.EncodeToPNG();
                    string fullPath = Path.Combine(outputPath, g.name + ".png");
                    Debug.Log("Output folder: " + outputPath);
                    Debug.Log("Full path: " + fullPath);
                    File.WriteAllBytes(fullPath, bytes);

                    Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + fullPath);
                }
            }
        }
        
    }

    private void OnSelectionChange()
    {
        selectedAssets.Clear();

        if (Selection.assetGUIDs.Length > 0)
        {
            foreach(Object o in Selection.objects)
            {
                if(o is GameObject)
                {
                    selectedAssets.Add((GameObject)o);
                }
            } 
        }
    }

    private void OnInspectorUpdate()
    {
        this.Repaint();
    }
}
