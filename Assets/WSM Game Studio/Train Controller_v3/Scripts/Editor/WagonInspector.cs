using UnityEngine;
using WSMGameStudio.RailroadSystem;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.SceneManagement;

[CustomEditor(typeof(Wagon_v3))]
public class WagonInspector : Editor
{
    private Wagon_v3 _wagon;

    private int _selectedMenuIndex = 0;
    private string[] _toolbarMenuOptions = new[] { "Coupling", "Lights", "SFX", "Other Settings" };
    private GUIStyle _menuBoxStyle;

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        _wagon = target as Wagon_v3;

        EditorGUI.BeginChangeCheck();
        _selectedMenuIndex = GUILayout.Toolbar(_selectedMenuIndex, _toolbarMenuOptions);
        if (EditorGUI.EndChangeCheck())
        {
            GUI.FocusControl(null);
        }

        //Set up the box style if null
        if (_menuBoxStyle == null)
        {
            _menuBoxStyle = new GUIStyle(GUI.skin.box);
            _menuBoxStyle.normal.textColor = GUI.skin.label.normal.textColor;
            _menuBoxStyle.fontStyle = FontStyle.Bold;
            _menuBoxStyle.alignment = TextAnchor.UpperLeft;
        }
        GUILayout.BeginVertical(_menuBoxStyle);

        if (_selectedMenuIndex == (int)WagonInspectorMenu.Coupling)
        {
            #region Coupling

            GUILayout.Label("COUPLING SETTINGS", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("coupling"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("decouplingSettings"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("recouplingTimeout"), true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_wagon, "Changed Coupling Settings");
                serializedObject.ApplyModifiedProperties();
                MarkSceneAlteration();
            }
            #endregion
        }
        else if (_selectedMenuIndex == (int)WagonInspectorMenu.Lights)
        {
            #region Lights

            GUILayout.Label("WAGON LIGHTS", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("externalLights"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("internalLights"), true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_wagon, "Changed Lights");
                serializedObject.ApplyModifiedProperties();
                MarkSceneAlteration();
            } 
            #endregion
        }
        else if (_selectedMenuIndex == (int)WagonInspectorMenu.SFX)
        {
            #region SFX

            GUILayout.Label("WAGON SFXS", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wheelsSFX"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wagonConnectionSFX"), true);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_wagon, "Changed SFX");
                serializedObject.ApplyModifiedProperties();
                MarkSceneAlteration();
            } 
            #endregion
        }
        else if (_selectedMenuIndex == (int)WagonInspectorMenu.OtherSettings)
        {
            #region Other Settings

            GUILayout.Label("MECHANICAL COMPONENTS", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wheelsScripts"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sensors"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("backJoint"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("frontJoint"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jointAnchor"), true);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_wagon, "Changed Other Settings");
                serializedObject.ApplyModifiedProperties();
                MarkSceneAlteration();
            }
            #endregion
        }

        GUILayout.EndVertical();
    }

    private void MarkSceneAlteration()
    {
        if (!Application.isPlaying)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(_wagon);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
#endif 
        }
    }
}
