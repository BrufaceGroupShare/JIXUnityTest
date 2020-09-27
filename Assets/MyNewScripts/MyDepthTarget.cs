//-----------------------------------------------------------------------
// <copyright file="DepthTarget.cs" company="Google LLC">
//

using UnityEngine;

/// <summary>
/// Component for a depth data consuming object. The component will be automatically
/// registered to a depth source in the scene.
/// </summary>
public class MyDepthTarget : MonoBehaviour
{
    /// <summary>
    /// Flag to set the depth texture as mainTexture.
    /// </summary>
    public bool SetAsMainTexture = false;

    /// <summary>
    /// Material that get a depth texture assigned.
    /// </summary>
    public Material DepthTargetMaterial;

    private void OnEnable()
    {
        // Takes the material of the object's renderer, if no DepthTargetMaterial is explicitly set.
        if (DepthTargetMaterial == null)
        {
            var renderer = GetComponent<Renderer>();

            if (renderer != null)
            {
                DepthTargetMaterial = renderer.sharedMaterial;
            }
        }

        MyDepthSource.AddDepthTarget(this);
    }

    private void OnDisable()
    {
        MyDepthSource.RemoveDepthTarget(this);
    }
}
