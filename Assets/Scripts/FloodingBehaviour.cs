using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCoreInternal;
using System.Linq;

public class FloodingBehaviour : MonoBehaviour
{

    private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();
    private List<DetectedPlane> m_UpdatedPlanes = new List<DetectedPlane>();
    private List<DetectedPlane> m_NewPlanes = new List<DetectedPlane>();
    private float m_LastTimestamp;

    /// <summary>
    /// Types of ARCore plane queries.
    /// </summary>
    public enum ARCorePlaneUtilQuery
    {
        /// <summary>
        /// Query all types of planes.
        /// </summary>
        All,

        /// <summary>
        /// Query new planes.
        /// </summary>
        New,

        /// <summary>
        /// Query updated planes.
        /// </summary>
        Updated
    }

    /// <summary>
    /// Gets the queried planes.
    /// </summary>
    /// <param name="whichQuery">Type of queried plane.</param>
    /// <returns>Returns a list of deteted planes.</returns>
    public List<DetectedPlane> GetPlanes(ARCorePlaneUtilQuery whichQuery)
    {
        // Conditionally collects new planes from ARCore.
        GetSessionPlanes();

        List<DetectedPlane> planes = new List<DetectedPlane>();

        switch (whichQuery)
        {
            case ARCorePlaneUtilQuery.All:
                planes = m_AllPlanes;
                break;
            case ARCorePlaneUtilQuery.New:
                planes = m_NewPlanes;
                break;
            case ARCorePlaneUtilQuery.Updated:
                planes = m_UpdatedPlanes;
                break;
        }

        return planes;
    }

    /// <summary>
    /// Gets the lowest plane.
    /// </summary>
    /// <param name="whichQuery">Type of queried plane.</param>
    /// <returns>Returns the lowest detected plane.</returns>
    public DetectedPlane GetLowestPlane(ARCorePlaneUtilQuery whichQuery)
    {
        List<DetectedPlane> planes = GetPlanes(whichQuery);

        var result = planes.OrderBy(a => a.CenterPose.position.y).ToArray();

        return result.Length > 0 ? result[0] : null;
    }

    /// <summary>
    /// Gets the Y value of the lowest plane.
    /// </summary>
    /// <returns>Returns the Y value of the lowest plane.</returns>
    public float GetLowestPlaneY()
    {
        return GetLowestPlaneY(ARCorePlaneUtilQuery.All);
    }

    /// <summary>
    /// Gets the Y value of the lowest plane.
    /// </summary>
    /// <param name="whichQuery">Type of queried plane.</param>
    /// <returns>Returns the Y value of the lowest plane.</returns>
    public float GetLowestPlaneY(ARCorePlaneUtilQuery whichQuery)
    {
        DetectedPlane lowestPlane = GetLowestPlane(whichQuery);

        float lowestY = lowestPlane != null ? lowestPlane.CenterPose.position.y : float.MaxValue;

        return lowestY;
    }

    private bool GetSessionPlanes()
    {
        bool foundPlanes = false;

        if (Mathf.Abs(m_LastTimestamp - Time.time) > float.Epsilon)
        {
            // Checks if the ARCore session is valid and running.
            if (Session.Status == SessionStatus.Tracking && Session.Status.IsValid())
            {
                // Gets new planes for this update.
                m_AllPlanes.Clear();
                m_UpdatedPlanes.Clear();
                m_NewPlanes.Clear();

                Session.GetTrackables<DetectedPlane>(m_AllPlanes);
                Session.GetTrackables<DetectedPlane>(m_NewPlanes, TrackableQueryFilter.New);
                Session.GetTrackables<DetectedPlane>(m_UpdatedPlanes, TrackableQueryFilter.Updated);
            }

            m_LastTimestamp = Time.time;
        }

        foundPlanes = m_UpdatedPlanes.Count > 0 ? true : false;
        return foundPlanes;
    }



    /// <summary>
    /// /////////////////////////////////////////
    /// </summary>
    //ARCorePlaneUtil m_ARCore;
    /// <summary>
    /// Water depth in meters.
    /// </summary>
    public float WaterDepthInM = 0.0f;//0.4f;

    /// <summary>
    /// Plane X dimension.
    /// </summary>
    public int Xdim = 250;

    /// <summary>
    /// Plane Y dimension.
    /// </summary>
    public int Ydim = 250;

    /// <summary>
    /// Plane cell size in m.
    /// </summary>
    public float CellSizeInM = 0.1f;

    /// <summary>
    /// Wave speed.
    /// </summary>
    public float WaveSpeed = 0.00025f;

