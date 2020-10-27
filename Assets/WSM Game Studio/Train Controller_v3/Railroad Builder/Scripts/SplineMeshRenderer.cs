using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Profiling;
using System.Timers;

namespace WSMGameStudio.Splines
{
    [System.Serializable]
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class SplineMeshRenderer : UniqueMesh
    {
        #region PUBLIC VARIABLES
        //Mesh generation parameters
        [SerializeField]
        [Tooltip("Guide spline")]
        private Spline _spline;
        [SerializeField]
        [Tooltip("Mesh generation method (EDITOR ONLY)")]
        private MeshGenerationMethod _meshGenerationMethod;
        [SerializeField]
        [Tooltip("Mesh that will be rendered along the spline")]
        private Mesh _baseMesh;
        [SerializeField]
        [Tooltip("(Optional) Mesh offset from spline")]
        private Vector3 _meshOffset;
        [SerializeField]
        [Tooltip("Enable collision for generated mesh")]
        private bool _enableCollision = false;
        [SerializeField]
        [Tooltip("(Optional) Custom mesh colliders generated using the same spline")]
        private SplineMeshRenderer[] _customMeshColliders;
        #endregion

        #region PRIVATE VARIABLES
        private float _splineLength;
        private Transform _auxTransform1;
        private Transform _auxTransform2;
        private MeshCollider _meshCollider;
        //Base mesh scanned values
        private float _baseMeshLength;
        private float _baseMeshMinZ;
        private float _baseMeshMaxZ;
        private Vector3 _verticeScanned = new Vector3();
        private int _baseMesh_VertexCount;
        private int _baseMesh_SubMeshCount;
        private Vector3[] _baseMesh_vertices;
        private Vector3[] _baseMesh_normals;
        private Vector4[] _baseMesh_tangents;
        private Vector2[] _baseMesh_uv;
        //Add Mesh Segment
        private int[] _segmentIndices;
        //Create Triangle
        private Vector3[] _triangleVertices;
        private Vector3[] _triangleNormals;
        private Vector2[] _triangleUvs;
        private Vector4[] _triangleTangents;
        private Vector3 _triangleVerticesStart;
        private Vector3 _triangleVerticesEnd;
        private Vector3 _triangleNormalsStart;
        private Vector3 _triangleNormalsEnd;
        private Vector4 _triangleTangentsStart;
        private Vector4 _triangleTangentsEnd;
        private Matrix4x4 _triangleLocalToWorld_Start;
        private Matrix4x4 _triangleLocalToWorld_End;
        private Matrix4x4 _triangleWorldToLocal;
        //Mesh generations values
        private List<Vector3> _vertices = new List<Vector3>();
        private List<Vector3> _normals = new List<Vector3>();
        private List<Vector2> _uvs = new List<Vector2>();
        private List<Vector4> _tangents = new List<Vector4>();
        private List<List<int>> _subMeshTriangles = new List<List<int>>();
        //Colliders
        private bool _isCustomCollider = false;
        #endregion

        #region PROPERTIES
        public Mesh GeneratedMesh
        {
            get { return mesh; }
        }

        public Spline Spline
        {
            get { return _spline; }
            set
            {
                _spline = value;
                ExtrudeMesh();
            }
        }

        public MeshGenerationMethod MeshGenerationMethod
        {
            get { return _meshGenerationMethod; }
            set { _meshGenerationMethod = value; }
        }

        public Mesh BaseMesh
        {
            get { return _baseMesh; }
            set
            {
                _baseMesh = value;
                ExtrudeMesh();
            }
        }

        public Vector3 MeshOffset
        {
            get { return _meshOffset; }
            set { _meshOffset = value; }
        }

        public bool EnableCollision
        {
            get { return _enableCollision; }
            set { _enableCollision = value; }
        }

        public SplineMeshRenderer[] CustomMeshColliders
        {
            get { return _customMeshColliders; }
            set { _customMeshColliders = value; }
        }

        public bool IsCustomCollider
        {
            get { return _isCustomCollider; }
            set { _isCustomCollider = value; }
        }

        public bool HasCustomColliders
        {
            get
            {
                return (_customMeshColliders != null && (_customMeshColliders.Length > 0));
            }
        }

        #endregion

        /// <summary>
        /// On Enable
        /// </summary>
        void OnEnable()
        {
            if (_spline == null)
                _spline = GetComponent<Spline>();

            _meshCollider = GetComponent<MeshCollider>();
            GetAuxTranforms();

            if (!Application.isPlaying) // Avoids follow terrain collision with train on play mode
                ExtrudeMesh();
        }

        /// <summary>
        /// Get aux tranforms used for mesh generation
        /// </summary>
        private void GetAuxTranforms()
        {
            foreach (Transform child in transform)
            {
                switch (child.name)
                {
                    case "Aux1":
                        _auxTransform1 = child;
                        break;
                    case "Aux2":
                        _auxTransform2 = child;
                        break;
                }
            }
        }

        /// <summary>
        /// Once per frame
        /// </summary>
        private void Update()
        {
            //EDITOR ONLY REALTIME MESH GENERATION
            if (!Application.isPlaying)
            {
                if (_meshGenerationMethod == MeshGenerationMethod.Realtime)
                {
                    int activeGameObjectID = 0;
#if UNITY_EDITOR
                    activeGameObjectID = UnityEditor.Selection.activeInstanceID;
#endif
                    if (gameObject.GetInstanceID() == activeGameObjectID)
                        RealtimeMeshGeneration();
                }
            }
        }

        /// <summary>
        /// Realtime Mesh Generation (EDITOR ONLY)
        /// </summary>
        private void RealtimeMeshGeneration()
        {
            ExtrudeMesh();
        }

        /// <summary>
        /// Extrude base mesh along spline
        /// </summary>
        public void ExtrudeMesh()
        {
            Profiler.BeginSample("ExtrudeMesh");

            if (_baseMesh == null)
            {
                Debug.LogWarning("Base Mesh Cannot be null");
                return;
            }

            bool notUVMapped = false;

            ResetAuxTranforms();

            if (_auxTransform1 != null)
            {
                //Reset mesh for new mesh creation
                ResetMeshValues();
                //Scan mesh
                BaseMeshScan(out notUVMapped);

                if (notUVMapped)
                {
                    Debug.LogWarning("Base Mesh segment is not UV mapped. Mesh segment must be UV mapped for mesh generation to work.");
                    return;
                }

                //Create extruded mesh
                CreateMesh();
                //Update mesh renderer
                UpdateMeshRenderer();
                //Enable/disable mesh collider
                UpdateCollider();
                //Generate Custom colliders
                GenerateCustomColliders();

                //Debug.Log(_duplicatedVertices.Count);
            }

            Profiler.EndSample();
        }

        /// <summary>
        /// Reset auxiliar tranforms values
        /// </summary>
        private void ResetAuxTranforms()
        {
            Profiler.BeginSample("ResetAuxTranforms");

            if (_auxTransform1 != null)
                GetAuxTranforms();

            if (_auxTransform1 == null) //Avoid error if not found yet
                return;

            _auxTransform1.position = Vector3.zero;
            _auxTransform1.rotation = new Quaternion();
            _auxTransform2.position = Vector3.zero;
            _auxTransform2.rotation = new Quaternion();

            Profiler.EndSample();
        }

        /// <summary>
        /// Update mesh collider
        /// </summary>
        private void UpdateCollider()
        {
            Profiler.BeginSample("UpdateCollider");

            _meshCollider.enabled = HasCustomColliders ? false : (IsCustomCollider ? IsCustomCollider : _enableCollision);
            if (_meshCollider.enabled)
                _meshCollider.sharedMesh = mesh;

            Profiler.EndSample();
        }

        /// <summary>
        /// Generate Custom Colliders meshes
        /// </summary>
        private void GenerateCustomColliders()
        {
            if (!_isCustomCollider) //Custom Colliders can not custom colliders
            {
                Profiler.BeginSample("GenerateCustomColliders");

                if (_customMeshColliders != null)
                {
                    foreach (var customCollider in _customMeshColliders)
                    {
                        if (customCollider != null)
                        {
                            customCollider.IsCustomCollider = true;
                            customCollider._spline = _spline;
                            customCollider.MeshGenerationMethod = MeshGenerationMethod.Manual;
                            customCollider._enableCollision = true;
                            customCollider.GetComponent<MeshRenderer>().enabled = false;
                            customCollider.ExtrudeMesh();
                        }
                    }
                }

                Profiler.EndSample();
            }
        }

        /// <summary>
        /// Revove old mesh values
        /// </summary>
        private void ResetMeshValues()
        {
            Profiler.BeginSample("ResetMeshValues");

            _vertices.Clear();
            _normals.Clear();
            _uvs.Clear();
            _tangents.Clear();
            _subMeshTriangles.Clear();

            Profiler.EndSample();
        }

        /// <summary>
        /// Scan base mesh for lenght
        /// </summary>
        private void BaseMeshScan(out bool notUVMapped)
        {
            Profiler.BeginSample("BaseMeshScan");

            notUVMapped = false;
            if (_baseMesh.uv == null || _baseMesh.uv.Length == 0)
            {
                notUVMapped = true;
                return;
            }

            float min_z = 0.0f;
            float max_z = 0.0f;
            float min_x = 0.0f;
            float max_x = 0.0f;
            _baseMesh_VertexCount = _baseMesh.vertexCount;
            _baseMesh_SubMeshCount = _baseMesh.subMeshCount;
            _baseMesh_vertices = _baseMesh.vertices;
            _baseMesh_normals = _baseMesh.normals;
            _baseMesh_tangents = _baseMesh.tangents;
            _baseMesh_uv = _baseMesh.uv;

            // find length
            for (int i = 0; i < _baseMesh_VertexCount; i++)
            {
                _verticeScanned = _baseMesh_vertices[i];
                min_z = (_verticeScanned.z < min_z) ? _verticeScanned.z : min_z;
                max_z = (_verticeScanned.z > max_z) ? _verticeScanned.z : max_z;
                min_x = (_verticeScanned.x < min_x) ? _verticeScanned.x : min_x;
                max_x = (_verticeScanned.x > max_x) ? _verticeScanned.x : max_x;
            }

            _baseMeshMinZ = min_z;
            _baseMeshMaxZ = max_z;
            _baseMeshLength = max_z - min_z;

            Profiler.EndSample();
        }

        /// <summary>
        /// Calculate mesh generation parameters
        /// </summary>
        private void CreateMesh()
        {
            Profiler.BeginSample("CreateMesh");

            if (_spline == null)
                return;

            if (_auxTransform1 != null)
                GetAuxTranforms();

            if (_auxTransform1 == null)
                return;

            bool exceededVertsLimit = false;
            _splineLength = _spline.Length;

            // 2.0 Mesh generation
            _spline.CalculateOrientedPoints(_baseMeshLength);

            int penultimateIndex = _spline.OrientedPoints.Length - 2;

            for (int i = 0; i <= penultimateIndex; i++)
            {
                // Set aux transform 1
                _auxTransform1.rotation = _spline.OrientedPoints[i].Rotation;
                _auxTransform1.position = _spline.OrientedPoints[i].Position + _meshOffset;
                // Set aux transform 2
                _auxTransform2.rotation = _spline.OrientedPoints[i + 1].Rotation;
                _auxTransform2.position = _spline.OrientedPoints[i + 1].Position + _meshOffset;

                exceededVertsLimit = false;
                AddMeshSegment(_auxTransform1, _auxTransform2, out exceededVertsLimit);
                if (exceededVertsLimit)
                {
                    _meshGenerationMethod = MeshGenerationMethod.Manual;
                    break;
                }
            }

            Profiler.EndSample();
        }

        /// <summary>
        /// Add new mesh segment along the spline. Each segment corresponds to a copy of the base mesh
        /// </summary>
        private void AddMeshSegment(Transform start, Transform end, out bool exceededVertsLimit)
        {
            Profiler.BeginSample("AddMeshSegment");

            exceededVertsLimit = false;

            int indicesLength;

            //Each sub-mesh corresponds to a Material
            for (int subMeshIndex = 0; subMeshIndex < _baseMesh_SubMeshCount; subMeshIndex++)
            {
                _segmentIndices = _baseMesh.GetIndices(subMeshIndex);
                indicesLength = _segmentIndices.Length;

                //Add new submesh (if needed)
                if (_subMeshTriangles.Count < subMeshIndex + 1)
                {
                    for (int i = _subMeshTriangles.Count; i < subMeshIndex + 1; i++)
                    {
                        _subMeshTriangles.Add(new List<int>());
                    }
                }

                //Triangle vertex indices
                for (int i = 0; i < indicesLength; i += 3)
                {
                    CreateTriangle(start, end, new int[] { _segmentIndices[i], _segmentIndices[i + 1], _segmentIndices[i + 2] }, subMeshIndex, out exceededVertsLimit);

                    if (exceededVertsLimit)
                        return;
                }
            }

            Profiler.EndSample();
        }

        /// <summary>
        /// Recover vetorial values (Vector2, Vector3, Vector4)  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vectorArray"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        private T[] GetVector<T>(T[] vectorArray, int[] indices)
        {
            Profiler.BeginSample("GetVector");

            T[] ret = new T[3];

            for (int i = 0; i < 3; i++)
                ret[i] = vectorArray[indices[i]];

            Profiler.EndSample();
            return ret;
        }

        /// <summary>
        /// Create new triangle
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="subMeshIndex"></param>
        private void CreateTriangle(Transform start, Transform end, int[] indices, int subMeshIndex, out bool exceededVertsLimit)
        {
            Profiler.BeginSample("CreateTriangle");

            exceededVertsLimit = false;

            Profiler.BeginSample("Create Triangle Variables");
            _triangleVertices = GetVector<Vector3>(_baseMesh_vertices, indices);
            _triangleNormals = GetVector<Vector3>(_baseMesh_normals, indices);
            _triangleUvs = GetVector<Vector2>(_baseMesh_uv, indices);
            _triangleTangents = GetVector<Vector4>(_baseMesh_tangents, indices);
            Profiler.EndSample();

            Profiler.BeginSample("Convert Local to World Space");
            _triangleLocalToWorld_Start = start.localToWorldMatrix;
            _triangleLocalToWorld_End = end.localToWorldMatrix;
            _triangleWorldToLocal = transform.worldToLocalMatrix;
            Profiler.EndSample();

            // apply offset
            float lerpValue = 0.0f;

            for (int i = 0; i < 3; i++)
            {
                lerpValue = GetLerpValue(_triangleVertices[i].z, _baseMeshMinZ, _baseMeshMaxZ, 0.0f, 1.0f);
                _triangleVertices[i].z = 0.0f;

                //Calculate vertices worlds positions and length
                _triangleVerticesStart = _triangleLocalToWorld_Start.MultiplyPoint(_triangleVertices[i]);
                _triangleVerticesEnd = _triangleLocalToWorld_End.MultiplyPoint(_triangleVertices[i]);
                _triangleVertices[i] = _triangleWorldToLocal.MultiplyPoint(Vector3.Lerp(_triangleVerticesStart, _triangleVerticesEnd, lerpValue));

                //Calculate normals worlds positions and length
                _triangleNormalsStart = _triangleLocalToWorld_Start.MultiplyVector(_triangleNormals[i]);
                _triangleNormalsEnd = _triangleLocalToWorld_End.MultiplyVector(_triangleNormals[i]);
                _triangleNormals[i] = _triangleWorldToLocal.MultiplyVector(Vector3.Lerp(_triangleNormalsStart, _triangleNormalsEnd, lerpValue));

                //Calculate tangents worlds positions and length
                _triangleTangentsStart = _triangleLocalToWorld_Start.MultiplyVector(_triangleTangents[i]);
                _triangleTangentsEnd = _triangleLocalToWorld_End.MultiplyVector(_triangleTangents[i]);
                _triangleTangents[i] = _triangleWorldToLocal.MultiplyVector(Vector3.Lerp(_triangleTangentsStart, _triangleTangentsEnd, lerpValue));
            }

            exceededVertsLimit = (_vertices.Count + _triangleVertices.Length > 65000);

            if (exceededVertsLimit)
            {
                string warning = string.Format("Mesh cannot have more than 65000 vertices. {0}If you need to go even further, please use the Split Spline operation{0}to reduce the size of your spline and keep building using the newly created spline.", System.Environment.NewLine);
                warning += (_meshGenerationMethod == MeshGenerationMethod.Realtime) ? string.Format("{0}Realtime mesh generation disabled.", System.Environment.NewLine) : string.Empty;
                Debug.LogWarning(warning);
                return;
            }
            else
                AddTriangle(_triangleVertices, _triangleNormals, _triangleUvs, _triangleTangents, subMeshIndex);

            Profiler.EndSample();
        }

        /// <summary>
        /// Add created triangle
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="uvs"></param>
        /// <param name="tangents"></param>
        /// <param name="subMeshIndex"></param>
        public void AddTriangle(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, Vector4[] tangents, int subMeshIndex)
        {
            Profiler.BeginSample("AddTriangle");

            int initialVertCount = _vertices.Count;
            int currentVertCount = initialVertCount;
            int lastSegmentStartIndex = (initialVertCount - _baseMesh_VertexCount);

            int newVertexIndex = 0;
            int duplicateIndex = 0;
            int duplicatesFound = 0;

            for (int i = 0; i < 3; i++)
            {
                if (!IsDuplicate(vertices[i], normals[i], uvs[i], lastSegmentStartIndex, initialVertCount, out duplicateIndex))
                {
                    _vertices.Add(vertices[i]);
                    _normals.Add(normals[i]);
                    _uvs.Add(uvs[i]);
                    _tangents.Add(tangents[i]);

                    currentVertCount++;

                    newVertexIndex = (initialVertCount + i - duplicatesFound);
                    _subMeshTriangles[subMeshIndex].Add(newVertexIndex); //New vertex added index
                }
                else
                {
                    duplicatesFound++;
                    _subMeshTriangles[subMeshIndex].Add(duplicateIndex); //Duplicated vertex, use original poligon index for the triangle
                }
            }

            Profiler.EndSample();
        }

        /// <summary>
        /// Check for duplicated vertices
        /// </summary>
        /// <param name="vertice"></param>
        /// <param name="normal"></param>
        /// <param name="uv"></param>
        /// <param name="lastSegmentStartIndex"></param>
        /// <param name="initialVertCount"></param>
        /// <param name="originalIndex"></param>
        /// <returns></returns>
        private bool IsDuplicate(Vector3 vertice, Vector3 normal, Vector2 uv, int lastSegmentStartIndex, int initialVertCount, out int originalIndex)
        {
            Profiler.BeginSample("IsDuplicate");

            bool duplicated = false;
            originalIndex = 0;

            //First segment validation
            if (lastSegmentStartIndex < 0)
                lastSegmentStartIndex = 0;

            for (int i = lastSegmentStartIndex; i < initialVertCount; i++)
            {
                duplicated = (_vertices[i] == vertice && _normals[i] == normal && _uvs[i] == uv);
                if (duplicated)
                {
                    originalIndex = i;
                    break;
                }
            }

            Profiler.EndSample();
            return duplicated;
        }

        /// <summary>
        /// Update Mesh values
        /// </summary>
        public void UpdateMeshRenderer()
        {
            Profiler.BeginSample("UpdateMeshRenderer");

            mesh.Clear();
            mesh.SetVertices(_vertices);

            mesh.SetNormals(_normals);
            mesh.SetUVs(0, _uvs);
            mesh.SetUVs(1, _uvs);
            if (_tangents.Count > 1) mesh.SetTangents(_tangents);
            mesh.subMeshCount = _subMeshTriangles.Count;

            for (int i = 0; i < _subMeshTriangles.Count; i++)
                mesh.SetTriangles(_subMeshTriangles[i], i);

            //If not editing realtime, show mesh generation log
            if (_meshGenerationMethod == MeshGenerationMethod.Manual)
                PrintMeshDetails();

            Profiler.EndSample();
        }

        /// <summary>
        /// Print mesh details on console
        /// </summary>
        public void PrintMeshDetails()
        {
            if (_vertices.Count > 0)

                Debug.Log(string.Format("Mesh Generated{0}Vertices: {1} Normals: {2} Uvs: {3} Tangents: {4} subMeshCount: {5} (Base Mesh Vertices: {6} Segments Count: {7} Length: {8}m)"
                                        , System.Environment.NewLine, _vertices.Count, _normals.Count, _uvs.Count, _tangents.Count, mesh.subMeshCount, _baseMesh_VertexCount, (_vertices.Count / _baseMesh_VertexCount), _splineLength.ToString("n0")
                                        ));
            else
                Debug.Log(string.Format("Could not generated mesh. Check warning messages for more details."));
        }

        /// <summary>
        /// Create new renderer at the end of the current one
        /// </summary>
        public GameObject ConnectNewRenderer()
        {
            return _spline.AppendSpline();
        }

        /// <summary>
        /// Custom Lerp
        /// </summary>
        /// <param name="value"></param>
        /// <param name="oldMin"></param>
        /// <param name="oldMax"></param>
        /// <param name="newMin"></param>
        /// <param name="newMax"></param>
        /// <returns></returns>
        private float GetLerpValue(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            return ((value - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
        }
    }
}
