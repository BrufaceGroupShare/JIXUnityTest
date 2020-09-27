using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class MyDepthSource : MonoBehaviour
{

    /// Value for testing whether a depth pixel has an invalid value.
    /// </summary>
    public const float InvalidDepthValue = 0;

    /// <summary>
    /// Value for converting millimeter depth to meter.
    /// </summary>
    public const float MillimeterToMeter = 0.001f;

    private static readonly string k_CurrentDepthTexturePropertyName = "_CurrentDepthTexture";
    private static readonly string k_TopLeftRightPropertyName = "_UvTopLeftRight";
    private static readonly string k_BottomLeftRightPropertyName = "_UvBottomLeftRight";
    private static readonly string k_OcclusionBlendingScale = "_OcclusionBlendingScale";

    private static Texture2D s_DepthTexture;
    private static List<MyDepthTarget> s_DepthTargets = new List<MyDepthTarget>();
    private static MyDepthSource s_Instance;
    private static Matrix4x4 s_ScreenRotation = Matrix4x4.Rotate(Quaternion.identity);
    private static Matrix4x4 s_LocalToWorldTransform = Matrix4x4.identity;
    private static bool s_UpdateDepth;
    private static bool s_AlwaysUpdateDepth;
    private static IDepthDataSource s_DepthDataSource;


    /// <summary>
    /// Gets the global reference to the depth texture.
    /// </summary>
    public static Texture2D DepthTexture
    {
        get
        {
            CheckAttachedToScene();

            if (!s_UpdateDepth)
            {
                s_DepthDataSource.UpdateDepthTexture(ref s_DepthTexture);
                s_UpdateDepth = true;
            }

            return s_DepthTexture;
        }
    }


    /// <summary>
    /// Gets the screen rotation transform to be used with a vertex.
    /// </summary>
    public static Matrix4x4 ScreenRotation
    {
        get
        {
            return s_ScreenRotation;
        }
    }



    /// <summary>
    /// Adds the DepthTarget to a list of depth consumers.
    /// </summary>
    /// <param name="target">A DepthTarget instance, which uses depth.</param>
    public static void AddDepthTarget(MyDepthTarget target)
    {
        CheckAttachedToScene();

        if (!s_DepthTargets.Contains(target))
        {
            s_DepthTargets.Add(target);

            if (target.DepthTargetMaterial != null)
            {
                SetDepthTexture(target);
            }
        }
    }

    
    /// <summary>
    /// Removes the DepthTarget from a list of depth consumers.
    /// </summary>
    /// <param name="target">A DepthTarget instance, which uses depth.</param>
    public static void RemoveDepthTarget(MyDepthTarget target)
    {
        CheckAttachedToScene();

        if (s_DepthTargets.Contains(target))
        {
            s_DepthTargets.Remove(target);
        }
    }

    /// <summary>
    /// Checks whether this component is part of the scene.
    /// </summary>
    private static void CheckAttachedToScene()
    {
        if (s_Instance == null)
        {
            if (Camera.main != null)
            {
                s_Instance = Camera.main.gameObject.AddComponent<MyDepthSource>();
            }
        }
    }

    private static void SetDepthTexture(MyDepthTarget target)
    {
        Texture2D depthTexture = DepthTexture;

        if (target.SetAsMainTexture)
        {
            if (target.DepthTargetMaterial.mainTexture != depthTexture)
            {
                target.DepthTargetMaterial.mainTexture = depthTexture;
            }
        }
        else if (target.DepthTargetMaterial.GetTexture(k_CurrentDepthTexturePropertyName) !=
            depthTexture)
        {
            target.DepthTargetMaterial.SetTexture(k_CurrentDepthTexturePropertyName,
                depthTexture);
        }
    }

    /// <summary>
    /// Sets a rotation Matrix4x4 to correctly transform the point cloud when the phone is used
    /// in different screen orientations.
    /// </summary>
    private static void UpdateScreenOrientation()
    {
        switch (Screen.orientation)
        {
            case ScreenOrientation.Portrait:
                s_ScreenRotation = Matrix4x4.Rotate(Quaternion.Euler(0, 0, -90));
                break;
            case ScreenOrientation.LandscapeLeft:
                s_ScreenRotation = Matrix4x4.Rotate(Quaternion.identity);
                break;
            case ScreenOrientation.PortraitUpsideDown:
                s_ScreenRotation = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90));
                break;
            case ScreenOrientation.LandscapeRight:
                s_ScreenRotation = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 180));
                break;
        }
    }

    private void Start()
    {
        var config = (DepthDataSourceConfig)Resources.Load("DepthDataSourceConfig");
        if (config != null && config.DepthDataSource != null)
        {
            s_DepthDataSource = config.DepthDataSource;
        }

        s_Instance = this;
        s_AlwaysUpdateDepth = true;

        // Default texture, will be updated each frame.
        s_DepthTexture = new Texture2D(2, 2);

        foreach (MyDepthTarget target in s_DepthTargets)
        {
            if (target.DepthTargetMaterial != null)
            {
                SetDepthTexture(target);
                UpdateScreenOrientationOnMaterial(target.DepthTargetMaterial);
                SetAlphaForBlendedOcclusionProperties(target.DepthTargetMaterial);
            }
        }
    }

    private void UpdateScreenOrientationOnMaterial(Material material)
    {
        var uvQuad = Frame.CameraImage.TextureDisplayUvs;
        material.SetVector(
            k_TopLeftRightPropertyName,
            new Vector4(
                uvQuad.TopLeft.x, uvQuad.TopLeft.y, uvQuad.TopRight.x, uvQuad.TopRight.y));
        material.SetVector(
            k_BottomLeftRightPropertyName,
            new Vector4(uvQuad.BottomLeft.x, uvQuad.BottomLeft.y, uvQuad.BottomRight.x,
                uvQuad.BottomRight.y));
    }

    private void SetAlphaForBlendedOcclusionProperties(Material material)
    {
        material.SetFloat(k_OcclusionBlendingScale, 0.5f);
    }

    private void Update()
    {
        UpdateScreenOrientation();

        s_LocalToWorldTransform = Camera.main.transform.localToWorldMatrix * ScreenRotation;

        bool updateDepth = false;

        foreach (MyDepthTarget target in s_DepthTargets)
        {
            if (!updateDepth)
            {
                updateDepth = true;
            }


            if (target.DepthTargetMaterial != null)
            {
                SetDepthTexture(target);
                UpdateScreenOrientationOnMaterial(target.DepthTargetMaterial);
                SetAlphaForBlendedOcclusionProperties(target.DepthTargetMaterial);
            }
        }

        s_UpdateDepth = updateDepth || s_AlwaysUpdateDepth;

        if (s_UpdateDepth)
        {
            // Updates depth from ARCore, only if at least one DepthTarget uses depth.
            s_DepthDataSource.UpdateDepthTexture(ref s_DepthTexture);
        }
    }

}
