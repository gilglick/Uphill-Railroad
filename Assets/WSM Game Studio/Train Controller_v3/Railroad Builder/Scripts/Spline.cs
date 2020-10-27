using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace WSMGameStudio.Splines
{
    public class Spline : MonoBehaviour
    {
        public float newCurveLength = 15f;

        /// <summary>
        /// Custom inspector UI Theme
        /// </summary>
        [SerializeField]
        private SMR_Theme _theme;
        /// <summary>
        /// Spline handles LOCAL positions
        /// </summary>
        [SerializeField]
        private Vector3[] _controlPointsPositions;
        /// <summary>
        /// Spline handles LOCAL rotations
        /// </summary>
        [SerializeField]
        private Quaternion[] _controlPointsRotations;
        /// <summary>
        /// Spline Handles Alignment
        /// </summary>
        [SerializeField]
        private BezierHandlesAlignment[] _modes;
        /// <summary>
        /// Evenly spaced points along spline
        /// </summary>
        [SerializeField]
        private OrientedPoint[] _orientedPoints;
        /// <summary>
        /// Merge the first and last control points creating a continuos loop
        /// </summary>
        [SerializeField]
        private bool _loop;
        /// <summary>
        /// Projects OrientedPoints on the terrain or gameobjects with collision
        /// </summary>
        [SerializeField]
        private bool _followTerrain = false;
        /// <summary>
        /// Max distance to project the spline on the terrain
        /// </summary>
        [SerializeField]
        private float _terrainCheckDistance = 20f;
        /// <summary>
        /// Handles spline resolution on auto handle aligment
        /// </summary>
        [SerializeField]
        [Range(0f, 1f)]
        private float _autoHandleSpacing = 0.33f;
        /// <summary>
        /// Handles visibility
        /// </summary>
        [SerializeField]
        private HandlesVisibility _handlesVisibility;
        /// <summary>
        /// Custom upwards direction for upside down or lateral splines
        /// </summary>
        [SerializeField]
        private Vector3 _customUpwardsDirection = Vector3.up;

        [SerializeField]
        private SplineUpwardsDirection _splineUpwardsDirection = SplineUpwardsDirection.Up;

        private Transform _transform;

        #region PROPERTIES

        /// <summary>
        /// Merge the first and last control points creating a continuos loop
        /// </summary>
        public bool Loop
        {
            get { return _loop; }
            set
            {
                if (_loop != value)
                {
                    _loop = value;
                    if (value == true)
                    {
                        _modes[_modes.Length - 1] = _modes[0];
                        SetControlPointPosition(0, _controlPointsPositions[0]);
                    }
                    else
                    {
                        ResetLastCurve();
                    }

                    UpdateAllHandlesAligment();
                }
            }
        }

        public bool FollowTerrain
        {
            get { return _followTerrain; }
            set { _followTerrain = value; }
        }

        public float TerrainCheckDistance
        {
            get { return _terrainCheckDistance; }
            set { _terrainCheckDistance = value; }
        }

        public float AutoHandleSpacing
        {
            get { return _autoHandleSpacing; }
            set
            {
                if (value != _autoHandleSpacing)
                {
                    _autoHandleSpacing = value;
                    UpdateAllHandlesAligment();
                }
            }
        }

        public HandlesVisibility HandlesVisibility
        {
            get { return _handlesVisibility; }
            set { _handlesVisibility = value; }
        }

        public Vector3 CustomUpwardsDirection
        {
            get
            {
                if (_customUpwardsDirection == Vector3.zero)
                    UpdateUpwardsDirectionVector();

                return _customUpwardsDirection;
            }
            set { _customUpwardsDirection = value; }
        }

        public SplineUpwardsDirection SplineUpwardsDirection
        {
            get { return _splineUpwardsDirection; }
            set
            {
                _splineUpwardsDirection = value;
                UpdateUpwardsDirectionVector();
            }
        }

        public int ControlPointCount
        {
            get { return _controlPointsPositions == null ? 0 : _controlPointsPositions.Length; }
        }

        public int CurveCount
        {
            get { return (_controlPointsPositions == null ? 0 : _controlPointsPositions.Length - 1) / 3; }
        }

        public float Length
        {
            get { return GetTotalDistance(); }
        }

        public OrientedPoint[] OrientedPoints
        {
            get { return _orientedPoints; }
            set { _orientedPoints = value; }
        }

        public SMR_Theme Theme
        {
            get { return _theme; }
            set { _theme = value; }
        }

        #endregion

        private void OnEnable()
        {
            _transform = GetComponent<Transform>();
        }

        /// <summary>
        /// Get control point by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3 GetControlPointPosition(int index)
        {
            if (_controlPointsPositions == null)
                Reset();

            return _controlPointsPositions[index];
        }

        /// <summary>
        /// Get rotation by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Quaternion GetControlPointRotation(int index)
        {
            return _controlPointsRotations[index];
        }

        /// <summary>
        /// Set control point rotation
        /// </summary>
        /// <param name="index"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public void SetControlPointRotation(int index, Quaternion rotation)
        {
            if (index % 3 == 0)
            {
                Quaternion deltaRotation = rotation * Quaternion.Inverse(_controlPointsRotations[index]);
                if (_loop)
                {
                    if (index == 0)
                    {
                        _controlPointsRotations[1] *= deltaRotation;
                        _controlPointsRotations[_controlPointsRotations.Length - 2] *= deltaRotation;
                        _controlPointsRotations[_controlPointsRotations.Length - 1] = rotation;
                    }
                    else if (index == _controlPointsPositions.Length - 1)
                    {
                        _controlPointsRotations[0] = rotation;
                        _controlPointsRotations[1] *= deltaRotation;
                        _controlPointsRotations[index - 1] *= deltaRotation;
                    }
                    else
                    {
                        _controlPointsRotations[index - 1] *= deltaRotation;
                        _controlPointsRotations[index + 1] *= deltaRotation;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        _controlPointsRotations[index - 1] *= deltaRotation;
                    }
                    if (index + 1 < _controlPointsRotations.Length)
                    {
                        _controlPointsRotations[index + 1] *= deltaRotation;
                    }
                }
            }

            _controlPointsRotations[index] = rotation;
        }

        /// <summary>
        /// Set control point by index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="point"></param>
        public void SetControlPointPosition(int index, Vector3 point)
        {
            if (index % 3 == 0)
            {
                Vector3 deltaPosition = point - _controlPointsPositions[index];
                if (_loop)
                {
                    if (index == 0)
                    {
                        _controlPointsPositions[1] += deltaPosition;
                        _controlPointsPositions[_controlPointsPositions.Length - 2] += deltaPosition;
                        _controlPointsPositions[_controlPointsPositions.Length - 1] = point;
                    }
                    else if (index == _controlPointsPositions.Length - 1)
                    {
                        _controlPointsPositions[0] = point;
                        _controlPointsPositions[1] += deltaPosition;
                        _controlPointsPositions[index - 1] += deltaPosition;
                    }
                    else
                    {
                        _controlPointsPositions[index - 1] += deltaPosition;
                        _controlPointsPositions[index + 1] += deltaPosition;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        _controlPointsPositions[index - 1] += deltaPosition;
                    }
                    if (index + 1 < _controlPointsPositions.Length)
                    {
                        _controlPointsPositions[index + 1] += deltaPosition;
                    }
                }
            }

            _controlPointsPositions[index] = point;

            if (index % 3 == 0)
            {
                for (int i = (index - 3); i <= (index + 3); i = i + 3)
                {
                    if (i >= 0 && i < _controlPointsPositions.Length)
                    {
                        if (_modes[i / 3] == BezierHandlesAlignment.Automatic || index == i)
                            EnforceHandleAlignment(i);
                    }
                }
            }
            else
                EnforceHandleAlignment(index);
        }

        /// <summary>
        /// Get control point mode by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public BezierHandlesAlignment GetHandlesAlignment(int index)
        {
            return _modes[(index + 1) / 3];
        }

        /// <summary>
        /// Set control point mode
        /// </summary>
        /// <param name="index"></param>
        /// <param name="handleAlignment"></param>
        public void SetHandlesAlignment(int index, BezierHandlesAlignment handleAlignment, bool enforceMode)
        {
            int modeIndex = (index + 1) / 3;
            _modes[modeIndex] = handleAlignment;
            if (_loop)
            {
                if (modeIndex == 0)
                {
                    _modes[_modes.Length - 1] = handleAlignment;
                }
                else if (modeIndex == _modes.Length - 1)
                {
                    _modes[0] = handleAlignment;
                }
            }

            if (enforceMode)
                EnforceHandleAlignment(index);
        }

        /// <summary>
        /// Update all handles aligment
        /// </summary>
        private void UpdateAllHandlesAligment()
        {
            for (int i = 0; i < _modes.Length; i++)
            {
                EnforceHandleAlignment(i * 3);
            }
        }

        /// <summary>
        /// Make sure the selected control point handles alignment mode is applied
        /// </summary>
        /// <param name="index"></param>
        private void EnforceHandleAlignment(int index)
        {
            int alignmentIndex = (index + 1) / 3;
            BezierHandlesAlignment handleAlignment = _modes[alignmentIndex];

            if (handleAlignment == BezierHandlesAlignment.Free) // Don't align if free mode is selected
                return;

            // Control point index is always in the middle of two handles
            int controlPointIndex = alignmentIndex * 3;

            if (handleAlignment == BezierHandlesAlignment.Automatic)
            {
                int previousControlPointIndex, nextControlPointIndex, previousHandle, nextHandle, lookTargetHandle;
                Vector3 direction, prevDirection, nextDirection;
                float previousNeighbourDistance, nextNeighbourDistance;

                if (controlPointIndex == 0) // First
                {
                    if (_loop)
                    {
                        previousControlPointIndex = _controlPointsPositions.Length - 3;
                        previousHandle = _controlPointsPositions.Length - 2;
                        prevDirection = (_controlPointsPositions[previousControlPointIndex] - _controlPointsPositions[controlPointIndex]).normalized;
                        previousNeighbourDistance = Vector3.Distance(_controlPointsPositions[controlPointIndex], _controlPointsPositions[previousControlPointIndex]);

                        nextControlPointIndex = controlPointIndex + 3;
                        nextHandle = controlPointIndex + 1;
                        nextDirection = (_controlPointsPositions[nextControlPointIndex] - _controlPointsPositions[controlPointIndex]).normalized;
                        nextNeighbourDistance = Vector3.Distance(_controlPointsPositions[controlPointIndex], _controlPointsPositions[nextControlPointIndex]);

                        direction = (nextDirection - prevDirection).normalized;

                        _controlPointsPositions[previousHandle] = _controlPointsPositions[controlPointIndex] - direction * previousNeighbourDistance * _autoHandleSpacing;
                        _controlPointsPositions[nextHandle] = _controlPointsPositions[controlPointIndex] + direction * nextNeighbourDistance * _autoHandleSpacing;
                    }
                    else
                    {
                        nextControlPointIndex = controlPointIndex + 3;
                        nextHandle = controlPointIndex + 1;
                        lookTargetHandle = controlPointIndex + 2;

                        direction = _controlPointsPositions[lookTargetHandle] - _controlPointsPositions[controlPointIndex];
                        direction = direction / direction.magnitude;

                        nextNeighbourDistance = Vector3.Distance(_controlPointsPositions[controlPointIndex], _controlPointsPositions[nextControlPointIndex]);

                        _controlPointsPositions[nextHandle] = _controlPointsPositions[controlPointIndex] + direction * nextNeighbourDistance * _autoHandleSpacing;
                    }
                }
                else if (controlPointIndex == _controlPointsPositions.Length - 1) // Last
                {
                    if (_loop)
                    {
                        previousControlPointIndex = _controlPointsPositions.Length - 3;
                        previousHandle = _controlPointsPositions.Length - 2;
                        prevDirection = (_controlPointsPositions[previousControlPointIndex] - _controlPointsPositions[controlPointIndex]).normalized;
                        previousNeighbourDistance = Vector3.Distance(_controlPointsPositions[controlPointIndex], _controlPointsPositions[previousControlPointIndex]);

                        nextControlPointIndex = 3;
                        nextHandle = 1;
                        nextDirection = (_controlPointsPositions[nextControlPointIndex] - _controlPointsPositions[controlPointIndex]).normalized;
                        nextNeighbourDistance = Vector3.Distance(_controlPointsPositions[controlPointIndex], _controlPointsPositions[nextControlPointIndex]);

                        direction = (nextDirection - prevDirection).normalized;

                        _controlPointsPositions[previousHandle] = _controlPointsPositions[controlPointIndex] - direction * previousNeighbourDistance * _autoHandleSpacing;
                        _controlPointsPositions[nextHandle] = _controlPointsPositions[controlPointIndex] + direction * nextNeighbourDistance * _autoHandleSpacing;
                    }
                    else
                    {
                        previousControlPointIndex = controlPointIndex - 3;
                        previousHandle = controlPointIndex - 1;
                        lookTargetHandle = controlPointIndex - 2;

                        direction = _controlPointsPositions[controlPointIndex] - _controlPointsPositions[lookTargetHandle];
                        direction = direction / direction.magnitude;

                        previousNeighbourDistance = Vector3.Distance(_controlPointsPositions[controlPointIndex], _controlPointsPositions[previousControlPointIndex]);

                        _controlPointsPositions[previousHandle] = _controlPointsPositions[controlPointIndex] - direction * previousNeighbourDistance * _autoHandleSpacing;
                    }
                }
                else // In between
                {
                    previousControlPointIndex = _loop ? LoopIndexAroundArray(_controlPointsPositions, controlPointIndex - 3) : controlPointIndex - 3;
                    previousHandle = _loop ? LoopIndexAroundArray(_controlPointsPositions, controlPointIndex - 1) : controlPointIndex - 1;
                    prevDirection = (_controlPointsPositions[previousControlPointIndex] - _controlPointsPositions[controlPointIndex]).normalized;
                    previousNeighbourDistance = Vector3.Distance(_controlPointsPositions[controlPointIndex], _controlPointsPositions[previousControlPointIndex]);

                    nextControlPointIndex = _loop ? LoopIndexAroundArray(_controlPointsPositions, controlPointIndex + 3) : controlPointIndex + 3;
                    nextHandle = _loop ? LoopIndexAroundArray(_controlPointsPositions, controlPointIndex + 1) : controlPointIndex + 1;
                    nextDirection = (_controlPointsPositions[nextControlPointIndex] - _controlPointsPositions[controlPointIndex]).normalized;
                    nextNeighbourDistance = Vector3.Distance(_controlPointsPositions[controlPointIndex], _controlPointsPositions[nextControlPointIndex]);

                    direction = (nextDirection - prevDirection).normalized;

                    _controlPointsPositions[previousHandle] = _controlPointsPositions[controlPointIndex] - direction * previousNeighbourDistance * _autoHandleSpacing;
                    _controlPointsPositions[nextHandle] = _controlPointsPositions[controlPointIndex] + direction * nextNeighbourDistance * _autoHandleSpacing;
                }
            }
            else // Enforce Aligned and Mirrored modes
            {
                // Don't align if it is start or end of non-loop spline
                if (!_loop && (alignmentIndex == 0 || alignmentIndex == _modes.Length - 1))
                    return;

                int fixedHandleIndex, enforcedHandleIndex;

                //Verifying which handle should be fixed and which should be enforced
                if (index <= controlPointIndex)
                {
                    fixedHandleIndex = controlPointIndex - 1;
                    if (fixedHandleIndex < 0)
                        fixedHandleIndex = _controlPointsPositions.Length - 2;

                    enforcedHandleIndex = controlPointIndex + 1;
                    if (enforcedHandleIndex >= _controlPointsPositions.Length)
                        enforcedHandleIndex = 1;
                }
                else
                {
                    fixedHandleIndex = controlPointIndex + 1;
                    if (fixedHandleIndex >= _controlPointsPositions.Length)
                        fixedHandleIndex = 1;

                    enforcedHandleIndex = controlPointIndex - 1;
                    if (enforcedHandleIndex < 0)
                        enforcedHandleIndex = _controlPointsPositions.Length - 2;
                }

                Vector3 middle = _controlPointsPositions[controlPointIndex];
                Vector3 enforcedTangent = middle - _controlPointsPositions[fixedHandleIndex];

                if (handleAlignment == BezierHandlesAlignment.Aligned)
                {
                    enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, _controlPointsPositions[enforcedHandleIndex]);
                }

                _controlPointsPositions[enforcedHandleIndex] = middle + enforcedTangent;
            }
        }

        /// <summary>
        /// Loop index around array if out of bounds
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private int LoopIndexAroundArray<T>(T[] array, int index)
        {
            return (index + array.Length) % array.Length;
        }

        /// <summary>
        /// Get point
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 GetPoint(float t)
        {
            int curveStartIndex;
            if (t >= 1f)
            {
                t = 1f;
                curveStartIndex = _controlPointsPositions.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * CurveCount;
                curveStartIndex = (int)t;
                t -= curveStartIndex;
                curveStartIndex *= 3;
            }
            return transform.TransformPoint(Bezier.GetPoint(
                _controlPointsPositions[curveStartIndex], _controlPointsPositions[curveStartIndex + 1], _controlPointsPositions[curveStartIndex + 2], _controlPointsPositions[curveStartIndex + 3], t));
        }

        /// <summary>
        /// Return the index of the closest oriented point
        /// </summary>
        /// <param name="t">Percentual interpolotion value</param>
        /// <returns></returns>
        public int GetClosestOrientedPointIndex(float t)
        {
            int closest = 0;
            t = Mathf.Clamp01(t);

            if (_orientedPoints == null)
                return closest;

            //closest = (t == 0f) ? 0 : (int)(_orientedPoints.Length * t);
            closest = (int)((_orientedPoints.Length - 1) * t);

            return closest;
        }

        /// <summary>
        /// Get point on segment
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            return Bezier.GetPoint(p0, p1, p2, p3, t);
        }

        /// <summary>
        /// Get point rotation at spline postion t
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Quaternion GetRotation(float t)
        {
            int curveStartIndex;
            if (t >= 1f)
            {
                t = 1f;
                curveStartIndex = _controlPointsRotations.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * CurveCount;
                curveStartIndex = (int)t;
                t -= curveStartIndex;
                curveStartIndex *= 3;
            }

            return Bezier.GetPointRotation(_controlPointsRotations[curveStartIndex], _controlPointsRotations[curveStartIndex + 1], _controlPointsRotations[curveStartIndex + 2], _controlPointsRotations[curveStartIndex + 3], t);
        }

        /// <summary>
        /// Get velocity
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 GetVelocity(float t)
        {
            int curveStartIndex;
            if (t >= 1f)
            {
                t = 1f;
                curveStartIndex = _controlPointsPositions.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * CurveCount;
                curveStartIndex = (int)t;
                t -= curveStartIndex;
                curveStartIndex *= 3;
            }

            return GetVelocity(_controlPointsPositions[curveStartIndex], _controlPointsPositions[curveStartIndex + 1], _controlPointsPositions[curveStartIndex + 2], _controlPointsPositions[curveStartIndex + 3], t);
        }

        /// <summary>
        /// Get velocity on spline segment
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 GetVelocity(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            Vector3 firstDerivative = Bezier.GetFirstDerivative(p0, p1, p2, p3, t);
            return transform.TransformPoint(firstDerivative) - transform.position;
        }

        /// <summary>
        /// Get direction
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 GetDirection(float t)
        {
            return GetVelocity(t).normalized;
        }

        /// <summary>
        /// Get direction on spline segment
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 GetDirection(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            return GetVelocity(p0, p1, p2, p3, t).normalized;
        }

        /// <summary>
        /// Get a list of spline oriented points based on the number os steps
        /// </summary>
        /// <param name="steps"></param>
        /// <returns></returns>
        public List<OrientedPoint> GetOrientedPoints(int steps)
        {
            List<OrientedPoint> ret = new List<OrientedPoint>();

            float stepPercentage = 1f / steps;
            float t = 0;

            while (t < 1f)
            {
                OrientedPoint orientedPoint = GetOrientedPoint(t);
                ret.Add(orientedPoint);
                t += stepPercentage;
            }

            return ret;
        }

        /// <summary>
        /// Get point position and rotation on spline position t
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public OrientedPoint GetOrientedPoint(float t)
        {
            Vector3 position = GetPoint(t);
            Quaternion rotation = GetRotation(t);
            Vector3 direction = GetDirection(t);

            return new OrientedPoint(position, rotation * Quaternion.LookRotation(direction, _customUpwardsDirection));
        }

        /// <summary>
        /// Get position and rotation on a spline segment
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public OrientedPoint GetOrientedPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Quaternion r0, Quaternion r1, Quaternion r2, Quaternion r3, float t)
        {
            Vector3 position = GetPoint(p0, p1, p2, p3, t);
            Quaternion rotation = Bezier.GetPointRotation(r0, r1, r2, r3, t);
            Vector3 direction = GetDirection(p0, p1, p2, p3, t);

            rotation = rotation * Quaternion.LookRotation(direction, _customUpwardsDirection);

            return new OrientedPoint(position, rotation);
        }

        /// <summary>
        /// Reset spline
        /// </summary>
        public void Reset()
        {
            _loop = false;

            _splineUpwardsDirection = SplineUpwardsDirection.Up;
            _customUpwardsDirection = Vector3.up;
            _controlPointsPositions = new Vector3[4];
            _controlPointsRotations = new Quaternion[4];

            for (int i = 0; i < _controlPointsPositions.Length; i++)
            {
                _controlPointsPositions[i] = new Vector3(0f, 0f, i * (newCurveLength / 3));
                _controlPointsRotations[i] = Quaternion.identity;
            }

            _modes = new BezierHandlesAlignment[]
            {
                BezierHandlesAlignment.Aligned,
                BezierHandlesAlignment.Aligned
            };
        }

        /// <summary>
        /// Reset all control points rotations
        /// </summary>
        public void ResetRotations()
        {
            ResetRotations(Quaternion.identity);
        }

        /// <summary>
        /// Set all control points rotations to new rotation value
        /// </summary>
        public void ResetRotations(Quaternion newRotation)
        {
            for (int i = 0; i < _controlPointsRotations.Length; i++)
            {
                _controlPointsRotations[i] = newRotation;
            }
        }

        /// <summary>
        /// Add new curve to spline
        /// </summary>
        public void AddCurve()
        {
            if (_loop)
                _loop = false;

            //Add positions
            Vector3 lastPointPosition = _controlPointsPositions[_controlPointsPositions.Length - 1];
            Vector3 lastPointDirection = transform.InverseTransformDirection(GetDirection(1)); // Last point direction
            Quaternion lastPointRotation = GetRotation(1); // Last point rotation

            Array.Resize(ref _controlPointsPositions, _controlPointsPositions.Length + 3);
            Array.Resize(ref _controlPointsRotations, _controlPointsRotations.Length + 3);

            float positionOffset = (newCurveLength / 3);
            //Add the 3 new control points
            for (int i = 3; i > 0; i--)
            {
                //Calculate new position based on last point direction
                lastPointPosition += (lastPointDirection * positionOffset);
                //Position
                _controlPointsPositions[_controlPointsPositions.Length - i] = lastPointPosition;
                //Rotation
                _controlPointsRotations[_controlPointsRotations.Length - i] = lastPointRotation;
            }

            //Add modes
            Array.Resize(ref _modes, _modes.Length + 1);
            _modes[_modes.Length - 1] = _modes[_modes.Length - 2];
            EnforceHandleAlignment(_controlPointsPositions.Length - 4);

            if (_loop)
            {
                _controlPointsPositions[_controlPointsPositions.Length - 1] = _controlPointsPositions[0];
                _controlPointsRotations[_controlPointsRotations.Length - 1] = _controlPointsRotations[0];
                _modes[_modes.Length - 1] = _modes[0];
                EnforceHandleAlignment(0);
            }
        }

        /// <summary>
        /// Remove the last curve (Disables loop property)
        /// </summary>
        public void RemoveCurve()
        {
            if (CurveCount <= 1)
            {
                Debug.Log("Spline has only one curve. Cannot remove last curve.");
                return;
            }

            _loop = false;

            Array.Resize(ref _controlPointsPositions, _controlPointsPositions.Length - 3);
            Array.Resize(ref _controlPointsRotations, _controlPointsRotations.Length - 3);
            Array.Resize(ref _modes, _modes.Length - 1);
        }

        /// <summary>
        /// Turns the last curve on a quarter of a circle
        /// The total length of the curve equals the half-circle's perimeter
        /// </summary>
        /// <param name="direction"></param>
        public void ShapeCurve_QuarterCircle(Vector3 direction)
        {
            //ResetLastCurve();
            if (_transform == null)
                _transform = GetComponent<Transform>();

            _loop = false;

            // Disable automatic aligment to avoid shape deformation
            if (_modes[_modes.Length - 1] == BezierHandlesAlignment.Automatic)
                _modes[_modes.Length - 1] = BezierHandlesAlignment.Aligned;

            float inverseOfPi = 0.3183f; //1f / Mathf.PI;
            float radius = 2 * newCurveLength * inverseOfPi;

            int curveStartIndex = _controlPointsPositions.Length - 4;
            Vector3 startPointPosition = _controlPointsPositions[curveStartIndex];
            Vector3 startPointDirection = GetCurveDirection(curveStartIndex);

            Vector3 startPoint_Right = (Quaternion.LookRotation(startPointDirection, _customUpwardsDirection) * Vector3.right);
            Vector3 startPoint_Up = (Quaternion.LookRotation(startPointDirection, _customUpwardsDirection) * Vector3.up);

            Vector3 centerDirection = Vector3.zero;

            if (direction == _transform.right) // Curve to the right
                centerDirection = startPoint_Right;
            else if (direction == -_transform.right) // Curve to the left
                centerDirection = -startPoint_Right;
            else if (direction == _transform.up) // Curve up
                centerDirection = startPoint_Up;
            else if (direction == -_transform.up) // Curve down
                centerDirection = -startPoint_Up;

            // startPoint, circleCenter, endpoint and auxPoint forms a perfect SQUARE with side length equals to the circle radius
            Vector3 circleCenterPosition = startPointPosition + (centerDirection * radius);
            Vector3 endPointPosition = circleCenterPosition + (startPointDirection * radius);
            Vector3 auxPointPosition = startPointPosition + (startPointDirection * radius); ;

            Vector3 handle1Position = Vector3.Lerp(startPointPosition, auxPointPosition, 0.55f); // 55% distance between start and aux
            Vector3 handle2Position = Vector3.Lerp(auxPointPosition, endPointPosition, 0.45f); // 45% distance between aux and end

            // Apply new positions
            _controlPointsPositions[curveStartIndex] = startPointPosition;
            _controlPointsPositions[curveStartIndex + 1] = handle1Position;
            _controlPointsPositions[curveStartIndex + 2] = handle2Position;
            _controlPointsPositions[curveStartIndex + 3] = endPointPosition;
        }

        /// <summary>
        /// Reset last curve
        /// </summary>
        public void ResetLastCurve()
        {
            _loop = false;

            int curveStartIndex = _controlPointsPositions.Length - 4;
            Vector3 startPointPosition = _controlPointsPositions[curveStartIndex];
            Vector3 startPointDirection = GetCurveDirection(curveStartIndex);
            Quaternion startPointRotation = GetCurveRotation(curveStartIndex);

            float positionOffset = (newCurveLength / 3);

            //Reset control points positions and rotations
            for (int i = 3; i > 0; i--)
            {
                //Calculate new position based on last point direction
                startPointPosition += (startPointDirection * positionOffset);
                //Position
                _controlPointsPositions[_controlPointsPositions.Length - i] = startPointPosition;
                //Rotation
                _controlPointsRotations[_controlPointsRotations.Length - i] = startPointRotation;
            }
        }

        /// <summary>
        /// Get curve diretion at the start of the curve
        /// </summary>
        /// <param name="curveStartIndex"></param>
        /// <returns></returns>
        private Vector3 GetCurveDirection(int curveStartIndex)
        {
            return transform.InverseTransformDirection(GetDirection(_controlPointsPositions[curveStartIndex], _controlPointsPositions[curveStartIndex + 1], _controlPointsPositions[curveStartIndex + 2], _controlPointsPositions[curveStartIndex + 3], 0f));
        }

        /// <summary>
        /// Get curve rotation at the start of the curve
        /// </summary>
        /// <param name="curveStartIndex"></param>
        /// <returns></returns>
        private Quaternion GetCurveRotation(int curveStartIndex)
        {
            return Bezier.GetPointRotation(_controlPointsRotations[curveStartIndex], _controlPointsRotations[curveStartIndex + 1], _controlPointsRotations[curveStartIndex + 2], _controlPointsRotations[curveStartIndex + 3], 0f);
        }

        /// <summary>
        /// Disable colliders and save their previous state
        /// </summary>
        /// <param name="colliders"></param>
        /// <param name="collidersState"></param>
        private static bool[] DisableColliders(MeshCollider[] colliders)
        {
            bool[] collidersState = new bool[colliders.Length];

            for (int i = 0; i < colliders.Length; i++)
            {
                collidersState[i] = colliders[i].enabled;
                colliders[i].enabled = false;
            }

            return collidersState;
        }

        /// <summary>
        /// Reenable colliders
        /// </summary>
        /// <param name="colliders"></param>
        /// <param name="collidersState"></param>
        private static void RenableColliders(MeshCollider[] colliders, bool[] collidersState)
        {
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = collidersState[i];
        }

        /// <summary>
        /// Disable colliders and save their active previous state
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        private static bool[] DisableIgnoredObjects(SMR_IgnoredObject[] objects)
        {
            bool[] objectsState = new bool[objects.Length];

            for (int i = 0; i < objects.Length; i++)
            {
                objectsState[i] = objects[i].gameObject.activeInHierarchy;
                objects[i].gameObject.SetActive(false);
            }

            return objectsState;
        }

        /// <summary>
        /// Reenable objects that were enabled before
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="objectsState"></param>
        private static void RenableIgnoredObjects(SMR_IgnoredObject[] objects, bool[] objectsState)
        {
            for (int i = 0; i < objects.Length; i++)
                objects[i].gameObject.SetActive(objectsState[i]);
        }

        /// <summary>
        /// Reset control points heights
        /// </summary>
        public void Flatten()
        {
            Vector3 firstPointPosition = GetControlPointPosition(0);

            for (int i = 0; i < _controlPointsPositions.Length; i += 3)
            {
                SetControlPointPosition(i, new Vector3(_controlPointsPositions[i].x, firstPointPosition.y, _controlPointsPositions[i].z));
            }

            //Adjust auxiliar control points
            for (int i = 0; i < _controlPointsPositions.Length; i++)
            {
                if (i % 3 == 0)
                    continue;

                SetControlPointPosition(i, new Vector3(_controlPointsPositions[i].x, firstPointPosition.y, _controlPointsPositions[i].z));
            }

            UpdateMeshRenderer();
        }

        /// <summary>
        /// Updates Spline Mesh renderer (if exists)
        /// </summary>
        private void UpdateMeshRenderer()
        {
            SplineMeshRenderer splineMeshRenderer = GetComponent<SplineMeshRenderer>();

            if (splineMeshRenderer != null)
                splineMeshRenderer.ExtrudeMesh();
        }

        /// <summary>
        /// Return bezier curve aproximated Length in meters
        /// </summary>
        /// <param name="realDistance"></param>
        /// <returns></returns>
        public float GetTotalDistance(bool realDistance = false)
        {
            float length = 0f;

            // Calculate real length if oriented points were already calculated
            if (realDistance && _orientedPoints != null)
            {
                int penultimateIndex = (_orientedPoints.Length - 1);
                for (int i = 0; i < penultimateIndex; i++)
                    length += Vector3.Distance(_orientedPoints[i].Position, _orientedPoints[i + 1].Position);
            }
            else // Calculate aproximated length
            {
                for (float t = 0f; t < 1f; t += 0.1f)
                    length += Vector3.Distance(GetPoint(t), GetPoint(t + 0.1f));
            }

            return length;
        }

        /// <summary>
        /// Split spline in two starting from selected control point
        /// </summary>
        /// <param name="selectedIndex"></param>
        public void SplitSpline(int selectedIndex)
        {
            _loop = false; // Disable loop on Split

            //Creating new spline control points
            int newSplineLength = _controlPointsPositions.Length - selectedIndex;
            int newModesLength = (newSplineLength / 3) + 1;
            Vector3[] newControlPointsPositions = new Vector3[newSplineLength];
            Quaternion[] newControlPointsRotations = new Quaternion[newSplineLength];
            BezierHandlesAlignment[] newModes = new BezierHandlesAlignment[newModesLength];

            //Populating new spline control points
            int newModesStart = (selectedIndex / 3);
            Array.Copy(_controlPointsPositions, selectedIndex, newControlPointsPositions, 0, newSplineLength);
            Array.Copy(_controlPointsRotations, selectedIndex, newControlPointsRotations, 0, newSplineLength);
            Array.Copy(_modes, newModesStart, newModes, 0, newModesLength);

            //Creating new spline game object
            GameObject newSplineGameObject = Instantiate(this.gameObject);
            Spline newSpline = newSplineGameObject.GetComponent<Spline>();
            Transform newSplinePosition = newSplineGameObject.GetComponent<Transform>();
            newSpline._controlPointsPositions = newControlPointsPositions;
            newSpline._controlPointsRotations = newControlPointsRotations;
            newSpline._modes = newModes;

            //Adjusting new spline world position and pivot
            Vector3 firstControlPoint = newSpline._controlPointsPositions[0];
            newSplinePosition.position = transform.TransformPoint(firstControlPoint);
            newSplinePosition.rotation = newSpline._controlPointsRotations[0];
            for (int i = 0; i < newSpline._controlPointsPositions.Length; i++)
            {
                newSpline._controlPointsPositions[i] -= firstControlPoint;
            }

            //Removing control points from older spline
            int resizeLength = selectedIndex + 1;
            int modeResizeLength = newModesStart + 1;
            Array.Resize(ref _controlPointsPositions, resizeLength);
            Array.Resize(ref _controlPointsRotations, resizeLength);
            Array.Resize(ref _modes, modeResizeLength);

            UpdateMeshRenderer();
            newSpline.UpdateMeshRenderer();

#if UNITY_EDITOR
            //Select new spline createdon Editor
            UnityEditor.Selection.activeGameObject = newSplineGameObject;
#endif

            Debug.Log("Spline Splitted Successfully!");
        }

        /// <summary>
        /// Insert a new curve between the current selected curve and the next one
        /// </summary>
        /// <param name="selectedIndex"></param>
        /// <returns>new selectedIndex</returns>
        public void SubdivideCurve(int selectedIndex)
        {
            //Temp lists for inserting operation
            List<Vector3> newPositions = new List<Vector3>(_controlPointsPositions);
            List<Quaternion> newRotations = new List<Quaternion>(_controlPointsRotations);
            List<BezierHandlesAlignment> newModes = new List<BezierHandlesAlignment>(_modes);

            // Curve segment information
            int insertIndex = selectedIndex + 2;
            int insertModeIndex = (selectedIndex / 3) + 1;
            Vector3 currentPosition = _controlPointsPositions[selectedIndex];
            Vector3 nextPosition = _controlPointsPositions[selectedIndex + 3];
            Quaternion currentRotation = _controlPointsRotations[selectedIndex];
            Quaternion nextRotation = _controlPointsRotations[selectedIndex + 3];

            Vector3 curveLinearDirection = (nextPosition - currentPosition).normalized;
            float handlesDistance = (nextPosition - currentPosition).magnitude * 0.1515f;

            // Creating curve and handles positions
            Vector3 newPosition = GetPoint(_controlPointsPositions[selectedIndex], _controlPointsPositions[selectedIndex + 1], _controlPointsPositions[selectedIndex + 2], _controlPointsPositions[selectedIndex + 3], 0.5f);
            Vector3 previousHandlePosition = newPosition + (-curveLinearDirection * handlesDistance);
            Vector3 afterHandlePosition = newPosition + (curveLinearDirection * handlesDistance);

            // Curve and handle rotations
            Quaternion newRotation = Quaternion.Lerp(currentRotation, nextRotation, 0.5f);
            Quaternion previousHandleRotation = Quaternion.Lerp(currentRotation, newRotation, 0.66f);
            Quaternion afterHandleRotation = Quaternion.Lerp(newRotation, nextRotation, 0.33f);
            // Curve handle Mode
            BezierHandlesAlignment newMode = _modes[selectedIndex / 3];

            // Inserting positions
            newPositions.Insert(insertIndex, afterHandlePosition);
            newPositions.Insert(insertIndex, newPosition);
            newPositions.Insert(insertIndex, previousHandlePosition);
            // Inserting rotations
            newRotations.Insert(insertIndex, afterHandleRotation);
            newRotations.Insert(insertIndex, nextRotation);
            newRotations.Insert(insertIndex, previousHandleRotation);
            // Insert mode
            newModes.Insert(insertModeIndex, newMode);

            //Applying New Control Point Insertion
            _controlPointsPositions = newPositions.ToArray();
            _controlPointsRotations = newRotations.ToArray();
            _modes = newModes.ToArray();

            Debug.Log("Curve Subdivide Successfully!");
        }

        /// <summary>
        /// Dissolve selected curve
        /// </summary>
        /// <param name="selectedIndex"></param>
        public void DissolveCurve(int selectedIndex)
        {
            int removeStartIndex = selectedIndex - 1; //remove previous handle, control point and next handle
            int modeToRemove = (selectedIndex / 3);

            //Temp lists for removal
            List<Vector3> newPositions = new List<Vector3>(_controlPointsPositions);
            List<Quaternion> newRotations = new List<Quaternion>(_controlPointsRotations);
            List<BezierHandlesAlignment> newModes = new List<BezierHandlesAlignment>(_modes);

            //Control Point removal
            newPositions.RemoveRange(removeStartIndex, 3);
            newRotations.RemoveRange(removeStartIndex, 3);
            newModes.RemoveAt(modeToRemove);

            //Applying Control Point removal
            _controlPointsPositions = newPositions.ToArray();
            _controlPointsRotations = newRotations.ToArray();
            _modes = newModes.ToArray();

            Debug.Log("Curve Dissolved Successfully!");
        }

        /// <summary>
        /// Populates the OrientedPoints array with a list of evenly spaced points along the spline.
        /// Spacing based on the base mesh length
        /// </summary>
        /// <param name="spacing"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public void CalculateOrientedPoints(float spacing, float resolution = 1)
        {
            Profiler.BeginSample("CalculateOrientedPoints");

            #region CALCULATING SEGMENTS SPACING
            float distanceSinceLastEvenPoint = 0;
            List<OrientedPoint> tempOrientedPoints = new List<OrientedPoint>();

            OrientedPoint start, handle1, handle2, end, interpolationPoint;

            OrientedPoint previousPoint = GetOrientedPoint(0f); // Get start of spline
            previousPoint.Position = transform.InverseTransformPoint(previousPoint.Position);

            //Adding spline start point
            tempOrientedPoints.Add(previousPoint);

            int lastCurveIndex = ControlPointCount - 3;
            for (int curveIndex = 0; curveIndex < lastCurveIndex; curveIndex += 3)
            {
                start = new OrientedPoint(_controlPointsPositions[curveIndex], _controlPointsRotations[curveIndex]);
                handle1 = new OrientedPoint(_controlPointsPositions[curveIndex + 1], _controlPointsRotations[curveIndex + 1]);
                handle2 = new OrientedPoint(_controlPointsPositions[curveIndex + 2], _controlPointsRotations[curveIndex + 2]);
                end = new OrientedPoint(_controlPointsPositions[curveIndex + 3], _controlPointsRotations[curveIndex + 3]);

                float controlNetLength = Vector3.Distance(start.Position, handle1.Position) + Vector3.Distance(handle1.Position, handle2.Position) + Vector3.Distance(handle2.Position, end.Position);
                float estimatedCurveLength = Vector3.Distance(start.Position, end.Position) + controlNetLength / 2f;
                int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);

                float timeSteps = (1f / divisions);

                float t = 0;
                while (t <= 1)
                {
                    t += timeSteps;

                    interpolationPoint = GetOrientedPoint(start.Position, handle1.Position, handle2.Position, end.Position, start.Rotation, handle1.Rotation, handle2.Rotation, end.Rotation, t);

                    distanceSinceLastEvenPoint += Vector3.Distance(previousPoint.Position, interpolationPoint.Position);

                    while (distanceSinceLastEvenPoint >= spacing)
                    {
                        float exceededDistance = distanceSinceLastEvenPoint - spacing;
                        OrientedPoint newEvenlySpacedPoint = previousPoint;
                        newEvenlySpacedPoint.Position = interpolationPoint.Position + (previousPoint.Position - interpolationPoint.Position).normalized * exceededDistance;
                        newEvenlySpacedPoint.Rotation = interpolationPoint.Rotation;

                        tempOrientedPoints.Add(newEvenlySpacedPoint);
                        distanceSinceLastEvenPoint = exceededDistance;
                        previousPoint = newEvenlySpacedPoint;
                    }

                    previousPoint = interpolationPoint;
                }
            }

            int endPointIndex = _controlPointsPositions.Length - 1;
            int lastEvenlySpacedPointIndex = tempOrientedPoints.Count - 1;
            float lastPointAndSplineEndDistance = Mathf.Abs(Vector3.Distance(tempOrientedPoints[lastEvenlySpacedPointIndex].Position, _controlPointsPositions[endPointIndex]));

            interpolationPoint = GetOrientedPoint(1f); // Get end of spline
            interpolationPoint.Position = transform.InverseTransformPoint(interpolationPoint.Position);

            if (lastPointAndSplineEndDistance <= (spacing * 0.5f))
                tempOrientedPoints[lastEvenlySpacedPointIndex] = interpolationPoint;
            else
                tempOrientedPoints.Add(interpolationPoint);
            #endregion

            #region DISABLING UNWANTED COLLIDERS
            //Disable custom colliders
            SplineMeshRenderer splineMeshRenderer = GetComponent<SplineMeshRenderer>();
            MeshCollider[] customColliders = null;
            bool[] customCollidersState = null;
            if (splineMeshRenderer != null && splineMeshRenderer.CustomMeshColliders.Length > 0)
            {
                int customCollidersCount = splineMeshRenderer.CustomMeshColliders.Length;
                customColliders = new MeshCollider[customCollidersCount];
                customCollidersState = new bool[customCollidersCount];

                for (int i = 0; i < customCollidersCount; i++)
                    customColliders[i] = splineMeshRenderer.CustomMeshColliders[i].GetComponent<MeshCollider>();

                customCollidersState = DisableColliders(customColliders);
            }

            //Disable colliders
            MeshCollider[] colliders = GetComponentsInChildren<MeshCollider>();
            bool[] collidersState = DisableColliders(colliders);

            //Ignored objects
            SMR_IgnoredObject[] ignoredObjects = GameObject.FindObjectsOfType<SMR_IgnoredObject>();
            bool[] ignoredObjectsState = DisableIgnoredObjects(ignoredObjects);
            #endregion

            #region COLLISION DETECTION
            RaycastHit hit = new RaycastHit();

            //Recalculating positions to world space
            Vector3 worldSpacePos;
            for (int i = 0; i < tempOrientedPoints.Count; i++)
            {
                worldSpacePos = transform.TransformPoint(tempOrientedPoints[i].Position);

                if (_followTerrain)
                    worldSpacePos = TerrainCollision(worldSpacePos, hit);

                tempOrientedPoints[i] = new OrientedPoint(worldSpacePos, tempOrientedPoints[i].Rotation);
            }

            _orientedPoints = tempOrientedPoints.ToArray();
            tempOrientedPoints.Clear();
            #endregion

            #region RENABLING UNWANTED COLLIDERS
            //Renable colliders
            RenableColliders(colliders, collidersState);
            //Renable custom colliders
            if (splineMeshRenderer != null && splineMeshRenderer.HasCustomColliders)
                RenableColliders(customColliders, customCollidersState);
            //Renable ignored objects
            RenableIgnoredObjects(ignoredObjects, ignoredObjectsState);
            #endregion

            Profiler.EndSample();
        }

        /// <summary>
        /// Follow terrain feature collision detection
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        [ExecuteInEditMode]
        private Vector3 TerrainCollision(Vector3 origin, RaycastHit hit)
        {
            Profiler.BeginSample("Follow Terrain Collision");

            if (Physics.Raycast(origin, Vector3.down, out hit, _terrainCheckDistance)) //, ~0, QueryTriggerInteraction.Ignore))
                return hit.point;
            else if (Physics.Raycast(origin, Vector3.up, out hit, _terrainCheckDistance)) //, ~0, QueryTriggerInteraction.Ignore))
                return hit.point;

            Profiler.EndSample();

            return origin;
        }

        /// <summary>
        /// Create new spline at the end of the current one
        /// </summary>
        public GameObject AppendSpline()
        {
            Vector3 lastPointPosition = this.GetControlPointPosition(this.ControlPointCount - 1);
            Quaternion lastPointRotation = this.GetRotation(1);

            Vector3 position = transform.TransformPoint(lastPointPosition);
            GameObject clone = Instantiate(this.gameObject, position, this.GetRotation(1) * Quaternion.LookRotation(this.GetDirection(1)));

            Spline newRendererSpline = clone.GetComponent<Spline>();
            newRendererSpline.Reset();
            newRendererSpline.ResetRotations(lastPointRotation);

            SplineMeshRenderer newRendererSplineMeshRenderer = clone.GetComponent<SplineMeshRenderer>();
            if (newRendererSplineMeshRenderer != null)
            {
                SplineMeshRenderer splineMeshRenderer = this.GetComponent<SplineMeshRenderer>();

                newRendererSplineMeshRenderer.MeshGenerationMethod = splineMeshRenderer.MeshGenerationMethod;
                newRendererSplineMeshRenderer.ExtrudeMesh();
            }

            return clone;
        }

        /// <summary>
        /// Updates custom upwards direction vector based splineUpwardsDirection enum
        /// </summary>
        private void UpdateUpwardsDirectionVector()
        {
            switch (_splineUpwardsDirection)
            {
                case SplineUpwardsDirection.Up:
                    _customUpwardsDirection = Vector3.up;
                    break;
                case SplineUpwardsDirection.Down:
                    _customUpwardsDirection = -Vector3.up;
                    break;
                case SplineUpwardsDirection.Right:
                    _customUpwardsDirection = Vector3.right;
                    break;
                case SplineUpwardsDirection.Left:
                    _customUpwardsDirection = -Vector3.right;
                    break;
                case SplineUpwardsDirection.Foward:
                    _customUpwardsDirection = Vector3.forward;
                    break;
                case SplineUpwardsDirection.Back:
                    _customUpwardsDirection = -Vector3.forward;
                    break;
            }
        }
    }
}
