using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WSMGameStudio.Splines;

[CustomEditor(typeof(SplineMeshRenderer))]
public class SplineMeshRendererInspector : Editor
{
    private SplineMeshRenderer _splineMeshRenderer;

    private int _selectedMenuIndex = 0;
    private string[] _toolbarMenuOptions = new[] { "Mesh Generation Settings", "Collision Settings" };
    private GUIStyle _menuBoxStyle;

    private GUIContent _btnPrintMeshDetails = new GUIContent("Print Mesh Details", "Prints the generated mesh details on console window when Realtime Mesh Generation is selected");
    private GUIContent _btnCreateMesh = new GUIContent("Generate Mesh", "Manually generates the mesh when Manual Mesh Generation is selected");
    private GUIContent _btnBakeMesh = new GUIContent("Bake Mesh", "Exports the generated mesh as a prefab (Mesh Baker Window)");
    private GUIContent _btnConnectNewRenderer = new GUIContent("Connect New Renderer", string.Format("Connects a new Mesh Renderer at the end of the current one. Usefull to improve performance with occlusion culling.", System.Environment.NewLine));

    /// <summary>
    /// Draw spline on editor
    /// </summary>
    private void OnSceneGUI()
    {
        _splineMeshRenderer = (SplineMeshRenderer)target;

        if (_splineMeshRenderer.Spline != null && _splineMeshRenderer.Spline.Theme != null)
        {
            Handles.BeginGUI();

            int line = _splineMeshRenderer.Spline.Theme.supportsVerticalBuilding ? SMR_UISettings.btnColumnLine_4 : SMR_UISettings.btnColumnLine_3;

            if (_splineMeshRenderer.MeshGenerationMethod == MeshGenerationMethod.Manual)
            {
                // Scene Window buttons
                if (GUI.Button(new Rect(SMR_UISettings.btnColumnLine_2, line, SMR_UISettings.btnWidth, SMR_UISettings.btnHeight), _splineMeshRenderer.Spline.Theme.MeshIcon))
                {
                    _splineMeshRenderer.ExtrudeMesh();
                }
                // Add icons to inspector buttons
                _btnCreateMesh.image = _splineMeshRenderer.Spline.Theme.SmallMeshIcon;
            }
            else if (_splineMeshRenderer.MeshGenerationMethod == MeshGenerationMethod.Realtime)
            {
                // Scene Window buttons
                if (GUI.Button(new Rect(SMR_UISettings.btnColumnLine_2, line, SMR_UISettings.btnWidth, SMR_UISettings.btnHeight), _splineMeshRenderer.Spline.Theme.LogIcon))
                {
                    _splineMeshRenderer.PrintMeshDetails();
                }
                // Add icons to inspector buttons
                _btnPrintMeshDetails.image = _splineMeshRenderer.Spline.Theme.SmallLogIcon;
            }

            Handles.EndGUI();
        }
    }

    /// <summary>
    /// Custom inspector
    /// </summary>
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        _splineMeshRenderer = (SplineMeshRenderer)target;

        //Set up the box style if null
        if (_menuBoxStyle == null)
        {
            _menuBoxStyle = new GUIStyle(GUI.skin.box);
            _menuBoxStyle.normal.textColor = GUI.skin.label.normal.textColor;
            _menuBoxStyle.fontStyle = FontStyle.Bold;
            _menuBoxStyle.alignment = TextAnchor.UpperLeft;
        }

        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_spline"), false);
        serializedObject.ApplyModifiedProperties();

        EditorGUI.BeginChangeCheck();
        _selectedMenuIndex = GUILayout.Toolbar(_selectedMenuIndex, _toolbarMenuOptions);
        if (EditorGUI.EndChangeCheck())
        {
            GUI.FocusControl(null);
        }

        GUILayout.BeginVertical(_menuBoxStyle);

        if (_selectedMenuIndex == (int)MeshRendererInspectorMenu.MeshGenerationSettings)
        {
            #region Mesh Generation Settings

            GUILayout.Label("Generation Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            MeshGenerationMethod meshGenerationMethod = (MeshGenerationMethod)EditorGUILayout.EnumPopup("Mesh Generation Method", _splineMeshRenderer.MeshGenerationMethod);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_splineMeshRenderer, "Mesh Generation Method Changed");
                MarkSceneAlteration();
                _splineMeshRenderer.MeshGenerationMethod = meshGenerationMethod;
            }

            EditorGUI.BeginChangeCheck();
            Mesh baseMesh = (Mesh)EditorGUILayout.ObjectField("Base Mesh", _splineMeshRenderer.BaseMesh, typeof(Mesh), false);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_splineMeshRenderer, "Base Mesh Changed");
                MarkSceneAlteration();
                _splineMeshRenderer.BaseMesh = baseMesh;
            }

            EditorGUI.BeginChangeCheck();
            Vector3 meshOffset = EditorGUILayout.Vector3Field("Mesh Offset", _splineMeshRenderer.MeshOffset);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_splineMeshRenderer, "Mesh Offset Changed");
                MarkSceneAlteration();
                _splineMeshRenderer.MeshOffset = meshOffset;
            }

            GUILayout.Label("Mesh Operations", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            if (_splineMeshRenderer.MeshGenerationMethod != MeshGenerationMethod.Manual)
            {
                if (GUILayout.Button(_btnPrintMeshDetails))
                {
                    _splineMeshRenderer.PrintMeshDetails();
                }
            }
            else
            {
                if (GUILayout.Button(_btnCreateMesh))
                {
                    _splineMeshRenderer.ExtrudeMesh();
                }
            }

            if (GUILayout.Button(_btnBakeMesh))
            {
                BakeMeshWindow.ShowWindow();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_btnConnectNewRenderer))
            {
                Selection.activeGameObject = _splineMeshRenderer.ConnectNewRenderer();
                MarkSceneAlteration();
            }
            GUILayout.EndHorizontal();

            #endregion
        }
        else if (_selectedMenuIndex == (int)MeshRendererInspectorMenu.CollisionSettings)
        {
            #region Collision Settings

            GUILayout.Label("Collision Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            bool enableCollision = EditorGUILayout.Toggle("Enable Collision", _splineMeshRenderer.EnableCollision);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_splineMeshRenderer, "Enable Collision Changed");
                MarkSceneAlteration();
                _splineMeshRenderer.EnableCollision = enableCollision;
            }

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_customMeshColliders"), true);
            serializedObject.ApplyModifiedProperties();

            #endregion
        }

        GUILayout.EndVertical();
    }

    /// <summary>
    /// Show player the scene needs to be saved
    /// </summary>
    private void MarkSceneAlteration()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(_splineMeshRenderer);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
