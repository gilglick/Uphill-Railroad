using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using WSMGameStudio.Splines;

[CustomEditor(typeof(Spline))]
public class SplineInspector : Editor
{
    private const int _lineSteps = 10;

    private Spline _spline;
    private Transform _splineTransform;
    private Quaternion _handleRotation;
    private const float _handleSize = 0.04f;
    private const float _pickSize = 0.06f;
    private int _selectedIndex = -1;

    private int _selectedMenuIndex = 0;
    private string[] _toolbarMenuOptions = new[] { "Curve Settings", "Spline Settings", "Handles Settings" };
    private GUIStyle _menuBoxStyle;

    private GUIContent _btnAddNewCurve = new GUIContent("Add Curve", "Adds a new curve at the end of the spline");
    private GUIContent _btnRemoveCurve = new GUIContent("Remove Curve", "Removes the last spline curve");

    private GUIContent _btnResetCurve = new GUIContent("Straight Line", "Change the last curve to a straight line");
    private GUIContent _btnTurnRight = new GUIContent("Right", "Turns the last spline curve to the right");
    private GUIContent _btnTurnLeft = new GUIContent("Left", "Turns the last spline curve to the left");
    private GUIContent _btnTurnUpwards = new GUIContent("Up", "Turns the last spline curve upwards");
    private GUIContent _btnTurnDownwards = new GUIContent("Down", "Turns the last spline curve downwards");

    private GUIContent _btnSubdivideCurve = new GUIContent("Subdivide Curve", "Insert a new curve between the current selected curve and the next one");
    private GUIContent _btnDissolveCurve = new GUIContent("Dissolve Curve", "Dissolve selected curve");

    private GUIContent _btnSplitSpline = new GUIContent("Split Spline", "Split spline in two");

    private GUIContent _btnResetRotations = new GUIContent("Reset Rotations", "Resets all control points rotations");
    private GUIContent _btnResetSpline = new GUIContent("Reset Spline", "Restarts spline from scratch");

    private GUIContent _btnFlatten = new GUIContent("Flatten", "Resets all control points height values, making a flat spline");
    private GUIContent _btnAppendSpline = new GUIContent("Append Spline", "Connect a new spline at the end of the current one");

    private static Color[] _alignmentColors = {
        Color.white,
        Color.yellow,
        Color.cyan,
        Color.red
    };

