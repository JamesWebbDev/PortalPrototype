using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PortalTraveller : MonoBehaviour
{
    [Header("Portal Traveller Variables")]
    // Slicing Variables
    public GameObject graphicsObject;
    public GameObject graphicsClone { get; set; }
    public Material[] originalMaterials { get; set; }
    public Material[] cloneMaterials { get; set; }

    // Teleporting Variables
    public Vector3 PreviousOffsetFromPortal { get; set; }

    public virtual void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;

        Debug.LogError("I Got Teleported!", this);
    }



    // Called when first touches portal
    public virtual void EnterPortalThreshold()
    {
        if (graphicsClone == null)
        {
            Debug.Log("New Clone created");
            graphicsClone = Instantiate(graphicsObject);
            graphicsClone.transform.parent = graphicsObject.transform.parent;
            graphicsClone.transform.localScale = graphicsObject.transform.localScale;
            originalMaterials = GetMaterials(graphicsObject);
            cloneMaterials = GetMaterials(graphicsClone);
        }
        else
        {
            graphicsClone.SetActive(true);
        }
    }

    // Called once no longer touching portal (excluding when teleporting)
    public virtual void ExitPortalThreshold()
    {
        graphicsClone.SetActive(false);
        // Disable slicing
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            originalMaterials[i].SetVector("SliceNormal", Vector3.zero);
        }
    }

    public void SetSliceOffsetDst(float dist, bool isClone)
    {
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            if (isClone)
            {
                cloneMaterials[i].SetFloat("SliceOffsetDst", dist);
            }
            else
            {
                originalMaterials[i].SetFloat("SliceOffsetDst", dist);
            }

        }
    }

    Material[] GetMaterials(GameObject g)
    {
        var renderers = g.GetComponentsInChildren<MeshRenderer>();
        var matList = new List<Material>();
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                matList.Add(mat);
            }
        }
        return matList.ToArray();
    }


}