    /// <summary>
    /// Wave height intensity.
    /// </summary>
    public float WaveIntensity = 0.025f;

    /// <summary>
    /// Material for rendering the flooding effects.
    /// </summary>
    public Material WaterMaterial = null;

    private float m_WaterLevel;
    private Vector3[] m_Vertices;
    private Vector3[] m_Normals;
    private Color[] m_Colors;
    private MeshFilter m_MeshFilter;
    private Mesh m_Mesh;

    private void GenerateWater()
    {
        m_MeshFilter = GetComponent<MeshFilter>();
        m_MeshFilter.sharedMesh = null;

        m_Mesh = new Mesh();
        m_Mesh.name = "Water";

        m_Vertices = new Vector3[(Xdim + 1) * (Ydim + 1)];
        m_Normals = new Vector3[m_Vertices.Length];
        Vector2[] uv = new Vector2[m_Vertices.Length];

        UpdateWater();
        for (int i = 0, y = 0; y <= Ydim; y++)
        {
            for (int x = 0; x <= Xdim; x++, i++)
            {
                uv[i] = new Vector2((float)x / Xdim, (float)y / Ydim);
            }
        }

        int[] triangles = new int[Xdim * Ydim * 6];
        for (int ti = 0, vi = 0, y = 0; y < Ydim; y++, vi++)
        {
            for (int x = 0; x < Xdim; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + Xdim + 1;
                triangles[ti + 5] = vi + Xdim + 2;
            }
        }

        m_Mesh.uv = uv;
        m_Mesh.triangles = triangles;
        m_Mesh.RecalculateNormals();
        m_Mesh.RecalculateTangents();

        m_MeshFilter.sharedMesh = m_Mesh;
        WaterMaterial.SetFloat("_ShowColorOnly", 0);
    }

    private void UpdateWater()
    {
        Vector3 center = new Vector3(Xdim * CellSizeInM * 0.5f, 0.0f, Ydim * CellSizeInM * 0.5f);
        m_Colors = new Color[m_Vertices.Length];

        for (int i = 0, y = 0; y <= Ydim; y++)
        {
            for (int x = 0; x <= Xdim; x++, i++)
            {
                Random.InitState(i + 123);
                float rndWaveSpeed = Random.Range(0.0001f, WaveSpeed);
                float rndWaveIntensity = Random.Range(0.0001f, WaveIntensity);
                float up = (Mathf.Sin(Time.time * rndWaveSpeed * i) * rndWaveIntensity) +
                    (WaveIntensity * 0.5f);
                m_Vertices[i] = new Vector3(x * CellSizeInM, up, y * CellSizeInM) - center;
                m_Normals[i] = Vector3.up;
                m_Colors[i] = new Color(up / WaveIntensity, up / WaveIntensity, up / WaveIntensity);
            }
        }

        m_Mesh.vertices = m_Vertices;
        m_Mesh.colors = m_Colors;
    }


    private void Awake()
    {
        //this.name = "ARCorePlaneUtil";
        m_LastTimestamp = 0.0f;

        m_Mesh = GetComponent<MeshFilter>().mesh;
        m_MeshRenderer = GetComponent<UnityEngine.MeshRenderer>();
    }



    private void Start()
    {
        m_WaterLevel = -0.5f;
        GenerateWater();
        UpdateWater();

    }

    private void Update()
    {
        UpdateWater();
        //m_WaterLevel = GetLowestPlaneY() + WaterDepthInM;
        //transform.position = new Vector3(0.0f, m_WaterLevel, 0.0f);

        m_WaterLevel = GetLowestPlaneY() + WaterDepthInM;
        transform.position = new Vector3(0.0f, GetLowestPlaneY(), 0.0f);

        if (m_DetectedPlane == null)
        {
            return;
        }
        else if (m_DetectedPlane.SubsumedBy != null)
        {
            Destroy(gameObject);
            return;
        }
        else if (m_DetectedPlane.TrackingState != TrackingState.Tracking)
        {
            m_MeshRenderer.enabled = false;
            return;
        }

        m_MeshRenderer.enabled = true;
        _UpdateMeshIfNeeded();
        //transform.position = new Vector3(0.0f, Camera.main.transform.position.y, 0.0f);
    }

    private void OnDestroy()
    {
        if (WaterMaterial != null)
        {
            WaterMaterial.SetFloat("_ShowColorOnly", 1f);
        }

        m_AllPlanes.Clear();
        m_UpdatedPlanes.Clear();
        m_NewPlanes.Clear();
    }

