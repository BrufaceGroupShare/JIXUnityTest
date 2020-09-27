using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.Common;

public class PlaneVisualizer : MonoBehaviour
{

    public float WaterDepthInM = -1f; //0.4f;

    private float m_WaterLevel;

    ARCorePlaneUtil PlaneUtil;

    public Material WaterMaterial = null;

    /// <summary>
    /// A prefab for tracking and visualizing detected planes.
    /// </summary>
    public GameObject TrackedPlanePrefab;

    private List<DetectedPlane> _newPlanes = new List<DetectedPlane>();

    private void Start()
    {
        m_WaterLevel = -0.5f;
        PlaneUtil = new ARCorePlaneUtil();
        WaterMaterial.SetFloat("_ShowColorOnly", 0);
    }

    // Update is called once per frame
    void Update()
    {
        Session.GetTrackables<DetectedPlane>(_newPlanes, TrackableQueryFilter.New);

        // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
        foreach (var curPlane in _newPlanes)
        {
            // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
            // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
            // coordinates.
            if (curPlane.CenterPose.position.y == PlaneUtil.GetLowestPlaneY())
            {

                m_WaterLevel = curPlane.CenterPose.position.y + WaterDepthInM;
                //var planeObject = Instantiate(TrackedPlanePrefab, Vector3.zero, Quaternion.identity, transform);
                var planeObject = Instantiate(TrackedPlanePrefab, new Vector3(0.0f, m_WaterLevel, 0.0f), Quaternion.identity,transform);
                planeObject.GetComponent<DetectedPlaneVisualizer>().Initialize(curPlane);

                // Apply a random color and grid rotation.
                //planeObject.GetComponent<Renderer>().material.SetColor("_GridColor", new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));
                //planeObject.GetComponent<Renderer>().material.SetFloat("_UvRotation", Random.Range(0.0f, 360.0f));
            }
            else
            {
                continue;
            }

        }

        //m_WaterLevel = PlaneUtil.GetLowestPlaneY() + WaterDepthInM;
        //transform.position = new Vector3(0.0f, m_WaterLevel, 0.0f);
    }
}
