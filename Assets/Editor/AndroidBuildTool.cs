using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public class AndroidBuildTool : EditorWindow
{
    private string buildFolder = "";
    private string fileName = "ChickenGame.apk";
    private bool developmentBuild = false;

    private void OnEnable()
    {
        // Default to Desktop if not set
        if (string.IsNullOrEmpty(buildFolder))
        {
            buildFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        }
    }

    [MenuItem("Tools/Chicken Game/Android APK Builder")]
    public static void ShowWindow()
    {
        GetWindow<AndroidBuildTool>("APK Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Android Build Settings", EditorStyles.boldLabel);
        
        buildFolder = EditorGUILayout.TextField("Build Folder", buildFolder);
        if (GUILayout.Button("Select Folder"))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Build Folder", buildFolder, "");
            if (!string.IsNullOrEmpty(selected)) buildFolder = selected;
        }

        fileName = EditorGUILayout.TextField("Base APK Name", fileName);
        developmentBuild = EditorGUILayout.Toggle("Development Build", developmentBuild);

        EditorGUILayout.Space();
        
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
        string previewName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}.apk";
        GUILayout.Label($"Will save as: {previewName}", EditorStyles.miniLabel);

        EditorGUILayout.Space();

        if (GUILayout.Button("Build APK", GUILayout.Height(40)))
        {
            BuildAPK();
        }

        if (GUILayout.Button("Open Build Folder"))
        {
            if (Directory.Exists(buildFolder))
            {
                Application.OpenURL("file://" + buildFolder);
            }
            else
            {
                Debug.LogWarning("Build folder does not exist yet.");
            }
        }
    }

    private void BuildAPK()
    {
        if (!Directory.Exists(buildFolder))
        {
            Directory.CreateDirectory(buildFolder);
        }

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
        string finalFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}.apk";
        string fullPath = Path.Combine(buildFolder, finalFileName);

        // Get all scenes in build settings
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = fullPath,
            target = BuildTarget.Android,
            options = developmentBuild ? BuildOptions.Development : BuildOptions.None
        };

        Debug.Log($"Starting Android Build to: {fullPath}...");
        
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build Succeeded! Size: {summary.totalSize / 1024 / 1024} MB");
            EditorUtility.DisplayDialog("Build Success", $"APK created successfully at:\n{fullPath}", "OK");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build Failed! Check console for details.");
            EditorUtility.DisplayDialog("Build Failed", "Android build failed. See Console for errors.", "OK");
        }
    }
}
