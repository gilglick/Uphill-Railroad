using UnityEngine;
using WSMGameStudio.RailroadSystem;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.SceneManagement;

[CustomEditor(typeof(TrainController_v3))]
public class TrainControllerInspector : Editor
{
    private TrainController_v3 _trainController_v3;

    private int _selectedMenuIndex = 0;
    private string[] _toolbarMenuOptions = new[] { "Controls", "Wagons", "Lights", "SFX", "Other Settings" };
    private GUIStyle _menuBoxStyle;

    private const string stopEngine = "Stop Engine";
    private const string startEngine = "Start Engine";
    private const string openLeftCabinDoor = "Open Left Cabin Door";
    private const string openRightCabinDoor = "Open Right Cabin Door";
    private const string openLeftPassengerDoor = "Open Left Passenger Door";
    private const string openRightPassengerDoor = "Open Right Passenger Door";
    private const string closeLeftCabinDoor = "Close Left Cabin Door";
    private const string closeRightCabinDoor = "Close Right Cabin Door";
    private const string closeLeftPassengerDoor = "Close Left Passenger Door";
    private const string closeRightPassengerDoor = "Close Right Passenger Door";

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        _trainController_v3 = target as TrainController_v3;

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

        if (_selectedMenuIndex == (int)TrainControllerInspectorMenu.Controls)
        {
            #region Controls

            GUILayout.Label("TRAIN CONTROLS", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            bool enginesOn = EditorGUILayout.Toggle("Engines On", _trainController_v3.enginesOn);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_trainController_v3, "Toggle Engines");
                _trainController_v3.enginesOn = enginesOn;
                MarkSceneAlteration();
            }

