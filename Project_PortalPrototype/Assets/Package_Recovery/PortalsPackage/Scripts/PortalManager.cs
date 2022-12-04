using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class PortalManager : MonoBehaviour
{
	private PortalScript[] _portalArray;
    public int _maxPortalIerations;


    void Awake()
	{
		_portalArray = FindObjectsOfType<PortalScript>();

        if (_portalArray.Length == 0)
        {
            Debug.LogError("Couldn't find any portals", this);
        }

        for (int i = 0; i < _portalArray.Length; i++)
        {
            _portalArray[i].Manager = this;
        }
	}

    private void OnPreCull()
    {
        PreRenderAllPortalTextures();

        RenderAllPortalTextures();

        PostRenderAllPortalTextures();
    }

    private void RenderAllPortalTextures()
    {
        for (int i = 0; i < _portalArray.Length; i++)
        {
            _portalArray[i].Render();
        }
    }
    private void PreRenderAllPortalTextures()
    {
        for (int i = 0; i < _portalArray.Length; i++)
        {
            _portalArray[i].PrePortalRender();
        }
    }

    private void PostRenderAllPortalTextures()
    {
        for (int i = 0; i < _portalArray.Length; i++)
        {
            _portalArray[i].PostPortalRender();
        }
    }

    public PortalScript[] GetUnlinkedPortals(PortalScript origPortal)
    {
        List<PortalScript> portals = new List<PortalScript>();

        for (int i = 0; i < _portalArray.Length; i++)
        {
            PortalScript portal = _portalArray[i];

            if (portal != origPortal && portal != portal.LinkedPortal)
            {
                portals.Add(portal);
            }
        }

        return portals.ToArray();
    }

}
