using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WagonsBuilderWindow : EditorWindow
{
    private string _wagonName;
    private string _txtMessage;
    private Color _txtColor = Color.black;
    private GUIStyle _errorMessageStyle;
    private GUIStyle _menuBoxStyle;

    private CustomWagonProfile _customProfile;
    private GameObject _modelPrefab;

    /// <summary>
    /// Open new window or select current opened one
    /// </summary>
    [MenuItem("Window/WSM Game Studio/Custom Wagons Creator")]
    public static void ShowWindow()
    {
        GetWindow<WagonsBuilderWindow>("Custom Wagons Creator");
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Label("Custom Wagons Creator", EditorStyles.boldLabel);

        //Set up the box style if null
        if (_menuBoxStyle == null)
        {
            _menuBoxStyle = new GUIStyle(GUI.skin.box);
            _menuBoxStyle.normal.textColor = GUI.skin.label.normal.textColor;
            _menuBoxStyle.fontStyle = FontStyle.Bold;
            _menuBoxStyle.alignment = TextAnchor.UpperLeft;
        }

        GUILayout.BeginVertical(_menuBoxStyle);

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(100));
        EditorGUILayout.LabelField("Profile", GUILayout.Width(100));
        EditorGUILayout.LabelField("Model", GUILayout.Width(100));
        EditorGUILayout.LabelField("Name", GUILayout.Width(100));
        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        _customProfile = EditorGUILayout.ObjectField(_customProfile, typeof(CustomWagonProfile), false) as CustomWagonProfile;
        _modelPrefab = EditorGUILayout.ObjectField(_modelPrefab, typeof(GameObject), false) as GameObject;
        _wagonName = EditorGUILayout.TextField(string.Empty, _wagonName);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Create"))
        {
            CreateWagon();
        }

        _errorMessageStyle = new GUIStyle();
        _errorMessageStyle.normal.textColor = _txtColor;
        _errorMessageStyle.fontStyle = FontStyle.Bold;
        GUILayout.Label(_txtMessage, _errorMessageStyle);

        GUILayout.EndVertical(); //Stylized Box
    }

    /// <summary>
    /// 
    /// </summary>
    private void CreateWagon()
    {
        _txtMessage = string.Empty;

        bool valid = CustomWagonCreator.Validate(_customProfile, _modelPrefab, _wagonName, out _txtMessage);
        if (!valid)
        {
            ShowMessage(_txtMessage, WSMGameStudio.Splines.MessageType.Error);
            return;
        }

        bool success = CustomWagonCreator.Create(_customProfile, _modelPrefab, _wagonName, out _txtMessage);
        if (success)
            ShowMessage(_txtMessage, WSMGameStudio.Splines.MessageType.Success);
    }

    /// <summary>
    /// Show message on Window
    /// </summary>
    /// <param name="message"></param>
    /// <param name="type"></param>
    private void ShowMessage(string message, WSMGameStudio.Splines.MessageType type)
    {
        _txtMessage = message;

        switch (type)
        {
            case WSMGameStudio.Splines.MessageType.Success:
                _txtColor = new Color(0, 0.5f, 0); //Dark green;
                break;
            case WSMGameStudio.Splines.MessageType.Warning:
                _txtColor = Color.yellow;
                break;
            case WSMGameStudio.Splines.MessageType.Error:
                _txtColor = Color.red;
                break;
        }
    }
}
