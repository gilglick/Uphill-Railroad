using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using WSMGameStudio.RailroadSystem;

public class RailroadTurnoutCreatorWindow : EditorWindow
{
    private string turnoutPrefabName;
    private string outputDirectory = string.Empty;
    private string defaultDiretory = "WSM Game Studio/Train Controller_v3/Prefabs/Turnouts/";
    private string txtMessage;
    private Color txtColor = Color.black;
    private GUIStyle errorMessageStyle;
    private GUIStyle instructionsTitleStyle;
    private GUIStyle instructionsStyle;

    public List<GameObject> mainRails;
    public GameObject turnoutRails;

    [MenuItem("Window/WSM Game Studio/Railroad Turnout Creator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<RailroadTurnoutCreatorWindow>("Railroad Turnout Creator");
    }

    /// <summary>
    /// Render Window
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Label("Railroad Turnout Creator", EditorStyles.boldLabel);

        if (outputDirectory == string.Empty)
            outputDirectory = defaultDiretory;

        turnoutPrefabName = EditorGUILayout.TextField("Turnout Prefab Name", turnoutPrefabName);
        outputDirectory = EditorGUILayout.TextField("Output Directory", outputDirectory);

        if (GUILayout.Button("Create"))
        {
            CreateTurnout();
        }

        errorMessageStyle = new GUIStyle();
        errorMessageStyle.normal.textColor = txtColor;
        GUILayout.Label(txtMessage, errorMessageStyle);

        string instructions = string.Format(
        " 1) (Optional) Create the main rail(s) segment(s) and the turnout rail segment using the Railroad Builder{0}" +
        " 2) (Optional) Bake the main rail(s) segment(s) and the turnout rail segment as prefabs using the Mesh Baker{0}" +
        " 3) Drag & Drop the rails prefabs to the scene at position 0, 0, 0{0}" +
        " 4) Tag the turnout rail segment object as TurnoutRails{0}" +
        " 5) Open the Railroad Turnout Window and select a name and output folder{0}" +
        " 6) Select the rail(s) segment(s) and the turnout rail segment objects from your scene{0}" +
        " 7) Click on the Create Button", Environment.NewLine);

        instructionsTitleStyle = new GUIStyle();
        instructionsTitleStyle.normal.textColor = Color.blue;
        instructionsTitleStyle.fontStyle = FontStyle.Bold;
        GUILayout.Label(" Instructions", instructionsTitleStyle);

        instructionsStyle = new GUIStyle();
        instructionsStyle.normal.textColor = Color.blue;
        GUILayout.Label(instructions, instructionsStyle);
    }

    /// <summary>
    /// Save turnout prefab
    /// </summary>
    private void CreateTurnout()
    {
        txtMessage = string.Empty;

        if (turnoutPrefabName == string.Empty)
        {
            ShowMessage("Please, enter a name for the prefab before proceding", WSMGameStudio.Splines.MessageType.Error);
            return;
        }

        GameObject[] selectedGameObjects = Selection.gameObjects;
        if (selectedGameObjects == null)
        {
            ShowMessage("No rails selected", WSMGameStudio.Splines.MessageType.Error);
            return;
        }

        if (selectedGameObjects.Length < 2)
        {
            ShowMessage("Select at least 2 rails segments", WSMGameStudio.Splines.MessageType.Error);
            return;
        }

        int tournoutsCount = 0;
        foreach (GameObject obj in selectedGameObjects)
        {
            if (obj.tag == "TurnoutRails")
                tournoutsCount++;
        }

        if(tournoutsCount != 1)
        {
            ShowMessage("Exactly 1 rail segments must be tagged as TurnoutRails", WSMGameStudio.Splines.MessageType.Error);
            return;
        }

        DirectoryInfo info = new DirectoryInfo(Application.dataPath);
        string folderPath = Path.Combine(info.Name, outputDirectory);
        string prefabFilePath = Path.Combine(folderPath, turnoutPrefabName + "_prefab.prefab");

        //Check folder existence
        if (!Directory.Exists(folderPath))
        {
            ShowMessage(string.Format("Folder does not exist: {0}", folderPath), WSMGameStudio.Splines.MessageType.Error);
            return;
        }

        //Overwrite dialog
        if (Exists(prefabFilePath))
        {
            if (!ShowOverwriteDialog(turnoutPrefabName))
                return;
        }

        /*
         * Check rails Main and Turnout
         * Create object
         * Set children
         * Delete object
         */
        GameObject sceneObject = new GameObject(turnoutPrefabName);
        RailroadSwitch_v3 railroadSwitchScript = sceneObject.AddComponent<RailroadSwitch_v3>();
        railroadSwitchScript._railsColliders = new List<GameObject>();

        foreach (GameObject obj in selectedGameObjects)
        {
            foreach (Transform child in obj.transform)
            {
                if (child.name == "GuideCollider")
                {
                    railroadSwitchScript._railsColliders.Add(child.gameObject);

                    if (obj.tag == "TurnoutRails")
                        child.gameObject.SetActive(false);
                }
            }

            obj.transform.SetParent(sceneObject.transform);
        }

        //Save prefab as .prefab file
        GameObject prefab = SavePrefabFile(sceneObject, prefabFilePath, true);
        //Intantiate prefab
        Instantiate(prefab);

        ShowMessage(string.Format("{0} Created Succesfuly!", turnoutPrefabName), WSMGameStudio.Splines.MessageType.Success);
    }

    #region Auxiliar Methods
    /// <summary>
    /// Save prefab as .prefab file
    /// </summary>
    /// <param name="prefabName"></param>
    /// <param name="prefabFilePath"></param>
    /// <returns></returns>
    private GameObject SavePrefabFile(GameObject sceneObject, string prefabFilePath, bool destroySceneObject)
    {
        GameObject prefab = PrefabUtility.CreatePrefab(prefabFilePath, sceneObject);

        if (destroySceneObject)
            GameObject.DestroyImmediate(sceneObject);

        return prefab;
    }

    /// <summary>
    /// Save mesh as .asset file
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="meshFilePath"></param>
    /// <returns>Saved mesh reference</returns>
    private static Mesh SaveMeshFile(Mesh mesh, string meshFilePath)
    {
        MeshUtility.Optimize(mesh);
        AssetDatabase.CreateAsset(mesh, meshFilePath);
        AssetDatabase.SaveAssets();
        return mesh;
    }

    /// <summary>
    /// Show message on Window
    /// </summary>
    /// <param name="message"></param>
    /// <param name="type"></param>
    private void ShowMessage(string message, WSMGameStudio.Splines.MessageType type)
    {
        txtMessage = message;

        switch (type)
        {
            case WSMGameStudio.Splines.MessageType.Success:
                txtColor = new Color(0, 0.5f, 0); //Dark green;
                break;
            case WSMGameStudio.Splines.MessageType.Warning:
                txtColor = Color.yellow;
                break;
            case WSMGameStudio.Splines.MessageType.Error:
                txtColor = Color.red;
                break;
        }
    }

    /// <summary>
    /// Show overwrite dialog confirmation
    /// </summary>
    /// <param name="meshName"></param>
    /// <returns></returns>
    private bool ShowOverwriteDialog(string prefabName)
    {
        return EditorUtility.DisplayDialog("Are you sure?",
                        string.Format("A prefab named {0} already exists. Do you want to overwrite it?", prefabName.ToUpper()),
                        "Yes",
                        "No");
    }

    /// <summary>
    /// Check if file already exists
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private bool Exists(string filePath)
    {
        return File.Exists(filePath);
    }
    #endregion
}
