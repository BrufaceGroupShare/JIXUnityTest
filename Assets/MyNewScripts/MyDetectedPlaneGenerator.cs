using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using System.Linq;
using GoogleARCore.Examples.Common;

public class MyDetectedPlaneGenerator : MonoBehaviour
{
    /// <summary>
    /// A prefab for tracking and visualizing detected planes.
    /// </summary>
    public GameObject DetectedPlanePrefab;

    /// <summary>
    /// A list to hold new planes ARCore began tracking in the current frame. This object is
    /// used across the application to avoid per-frame allocations.
    /// </summary>
    private List<DetectedPlane> m_NewPlanes = new List<DetectedPlane>();

    ///temprarty hole pnales
    List<DetectedPlane> planes = new List<DetectedPlane>();

    /// <summary>
    /// The Unity Update method.
    /// </summary>
    public void Update()
    {
        // Check that motion tracking is tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        // Iterate over planes found in this frame and instantiate corresponding GameObjects to
        // visualize them.
        Session.GetTrackables<DetectedPlane>(m_NewPlanes, TrackableQueryFilter.New);

        //GameObject planeObject =
        //        Instantiate(DetectedPlanePrefab, Vector3.zero, Quaternion.identity, transform);
        //planeObject.GetComponent<DetectedPlaneVisualizer>().Initialize(GetLowestPlane());

        //if (m_NewPlanes[0] != null)
        //{
        //    GameObject planeObject =
        //       Instantiate(GridPrefab, new Vector3(0, -1, 0), Quaternion.identity, transform);
        //    planeObject.GetComponent<DetectedPlaneVisualizer>().Initialize(m_NewPlanes[0]);
        //} else
        //{
        //    return;
        //}

        GameObject planeObject =
                Instantiate(DetectedPlanePrefab, Vector3.zero, Quaternion.identity, transform);
        planeObject.GetComponent<DetectedPlaneVisualizer>().Initialize(GetLowestPlane());


        //for (int i = 0; i < m_NewPlanes.Count; i++)
        //{
        //    // Instantiate a plane visualization prefab and set it to track the new plane. The
        //    // transform is set to the origin with an identity rotation since the mesh for our
        //    // prefab is updated in Unity World coordinates.
        //    GameObject planeObject =
        //        Instantiate(DetectedPlanePrefab, Vector3.zero, Quaternion.identity, transform);
        //    planeObject.GetComponent<DetectedPlaneVisualizer>().Initialize(GetLowestPlane());
        //}
    }

    //private bool GetSessionPlanes()
    //{
    //    bool foundPlanes = false;

    //    if (Mathf.Abs(m_LastTimestamp - Time.time) > float.Epsilon)
    //    {
    //        // Checks if the ARCore session is valid and running.
    //        if (Session.Status == SessionStatus.Tracking && Session.Status.IsValid())
    //        {
    //            // Gets new planes for this update.
    //            m_AllPlanes.Clear();
    //            m_UpdatedPlanes.Clear();
    //            m_NewPlanes.Clear();

    //            Session.GetTrackables<DetectedPlane>(m_AllPlanes);
    //            Session.GetTrackables<DetectedPlane>(m_NewPlanes, TrackableQueryFilter.New);
    //            Session.GetTrackables<DetectedPlane>(m_UpdatedPlanes, TrackableQueryFilter.Updated);
    //        }

    //        m_LastTimestamp = Time.time;
    //    }

    //    foundPlanes = m_UpdatedPlanes.Count > 0 ? true : false;
    //    return foundPlanes;
    //}

    public List<DetectedPlane> GetPlanes(ARCorePlaneUtilQuery whichQuery)
    {
        // Conditionally collects new planes from ARCore.
        //GetSessionPlanes();

        List<DetectedPlane> planes = new List<DetectedPlane>();

        switch (whichQuery)
        {
            case ARCorePlaneUtilQuery.All:
                planes = m_NewPlanes;
                break;
                //case ARCorePlaneUtilQuery.New:
                //    planes = m_NewPlanes;
                //    break;
                //case ARCorePlaneUtilQuery.Updated:
                //    planes = m_UpdatedPlanes;
                //    break;
        }

        return planes;
    }

    public enum ARCorePlaneUtilQuery
    {
        /// <summary>
        /// Query all types of planes.
        /// </summary>
        All

        ///// <summary>
        ///// Query new planes.
        ///// </summary>
        //New,

        ///// <summary>
        ///// Query updated planes.
        ///// </summary>
        //Updated
    }

    public DetectedPlane GetLowestPlane()
    {
        //List<DetectedPlane> planes = GetPlanes(m_NewPlanes);

        var result = m_NewPlanes.OrderBy(a => a.CenterPose.position.y).ToArray();

        return result.Length > 0 ? result[0] : null;
    }

    //public DetectedPlane GetLowestPlane(ARCorePlaneUtilQuery whichQuery)
    //{
    //    List<DetectedPlane> planes = GetPlanes(whichQuery);

    //    var result = planes.OrderBy(a => a.CenterPose.position.y).ToArray();

    //    return result.Length > 0 ? result[0] : null;
    //}

    //public float GetLowestPlaneY(ARCorePlaneUtilQuery whichQuery)
    //{
    //    //DetectedPlane lowestPlane = GetLowestPlane(whichQuery);

    //    //float lowestY = lowestPlane != null ? lowestPlane.CenterPose.position.y : float.MaxValue;

    //    return lowestY;
    //}

    //public float GetLowestPlaneY(ARCorePlaneUtilQuery whichQuery)
    //{
    //    DetectedPlane lowestPlane = GetLowestPlane(whichQuery);

    //    float lowestY = lowestPlane != null ? lowestPlane.CenterPose.position.y : float.MaxValue;

    //    return lowestY;
    //}

    //public float GetLowestPlaneY()
    //{
    //    //return GetLowestPlaneY(ARCorePlaneUtilQuery.All);
    //}
}