    /// <summary>
    /// Draw spline on editor
    /// </summary>
    private void OnSceneGUI()
    {
        _spline = target as Spline;
        _splineTransform = _spline.transform;
        _handleRotation = Tools.pivotRotation == PivotRotation.Local ? _splineTransform.rotation : Quaternion.identity;

        bool startVisible, isHandle1_Visible, isHandle2_Visible, endVisible;

        //Draw spline - 1 section by iteration
        Vector3 segmentStartPoint = ShowControlPoint(0, out startVisible);
        for (int i = 1; i < _spline.ControlPointCount; i += 3)
        {
            //Draw control points
            Vector3 bezierHandle1 = ShowControlPoint(i, out isHandle1_Visible); //Handle 1
            Vector3 bezierHandle2 = ShowControlPoint(i + 1, out isHandle2_Visible); //Handle 2
            Vector3 segmentEndPoint = ShowControlPoint(i + 2, out endVisible); //Next Section start

            //Draw handle lines
            Handles.color = Color.red;
            if (isHandle1_Visible) Handles.DrawLine(segmentStartPoint, bezierHandle1);
            if (isHandle2_Visible) Handles.DrawLine(bezierHandle2, segmentEndPoint);

            Handles.DrawBezier(segmentStartPoint, segmentEndPoint, bezierHandle1, bezierHandle2, Color.white, null, 2f);
            segmentStartPoint = segmentEndPoint;
        }

        ShowDirections();

        Handles.BeginGUI();

        if (_spline.Theme == null)
        {
            Debug.LogWarning("Spline Theme Property not set. To unlock Curve Operations on the Scene Window you need to choose a theme.");
        }
        else
        {
            // Scene Window buttons
            if (GUI.Button(new Rect(SMR_UISettings.btnColumnLine_1, SMR_UISettings.btnColumnLine_1, SMR_UISettings.btnWidth, SMR_UISettings.btnHeight), _spline.Theme.AddIcon))
                AddCurve();
            if (GUI.Button(new Rect(SMR_UISettings.btnColumnLine_2, SMR_UISettings.btnColumnLine_1, SMR_UISettings.btnWidth, SMR_UISettings.btnHeight), _spline.Theme.DeleteIcon))
                RemoveCurve();
            if (_spline.Theme.supportsVerticalBuilding)
            {
                if (GUI.Button(new Rect(SMR_UISettings.btnColumnLine_1, SMR_UISettings.btnColumnLine_2, SMR_UISettings.btnWidth, SMR_UISettings.btnHeight), _spline.Theme.CurvedArrowLeftIcon))
                    TurnCurve_Left();
                if (GUI.Button(new Rect(SMR_UISettings.btnColumnLine_2, SMR_UISettings.btnColumnLine_2, SMR_UISettings.btnWidth, SMR_UISettings.btnHeight), _spline.Theme.CurvedArrowRightIcon))
                    TurnCurve_Right();
                if (GUI.Button(new Rect(SMR_UISettings.btnColumnLine_1, SMR_UISettings.btnColumnLine_3, SMR_UISettings.btnWidth, SMR_UISettings.btnHeight), _spline.Theme.CurvedArrowUpIcon))
                    TurnCurve_Upwards();
                if (GUI.Button(new Rect(SMR_UISettings.btnColumnLine_2, SMR_UISettings.btnColumnLine_3, SMR_UISettings.btnWidth, SMR_UISettings.btnHeight), _spline.Theme.CurvedArrowDownIcon))
                    TurnCurve_Downwards();
                if (GUI.Button(new Rect(SMR_UISettings.btnColumnLine_1, SMR_UISettings.btnColumnLine_4, SMR_UISettings.btnWidth, SMR_UISettings.btnHeight), _spline.Theme.ArrowUpIcon))
                    ResetCurve();
            }
            else
            {
                if (GUI.Button(new Rect(SMR_UISettings.btnColumnLine_1, SMR_UISettings.btnColumnLine_2, SMR_UISettings.btnWidth, SMR_UISettings.btnHeight), _spline.Theme.CurvedArrowLeftIcon))
                    TurnCurve_Left();
                if (GUI.Button(new Rect(SMR_UISettings.btnColumnLine_2, SMR_UISettings.btnColumnLine_2, SMR_UISettings.btnWidth, SMR_UISettings.btnHeight), _spline.Theme.CurvedArrowRightIcon))
                    TurnCurve_Right();
                if (GUI.Button(new Rect(SMR_UISettings.btnColumnLine_1, SMR_UISettings.btnColumnLine_3, SMR_UISettings.btnWidth, SMR_UISettings.btnHeight), _spline.Theme.ArrowUpIcon))
                    ResetCurve();
            }

            // Add icons to inspector buttons
            _btnAddNewCurve.image = _spline.Theme.SmallAddIcon;
            _btnRemoveCurve.image = _spline.Theme.SmallDeleteIcon;
            _btnTurnRight.image = _spline.Theme.SmallCurvedArrowRightIcon;
            _btnTurnLeft.image = _spline.Theme.SmallCurvedArrowLeftIcon;
            _btnTurnUpwards.image = _spline.Theme.SmallCurvedArrowUpIcon;
            _btnTurnDownwards.image = _spline.Theme.SmallCurvedArrowDownIcon;
            _btnResetCurve.image = _spline.Theme.SmallArrowUpIcon;
        }

        Handles.EndGUI();
    }

