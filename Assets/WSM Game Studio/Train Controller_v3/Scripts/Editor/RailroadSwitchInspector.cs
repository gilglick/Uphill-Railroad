using UnityEngine;
using WSMGameStudio.RailroadSystem;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.SceneManagement;

[CustomEditor(typeof(RailroadSwitch_v3))]
public class RailroadSwitchInspector : Editor
{
    private RailroadSwitch_v3 _railroadSwitch;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        _railroadSwitch = target as RailroadSwitch_v3;

        if (GUILayout.Button("Switch Rails"))
        {
#if UNITY_EDITOR
            Undo.RecordObject(_railroadSwitch, "Switch Rails");
#endif
            _railroadSwitch.SwitchRails();
#if UNITY_EDITOR
            MarkSceneAlteration();
#endif
        }
    }

    private void MarkSceneAlteration()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(_railroadSwitch);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
#endif
    }
}