    //private void Start()
    //{

    //}

    //private void Update()
    //{
    //   transform.position = new Vector3(0, Camera.main.transform.position.y, 0);
    //}

    private DetectedPlane m_DetectedPlane;
    private List<Vector3> m_PreviousFrameMeshVertices = new List<Vector3>();
    private List<Vector3> m_MeshVertices = new List<Vector3>();
    private Vector3 m_PlaneCenter = new Vector3();

    private List<Color> m_MeshColors = new List<Color>();

    private List<int> m_MeshIndices = new List<int>();

    private MeshRenderer m_MeshRenderer;

    private void _UpdateMeshIfNeeded()
    {
        m_DetectedPlane.GetBoundaryPolygon(m_MeshVertices);

        if (_AreVerticesListsEqual(m_PreviousFrameMeshVertices, m_MeshVertices))
        {
            return;
        }

        m_PreviousFrameMeshVertices.Clear();
        m_PreviousFrameMeshVertices.AddRange(m_MeshVertices);

        m_PlaneCenter = m_DetectedPlane.CenterPose.position;

        Vector3 planeNormal = m_DetectedPlane.CenterPose.rotation * Vector3.up;

        m_MeshRenderer.material.SetVector("_PlaneNormal", planeNormal);

        int planePolygonCount = m_MeshVertices.Count;

        // The following code converts a polygon to a mesh with two polygons, inner polygon
        // renders with 100% opacity and fade out to outter polygon with opacity 0%, as shown
        // below.  The indices shown in the diagram are used in comments below.
        // _______________     0_______________1
        // |             |      |4___________5|
        // |             |      | |         | |
        // |             | =>   | |         | |
        // |             |      | |         | |
        // |             |      |7-----------6|
        // ---------------     3---------------2
        m_MeshColors.Clear();

        // Fill transparent color to vertices 0 to 3.
        for (int i = 0; i < planePolygonCount; ++i)
        {
            m_MeshColors.Add(Color.clear);
        }

        // Feather distance 0.2 meters.
        const float featherLength = 0.2f;

        // Feather scale over the distance between plane center and vertices.
        const float featherScale = 0.2f;

        // Add vertex 4 to 7.
        for (int i = 0; i < planePolygonCount; ++i)
        {
            Vector3 v = m_MeshVertices[i];

            // Vector from plane center to current point
            Vector3 d = v - m_PlaneCenter;

            float scale = 1.0f - Mathf.Min(featherLength / d.magnitude, featherScale);
            m_MeshVertices.Add((scale * d) + m_PlaneCenter);

            m_MeshColors.Add(Color.white);
        }

        m_MeshIndices.Clear();
        int firstOuterVertex = 0;
        int firstInnerVertex = planePolygonCount;

        // Generate triangle (4, 5, 6) and (4, 6, 7).
        for (int i = 0; i < planePolygonCount - 2; ++i)
        {
            m_MeshIndices.Add(firstInnerVertex);
            m_MeshIndices.Add(firstInnerVertex + i + 1);
            m_MeshIndices.Add(firstInnerVertex + i + 2);
        }

        // Generate triangle (0, 1, 4), (4, 1, 5), (5, 1, 2), (5, 2, 6), (6, 2, 3), (6, 3, 7)
        // (7, 3, 0), (7, 0, 4)
        for (int i = 0; i < planePolygonCount; ++i)
        {
            int outerVertex1 = firstOuterVertex + i;
            int outerVertex2 = firstOuterVertex + ((i + 1) % planePolygonCount);
            int innerVertex1 = firstInnerVertex + i;
            int innerVertex2 = firstInnerVertex + ((i + 1) % planePolygonCount);

            m_MeshIndices.Add(outerVertex1);
            m_MeshIndices.Add(outerVertex2);
            m_MeshIndices.Add(innerVertex1);

            m_MeshIndices.Add(innerVertex1);
            m_MeshIndices.Add(outerVertex2);
            m_MeshIndices.Add(innerVertex2);
        }

        m_Mesh.Clear();
        m_Mesh.SetVertices(m_MeshVertices);
        m_Mesh.SetTriangles(m_MeshIndices, 0);
        m_Mesh.SetColors(m_MeshColors);
    }

    private bool _AreVerticesListsEqual(List<Vector3> firstList, List<Vector3> secondList)
    {
        if (firstList.Count != secondList.Count)
        {
            return false;
        }

        for (int i = 0; i < firstList.Count; i++)
        {
            if (firstList[i] != secondList[i])
            {
                return false;
            }
        }

        return true;
    }

}