    /// <summary>
    /// Show spline point handles on Editor
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private Vector3 ShowControlPoint(int index, out bool visible)
    {
        bool isBezierHandle = false;
        bool isSelected = false;
        visible = CheckControlPointVisibility(index, out isSelected, out isBezierHandle);

        Vector3 pointPosition = _splineTransform.TransformPoint(_spline.GetControlPointPosition(index));
        Quaternion pointRotation = _spline.GetControlPointRotation(index);

        if (visible)
        {
            float size = HandleUtility.GetHandleSize(pointPosition);
            if (index == 0)
                size *= 5f;
            else
                size *= 3f;

            if (isBezierHandle) //Handles uses selected mode colors
                Handles.color = _alignmentColors[(int)_spline.GetHandlesAlignment(index)];
            else
                Handles.color = Color.green; //Start of new spline section will be green for easy identification

            //Button used to select handle/controlPoint
            if (Handles.Button(pointPosition, _handleRotation, (size * _handleSize), (size * _pickSize), Handles.SphereHandleCap))
            {
                _selectedIndex = index;
                Repaint();
            }

            //Is selected handle/controlPoint
            if (isSelected)
            {
                //Position handle
                EditorGUI.BeginChangeCheck();
                pointPosition = Handles.PositionHandle(pointPosition, _handleRotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_spline, "Move Point");
                    MarkSceneAlteration();
                    _spline.SetControlPointPosition(index, _splineTransform.InverseTransformPoint(pointPosition));
                }

                //Rotation handle
                EditorGUI.BeginChangeCheck();
                pointRotation = Handles.DoRotationHandle(pointRotation, pointPosition);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_spline, "Rotate Point");
                    MarkSceneAlteration();
                    _spline.SetControlPointRotation(index, pointRotation);
                }
            }
        }

        return pointPosition;
    }

    /// <summary>
    /// Check if controlpoint or handle should be visible
    /// </summary>
    /// <param name="index"></param>
    /// <param name="isSelected"></param>
    /// <param name="isBezierHandle"></param>
    /// <returns></returns>
    private bool CheckControlPointVisibility(int index, out bool isSelected, out bool isBezierHandle)
    {
        isSelected = (_selectedIndex == index);
        isBezierHandle = !(index == 0 || ((index) % 3 == 0));

        if (isBezierHandle && _spline.GetHandlesAlignment(index) == BezierHandlesAlignment.Automatic)
            return false;

        if (_spline.HandlesVisibility == HandlesVisibility.ShowAllHandles)
            return true;

        if (_spline.HandlesVisibility == HandlesVisibility.ShowOnlyActiveHandles)
        {
            if (isBezierHandle)
            {
                bool selectedIsBezierHandle = !(_selectedIndex == 0 || ((_selectedIndex) % 3 == 0));
                bool isBeforeSelected = (index == (_selectedIndex - 1));
                bool isAfterSelected = (index == (_selectedIndex + 1));

                if (isSelected)
                    return true;
                else if (!selectedIsBezierHandle && (isBeforeSelected || isAfterSelected)) //Current is before or after selected
                    return true;
                else if (selectedIsBezierHandle && (_selectedIndex == index - 2 || _selectedIndex == index + 2))
                    return true;
            }
            else //Control points are always visible, only handles can be invisible
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Show spline points forward direction on Editor
    /// </summary>
    private void ShowDirections()
    {
        Handles.color = Color.red;
        Vector3 point = _spline.GetPoint(0f);
        Handles.DrawLine(point, point + _spline.GetDirection(0f) * SplineDefaultValues.DirectionScale);
        int steps = SplineDefaultValues.StepsPerCurve * _spline.CurveCount;
        for (int i = 1; i <= steps; i++)
        {
            point = _spline.GetPoint(i / (float)steps);
            Handles.DrawLine(point, point + _spline.GetDirection(i / (float)steps) * SplineDefaultValues.DirectionScale);
        }
    }

    /// <summary>
    /// Draw inspector elements and UI
    /// </summary>
    public override void OnInspectorGUI()
    {
        _spline = target as Spline;

        //Set up the box style if null
        if (_menuBoxStyle == null)
        {
            _menuBoxStyle = new GUIStyle(GUI.skin.box);
            _menuBoxStyle.normal.textColor = GUI.skin.label.normal.textColor;
            _menuBoxStyle.fontStyle = FontStyle.Bold;
            _menuBoxStyle.alignment = TextAnchor.UpperLeft;
        }

        EditorGUI.BeginChangeCheck();
        SMR_Theme theme = (SMR_Theme)EditorGUILayout.ObjectField("Theme", _spline.Theme, typeof(SMR_Theme), false);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_spline, "Changed Theme");
            MarkSceneAlteration();
            _spline.Theme = theme;
        }

        EditorGUI.BeginChangeCheck();
        _selectedMenuIndex = GUILayout.Toolbar(_selectedMenuIndex, _toolbarMenuOptions);
        if (EditorGUI.EndChangeCheck())
        {
            GUI.FocusControl(null);
        }

        GUILayout.BeginVertical(_menuBoxStyle);

        if (_selectedMenuIndex == (int)SplineInspectorMenu.CurveSettings)
        {
            #region Curve Settings

            /*
             * Curve Settings
             */
            GUILayout.Label("Curve Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            bool loop = EditorGUILayout.Toggle("Close Loop", _spline.Loop);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Toggle Loop");
                MarkSceneAlteration();
                _spline.Loop = loop;
            }

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New Curve Length");
            int newCurveLength = Mathf.Abs(EditorGUILayout.IntField((int)_spline.newCurveLength, GUILayout.MaxWidth(100)));
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Change Curve Length");
                MarkSceneAlteration();
                _spline.newCurveLength = newCurveLength < 3 ? 3 : newCurveLength;
            }

            /*
             * Curve Operations
             */
            GUILayout.Label("Curve Operations", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            if (GUILayout.Button(_btnAddNewCurve))
            {
                AddCurve();
            }

            if (GUILayout.Button(_btnTurnLeft))
            {
                TurnCurve_Left();
            }

            if (GUILayout.Button(_btnTurnUpwards))
            {
                TurnCurve_Upwards();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            if (GUILayout.Button(_btnRemoveCurve))
            {
                RemoveCurve();
            }

            if (GUILayout.Button(_btnTurnRight))
            {
                TurnCurve_Right();
            }

            if (GUILayout.Button(_btnTurnDownwards))
            {
                TurnCurve_Downwards();
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_btnResetCurve))
            {
                ResetCurve();
            }

            GUILayout.EndHorizontal();

            //Ignore first/last controlpoints and bezier handles
            using (new EditorGUI.DisabledScope(((_selectedIndex == 0) || (_selectedIndex == (_spline.ControlPointCount - 1))) || (_selectedIndex % 3 != 0)))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(_btnSubdivideCurve))
                {
                    Undo.RecordObject(_spline, "Subdivide Curve");
                    _spline.SubdivideCurve(_selectedIndex);
                    MarkSceneAlteration();
                }

                if (GUILayout.Button(_btnDissolveCurve))
                {
                    Undo.RecordObject(_spline, "Dissolve Curve");
                    _spline.DissolveCurve(_selectedIndex);
                    MarkSceneAlteration();
                }
                GUILayout.EndHorizontal();
            }
            #endregion
        }
        else if (_selectedMenuIndex == (int)SplineInspectorMenu.SplineSettings)
        {
            #region Spline Settings

            /*
             * Spline Settings
             */
            GUILayout.Label("Spline Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            bool followTerrain = EditorGUILayout.Toggle("Follow Terrain", _spline.FollowTerrain);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Follow Terrain Property");
                MarkSceneAlteration();
                _spline.FollowTerrain = followTerrain;
            }

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Terrain Check Distance");
            float terrainCheckDistance = EditorGUILayout.FloatField(_spline.TerrainCheckDistance, GUILayout.MaxWidth(100));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Terrain Check Distance");
                MarkSceneAlteration();
                _spline.TerrainCheckDistance = Mathf.Abs(terrainCheckDistance);
            }
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            SplineUpwardsDirection splineUpwardsDirection = (SplineUpwardsDirection)EditorGUILayout.EnumPopup("Custom Upwards Direction", _spline.SplineUpwardsDirection);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Custom Upwards Direction");
                MarkSceneAlteration();
                _spline.SplineUpwardsDirection = splineUpwardsDirection;
            }

            /*
             * Spline Operations
             */
            GUILayout.Label("Spline Operations", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(((_selectedIndex == 0) || (_selectedIndex == (_spline.ControlPointCount - 1))) || (_selectedIndex % 3 != 0)))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(_btnSplitSpline))
                {
                    Undo.RecordObject(_spline, "Split Spline");
                    _spline.SplitSpline(_selectedIndex);
                    MarkSceneAlteration();
                }
            }

            if (GUILayout.Button(_btnAppendSpline))
            {
                Undo.RecordObject(_spline, "Append");
                Selection.activeGameObject = _spline.AppendSpline();
                MarkSceneAlteration();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(_btnFlatten))
            {
                Undo.RecordObject(_spline, "Flatten");
                _spline.Flatten();
                MarkSceneAlteration();
            }

            if (GUILayout.Button(_btnResetRotations))
            {
                Undo.RecordObject(_spline, "Reset Rotations");
                _spline.ResetRotations();
                MarkSceneAlteration();
            }

            if (GUILayout.Button(_btnResetSpline))
            {
                Undo.RecordObject(_spline, "Reset");
                _spline.Reset();
                MarkSceneAlteration();
            }
            GUILayout.EndHorizontal();

            #endregion
        }
        else if (_selectedMenuIndex == (int)SplineInspectorMenu.HandlesSettings)
        {
            #region Handles Settings
            /*
            * Handles Settings
            */
            GUILayout.Label("Handles Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            HandlesVisibility selectedHandlesVisibility = (HandlesVisibility)EditorGUILayout.EnumPopup("Handles Visibility", _spline.HandlesVisibility);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Handle Visibility Changed");
                MarkSceneAlteration();
                _spline.HandlesVisibility = selectedHandlesVisibility;
            }

            EditorGUI.BeginChangeCheck();
            float autoHandleSpacing = EditorGUILayout.Slider("Automatic Handles Spacing", _spline.AutoHandleSpacing, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Automatic Handles Spacing");
                MarkSceneAlteration();
                _spline.AutoHandleSpacing = autoHandleSpacing;
            }

            /*
             * Selected Handle
             */
            if (_selectedIndex >= 0 && _selectedIndex < _spline.ControlPointCount)
            {
                DrawSelectedPointInspector();
            }
            else
                GUILayout.Label(string.Format("No Handle Selected{0}Click on a handle to select it", System.Environment.NewLine), EditorStyles.boldLabel);
            #endregion
        }

        GUILayout.EndVertical();
    }

    #region CURVE OPERATIONS

    /// <summary>
    /// Add new curve
    /// </summary>
    private void AddCurve()
    {
        Undo.RecordObject(_spline, "Add Curve");
        _spline.AddCurve();
        MarkSceneAlteration();
    }

    /// <summary>
    /// Remove last curve
    /// </summary>
    private void RemoveCurve()
    {
        Undo.RecordObject(_spline, "Remove Curve");
        _spline.RemoveCurve();
        MarkSceneAlteration();
    }

    /// <summary>
    /// Reset last curve
    /// </summary>
    private void ResetCurve()
    {
        Undo.RecordObject(_spline, "Reset Curve");
        _spline.ResetLastCurve();
        MarkSceneAlteration();
    }

    /// <summary>
    /// Turn curve 45º right
    /// </summary>
    private void TurnCurve_Right()
    {
        Undo.RecordObject(_spline, "Turn Curve Right");
        _spline.ShapeCurve_QuarterCircle(_splineTransform.right);
        MarkSceneAlteration();
    }

    /// <summary>
    /// Turn curve 45º left
    /// </summary>
    private void TurnCurve_Left()
    {
        Undo.RecordObject(_spline, "Turn Curve Left");
        _spline.ShapeCurve_QuarterCircle(-_splineTransform.right);
        MarkSceneAlteration();
    }

    /// <summary>
    /// Turn curve 45º upward
    /// </summary>
    private void TurnCurve_Upwards()
    {
        Undo.RecordObject(_spline, "Turn Curve Upwards");
        _spline.ShapeCurve_QuarterCircle(_splineTransform.up);
        MarkSceneAlteration();
    }

    /// <summary>
    /// Turn curve 45º downwards
    /// </summary>
    private void TurnCurve_Downwards()
    {
        Undo.RecordObject(_spline, "Turn Curve Downwards");
        _spline.ShapeCurve_QuarterCircle(-_splineTransform.up);
        MarkSceneAlteration();
    }

    #endregion

    /// <summary>
    /// Draw inpector elements form selected spline point
    /// </summary>
    private void DrawSelectedPointInspector()
    {
        GUILayout.Label("Selected Handle", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", _spline.GetControlPointPosition(_selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_spline, "Move Point");
            MarkSceneAlteration();
            _spline.SetControlPointPosition(_selectedIndex, point);
        }

        EditorGUI.BeginChangeCheck();
        Vector3 rotation = EditorGUILayout.Vector3Field("Rotation", Convert.QuaternionToVector3(_spline.GetControlPointRotation(_selectedIndex)));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_spline, "Rotate Point");
            MarkSceneAlteration();
            _spline.SetControlPointRotation(_selectedIndex, Convert.Vector3ToQuaternion(rotation));
        }

        EditorGUI.BeginChangeCheck();
        BezierHandlesAlignment handleAlignment = (BezierHandlesAlignment)EditorGUILayout.EnumPopup("Handles Alignment", _spline.GetHandlesAlignment(_selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_spline, "Change Handle Alignment");
            _spline.SetHandlesAlignment(_selectedIndex, handleAlignment, true);
            MarkSceneAlteration();
        }
    }

    /// <summary>
    /// Show player the scene needs to be saved
    /// </summary>
    private void MarkSceneAlteration()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(_spline);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}