            EditorGUI.BeginChangeCheck();
            float maxSpeedKph = EditorGUILayout.Slider("Max Speed (KPH)", _trainController_v3.maxSpeedKph, 0f, 105f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_trainController_v3, "Changed Speed");
                _trainController_v3.maxSpeedKph = maxSpeedKph;
                MarkSceneAlteration();
            }

            EditorGUI.BeginChangeCheck();
            float acceleration = EditorGUILayout.Slider("Acceleration", _trainController_v3.acceleration, -1f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_trainController_v3, "Changed Acceleration");
                _trainController_v3.acceleration = acceleration;
                MarkSceneAlteration();
            }

            EditorGUI.BeginChangeCheck();
            bool automaticBrakes = EditorGUILayout.Toggle("Automatic Brakes", _trainController_v3.automaticBrakes);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_trainController_v3, "Toggle Auto Brakes");
                _trainController_v3.automaticBrakes = automaticBrakes;
                MarkSceneAlteration();
            }

            EditorGUI.BeginChangeCheck();
            float brake = EditorGUILayout.Slider("Brakes", _trainController_v3.brake, 0, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_trainController_v3, "Changed Brakes");
                _trainController_v3.brake = brake;
                MarkSceneAlteration();
            }

            GUILayout.Label("TRAIN OPERATIONS", EditorStyles.boldLabel);

            GUILayout.BeginVertical();
            if (_trainController_v3.enginesOn)
            {
                if (GUILayout.Button(stopEngine))
                {
                    _trainController_v3.ToggleEngine();
                    MarkSceneAlteration();
                }
            }
            else
            {
                if (GUILayout.Button(startEngine))
                {
                    _trainController_v3.ToggleEngine();
                    MarkSceneAlteration();
                }
            }

            GUILayout.BeginVertical();

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                GUILayout.BeginHorizontal();
                if (_trainController_v3.DoorsController != null)
                {
                    //Cabin left door
                    if (!_trainController_v3.DoorsController.CabinLeftDoorOpen)
                    {
                        if (GUILayout.Button(openLeftCabinDoor))
                            _trainController_v3.DoorsController.OpenCabinDoorLeft();
                    }
                    else
                    {
                        if (GUILayout.Button(closeLeftCabinDoor))
                            _trainController_v3.DoorsController.CloseCabinDoorLeft();
                    }
                    //Cabin right door
                    if (!_trainController_v3.DoorsController.CabinRightDoorOpen)
                    {
                        if (GUILayout.Button(openRightCabinDoor))
                            _trainController_v3.DoorsController.OpenCabinDoorRight();
                    }
                    else
                    {
                        if (GUILayout.Button(closeRightCabinDoor))
                            _trainController_v3.DoorsController.CloseCabinDoorRight();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    //Passengers left door
                    if (!_trainController_v3.DoorsController.PassengerLeftDoorOpen)
                    {
                        if (GUILayout.Button(openLeftPassengerDoor))
                        {
                            _trainController_v3.DoorsController.StationDoorDirection = StationDoorDirection.Left;
                            _trainController_v3.DoorsController.OpenPassengersDoors();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(closeLeftPassengerDoor))
                            _trainController_v3.DoorsController.ClosePassengersLeftDoors();
                    }
                    //Passengers right door
                    if (!_trainController_v3.DoorsController.PassengerRightDoorOpen)
                    {
                        if (GUILayout.Button(openRightPassengerDoor))
                        {
                            _trainController_v3.DoorsController.StationDoorDirection = StationDoorDirection.Right; _trainController_v3.DoorsController.OpenPassengersDoors();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(closeRightPassengerDoor))
                            _trainController_v3.DoorsController.ClosePassengersRightDoors();
                    }
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            #endregion
        }
        else if (_selectedMenuIndex == (int)TrainControllerInspectorMenu.Wagons)
        {
            #region Wagons

            GUILayout.Label("TRAIN WAGONS", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wagons"), true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_trainController_v3, "Changed Wagons");
                serializedObject.ApplyModifiedProperties();
                MarkSceneAlteration();
            }

            GUILayout.Label("WAGONS OPERATIONS", EditorStyles.boldLabel);

            #region EDITOR MODE ONLY

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                if (GUILayout.Button("Connect Wagons"))
                {
#if UNITY_EDITOR
                    Undo.RecordObject(_trainController_v3, "Wagons Connected");
#endif
                    SetWagonsPositions();
#if UNITY_EDITOR
                    MarkSceneAlteration();
#endif
                }
            }
            #endregion

            #region PLAY MODE ONLY

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Decouple First Wagon"))
                {
                    _trainController_v3.DecoupleFirstWagon();
                }

                if (GUILayout.Button("Decouple Last Wagon"))
                {
                    _trainController_v3.DecoupleLastWagon();
                }
                GUILayout.EndHorizontal();
            }
            #endregion

            #endregion
        }
        else if (_selectedMenuIndex == (int)TrainControllerInspectorMenu.Lights)
        {
            #region Lights

            GUILayout.Label("TRAIN LIGHTS", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("externalLights"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("internalLights"), true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_trainController_v3, "Changed Lights");
                serializedObject.ApplyModifiedProperties();
                MarkSceneAlteration();
            }

            GUILayout.Label("LIGHTS OPERATIONS", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("External Lights"))
            {
#if UNITY_EDITOR
                Undo.RecordObject(_trainController_v3, "Toggled External Lights");
#endif
                _trainController_v3.ToggleLights();
#if UNITY_EDITOR
                MarkSceneAlteration();
#endif
            }

            if (GUILayout.Button("Internal Lights"))
            {
#if UNITY_EDITOR
                Undo.RecordObject(_trainController_v3, "Toggled Internal Lights");
#endif
                _trainController_v3.ToggleInternalLights();
#if UNITY_EDITOR
                MarkSceneAlteration();
#endif
            }
            GUILayout.EndHorizontal();

            #endregion
        }
        else if (_selectedMenuIndex == (int)TrainControllerInspectorMenu.SFX)
        {
            #region SFX

            GUILayout.Label("TRAIN SFXS", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hornSFX"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bellSFX"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("engineSFX"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wheelsSFX"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("brakesSFX"), true);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_trainController_v3, "Changed SFX");
                serializedObject.ApplyModifiedProperties();
                MarkSceneAlteration();
            }

            GUILayout.Label("SFX OPERATIONS", EditorStyles.boldLabel);

            //Enabled only on lpay mode
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Honk"))
                {
                    _trainController_v3.Honk();
                }

                if (GUILayout.Button("Bell"))
                {
                    _trainController_v3.ToogleBell();
                }
                GUILayout.EndHorizontal();
            }

            #endregion
        }
        else if (_selectedMenuIndex == (int)TrainControllerInspectorMenu.OtherSettings)
        {
            #region OtherSettings

            GUILayout.Label("MECHANICAL COMPONENTS", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wheelsScripts"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sensors"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("backJoint"), true);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_trainController_v3, "Changed Other Settings");
                serializedObject.ApplyModifiedProperties();
                MarkSceneAlteration();
            }

            #endregion
        }

        GUILayout.EndVertical();
    }

    /// <summary>
    /// Automatically calculates wagons position on the train
    /// </summary>
    private void SetWagonsPositions()
    {
        if (_trainController_v3.wagons == null)
        {
            Debug.LogWarning("Wagons list cannot be null");
            return;
        }

        float lastWagonJointDistance = 0f;
        float totalDistance = 0f;

        for (int index = 0; index < _trainController_v3.wagons.Count; index++)
        {
            _trainController_v3.wagons[index].transform.position = _trainController_v3.transform.position;
            _trainController_v3.wagons[index].transform.rotation = _trainController_v3.transform.rotation;

            if (index == 0)
            {
                lastWagonJointDistance = Mathf.Abs(_trainController_v3.backJoint.transform.localPosition.z);
                totalDistance -= ((_trainController_v3.wagons[index].JoinDistance * 0.5f) + lastWagonJointDistance);
            }
            else
                totalDistance -= ((_trainController_v3.wagons[index].JoinDistance * 0.5f) + (lastWagonJointDistance * 0.5f));

            _trainController_v3.wagons[index].transform.position += (_trainController_v3.wagons[index].transform.forward * totalDistance);

            lastWagonJointDistance = _trainController_v3.wagons[index].JoinDistance;

            EditorUtility.SetDirty(_trainController_v3.wagons[index]);
        }
    }

    private void MarkSceneAlteration()
    {
        if (!Application.isPlaying)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(_trainController_v3);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
#endif 
        }
    }
}
