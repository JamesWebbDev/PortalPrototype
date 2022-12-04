using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PortalScript : MonoBehaviour
{
	public PortalManager Manager { get; set; }

	public PortalScript LinkedPortal;
	private PortalScript[] _otherPortals;
	public MeshRenderer PortalScreen;
	private MeshFilter _screenFilter;
	private Vector3 _screenLocalPos;
	private Vector3 _screenScale;
	Camera _playerCam;
	Camera _portalCam;
	RenderTexture _viewTexture;

	int iterations;

	private List<PortalTraveller> _trackedTravellers = new List<PortalTraveller>();

    void Awake()
	{
		_playerCam = Camera.main;
		_portalCam = GetComponentInChildren<Camera>();
		_portalCam.enabled = false;
		

		_screenLocalPos = PortalScreen.transform.localPosition;
		_screenScale = PortalScreen.transform.localScale;
		_screenFilter = PortalScreen.GetComponent<MeshFilter>();

		PortalScreen.material.SetInt("DisplayMask", 1);

	}

    private void Start()
    {
		_otherPortals = Manager.GetUnlinkedPortals(this);
		iterations = Manager._maxPortalIerations;
    }

    #region Render Functions

    // Determine what pixels of a clone should and shouldn't be visible
    public void PrePortalRender()
    {
		foreach (var traveller in _trackedTravellers)
        {
			UpdateSlimeParams(traveller);
        }
    }

	// Determine what pixels of a clone should and shouldn't be visible
	public void PostPortalRender()
    {
		foreach (var traveller in _trackedTravellers)
		{
			UpdateSlimeParams(traveller);
		}

		ProtectScreenFromClipping(_playerCam.transform.position);
	}

    #endregion

    #region Rendering 

    void CreateViewTexture(PortalScript portal)
	{
		if (_viewTexture == null || _viewTexture.width != Screen.width || _viewTexture.height != Screen.height)
		{
			if (_viewTexture != null)
			{
				// Clear the old texture
				_viewTexture.Release();
			}

			_viewTexture = new RenderTexture(Screen.width, Screen.height, 0);

			// Render the view from the portal camera to the view texture
			_portalCam.targetTexture = _viewTexture;
			// Display the view texture on the screen of the linked portal
			portal.PortalScreen.material.SetTexture("_MainTex", _viewTexture);
		}
	}

	

	// Called just before player camera is rendered
	public void Render()
	{

		#region Old Code
		
		if (!VisibleFromCamera(LinkedPortal.PortalScreen, _playerCam))
		{
			return;
		}

		//UpdateOtherPortalsVisibility();


		CreateViewTexture(LinkedPortal);

		var localToWorldMatrix = _playerCam.transform.localToWorldMatrix;
		var renderPositions = new Vector3[iterations];
		var renderRotations = new Quaternion[iterations];

		int startIndex = 0;
		_portalCam.projectionMatrix = _playerCam.projectionMatrix;
		for (int i = 0; i < iterations; i++)
        {
			if (i > 0)
            {
				// No need for recursive rendering if linked portal is not visible through this portal
				if (!BoundsOverlap(_screenFilter, LinkedPortal._screenFilter, _portalCam))
				{
					break;
				}
			}

			localToWorldMatrix = transform.localToWorldMatrix * LinkedPortal.transform.worldToLocalMatrix * localToWorldMatrix;
			int renderOrderIndex = iterations - i - 1;
			renderPositions[renderOrderIndex] = localToWorldMatrix.GetColumn(3);
			renderRotations[renderOrderIndex] = localToWorldMatrix.rotation;

			_portalCam.transform.SetPositionAndRotation(renderPositions[renderOrderIndex], renderRotations[renderOrderIndex]);
			startIndex = renderOrderIndex;
        }

		// Make portal cam position and rotation the same relative to this portal as player cam relative to linked portal
		var m = transform.localToWorldMatrix * LinkedPortal.transform.worldToLocalMatrix * _playerCam.transform.localToWorldMatrix;
		_portalCam.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

		// Hide screen so that camera can see through portal
		PortalScreen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
		LinkedPortal.PortalScreen.material.SetInt("DisplayMask", 0);
		for (int i = 0; i < _otherPortals.Length; i++)
        {
			_otherPortals[i].PortalScreen.material.SetInt("DisplayMask", 0);
		}

		for (int i = startIndex; i < iterations; i++)
        {
			_portalCam.transform.SetPositionAndRotation(renderPositions[i], renderRotations[i]);

			SetNearClipPlane();
			HandleClipping();

			// Render what the camera sees to the view Texture
			_portalCam.Render();

			if (i == startIndex)
            {
				LinkedPortal.PortalScreen.material.SetInt("DisplayMask", 1);
			}
		}

		// Unhide objects hidden at start of render
		PortalScreen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
		
		#endregion

	}

	void RenderAPortal(PortalScript portal)
    {
		//if (!VisibleFromCamera(portal.PortalScreen, _playerCam))
		//{
		//	return;
		//}

		//UpdateOtherPortalsVisibility();


		CreateViewTexture(portal);

		

		var localToWorldMatrix = _playerCam.transform.localToWorldMatrix;
		var renderPositions = new Vector3[iterations];
		var renderRotations = new Quaternion[iterations];

		int startIndex = 0;
		_portalCam.projectionMatrix = _playerCam.projectionMatrix;
		for (int i = 0; i < iterations; i++)
		{
			if (i > 0)
			{
				// No need for recursive rendering if linked portal is not visible through this portal
				if (!BoundsOverlap(_screenFilter, portal._screenFilter, _portalCam))
				{
					break;
				}
			}

			localToWorldMatrix = transform.localToWorldMatrix * portal.transform.worldToLocalMatrix * localToWorldMatrix;
			int renderOrderIndex = iterations - i - 1;
			renderPositions[renderOrderIndex] = localToWorldMatrix.GetColumn(3);
			renderRotations[renderOrderIndex] = localToWorldMatrix.rotation;

			_portalCam.transform.SetPositionAndRotation(renderPositions[renderOrderIndex], renderRotations[renderOrderIndex]);
			startIndex = renderOrderIndex;
		}

		// Make portal cam position and rotation the same relative to this portal as player cam relative to linked portal
		//var m = transform.localToWorldMatrix * portal.transform.worldToLocalMatrix * _playerCam.transform.localToWorldMatrix;
		//_portalCam.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

		// Hide screen so that camera can see through portal
		PortalScreen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
		portal.PortalScreen.material.SetInt("DisplayMask", 0);
		for (int i = 0; i < _otherPortals.Length; i++)
		{
			_otherPortals[i].PortalScreen.material.SetInt("DisplayMask", 0);
		}

		for (int i = startIndex; i < iterations; i++)
		{
			_portalCam.transform.SetPositionAndRotation(renderPositions[i], renderRotations[i]);

			SetNearClipPlane();
			HandleClipping();

			// Render what the camera sees to the view Texture
			_portalCam.Render();

			if (i == startIndex)
			{
				portal.PortalScreen.material.SetInt("DisplayMask", 1);
			}

			for (int j = 0; j < _otherPortals.Length; j++)
			{
				_otherPortals[j].PortalScreen.material.SetInt("DisplayMask", 1);
			}
		}




		// Unhide objects hidden at start of render
		PortalScreen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
	}

	void HandleClipping()
	{
		// There are two main graphical issues when slicing travellers
		// 1. Tiny sliver of mesh drawn on backside of portal
		//    Ideally the oblique clip plane would sort this out, but even with 0 offset, tiny sliver still visible
		// 2. Tiny seam between the sliced mesh, and the rest of the model drawn onto the portal screen
		// This function tries to address these issues by modifying the slice parameters when rendering the view from the portal
		// Would be great if this could be fixed more elegantly, but this is the best I can figure out for now
		const float hideDst = -1000;
		const float showDst = 1000;
		float screenThickness = LinkedPortal.ProtectScreenFromClipping(_portalCam.transform.position);

		foreach (var traveller in _trackedTravellers)
		{
			if (SameSideOfPortal(traveller.transform.position, portalCamPos))
			{
				// Addresses issue 1
				traveller.SetSliceOffsetDst(hideDst, false);
			}
			else
			{
				// Addresses issue 2
				traveller.SetSliceOffsetDst(showDst, false);
			}

			// Ensure clone is properly sliced, in case it's visible through this portal:
			int cloneSideOfLinkedPortal = -SideOfPortal(traveller.transform.position);
			bool camSameSideAsClone = LinkedPortal.SideOfPortal(portalCamPos) == cloneSideOfLinkedPortal;
			if (camSameSideAsClone)
			{
				traveller.SetSliceOffsetDst(screenThickness, true);
			}
			else
			{
				traveller.SetSliceOffsetDst(-screenThickness, true);
			}
		}

		var offsetFromPortalToCam = portalCamPos - transform.position;
		foreach (var linkedTraveller in LinkedPortal._trackedTravellers)
		{
			var travellerPos = linkedTraveller.graphicsObject.transform.position;
			var clonePos = linkedTraveller.graphicsClone.transform.position;
			// Handle clone of linked portal coming through this portal:
			bool cloneOnSameSideAsCam = LinkedPortal.SideOfPortal(travellerPos) != SideOfPortal(portalCamPos);
			if (cloneOnSameSideAsCam)
			{
				// Addresses issue 1
				linkedTraveller.SetSliceOffsetDst(hideDst, true);
			}
			else
			{
				// Addresses issue 2
				linkedTraveller.SetSliceOffsetDst(showDst, true);
			}

			// Ensure traveller of linked portal is properly sliced, in case it's visible through this portal:
			bool camSameSideAsTraveller = LinkedPortal.SameSideOfPortal(linkedTraveller.transform.position, portalCamPos);
			if (camSameSideAsTraveller)
			{
				linkedTraveller.SetSliceOffsetDst(screenThickness, false);
			}
			else
			{
				linkedTraveller.SetSliceOffsetDst(-screenThickness, false);
			}
		}
	}

	// Sets the thickness of the portal screen so as not to clip with camera near plane when player goes through
	float ProtectScreenFromClipping(Vector3 cameraPos)
    {
		float halfHeight = _playerCam.nearClipPlane * Mathf.Tan(_playerCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
		float halfWidth = halfHeight * _playerCam.aspect;
		float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, _playerCam.nearClipPlane).magnitude;
		float screenThickness = dstToNearClipPlaneCorner;

		//int portalSide = Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));

		Transform screenT = PortalScreen.transform;
		bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - cameraPos) > 0;
		screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness);
		screenT.localPosition = (Vector3.forward * screenThickness * (camFacingSameDirAsPortal ? 0.5f : -0.5f)) + _screenLocalPos;

		return screenThickness;
    }

	public void ResetScreenPositionAndScale()
    {
		Transform screenT = PortalScreen.transform;
		screenT.localPosition = _screenLocalPos;
		screenT.localScale = _screenScale;
    }

    #endregion

    #region Teleporting

	// Checks if the traveller was on a different side of the portal this frame than it was on the previous frame
    private void LateUpdate()
    {
		HandleTravellers();
    }

    // Only add new travellers to the list and save their positions 
    void OnTravellerEnterPortal(PortalTraveller traveller)
    {
		if (!_trackedTravellers.Contains(traveller))
        {
			traveller.EnterPortalThreshold();
			traveller.PreviousOffsetFromPortal = traveller.transform.position - transform.position;
			_trackedTravellers.Add(traveller);
        }
    }

	void OnTravellerExitPortal(PortalTraveller traveller)
    {
		if (_trackedTravellers.Contains(traveller))
        {
			traveller.ExitPortalThreshold();
			_trackedTravellers.Remove(traveller);

        }
    }

	// Get travellers that enter the trigger box
	private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PortalTraveller traveller))
        {
			Debug.LogWarning("Detected Traveller");

			OnTravellerEnterPortal(traveller);
        }
    }

	
	// Remove travellers if they leave the trigger box
	private void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent(out PortalTraveller traveller) && _trackedTravellers.Contains(traveller))
		{
			traveller.ExitPortalThreshold();
			_trackedTravellers.Remove(traveller);
		}
	}

	void HandleTravellers()
	{
		for (int i = 0; i < _trackedTravellers.Count; i++)
		{
			PortalTraveller traveller = _trackedTravellers[i];
			Transform travellerT = traveller.transform;
			var m = LinkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;

			Vector3 offsetFromPortal = travellerT.position - transform.position;
			// int is Positive if traveller is in front of portal, Negative if traveller is behind portal
			int portalSide = Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
			int portalSideOld = Math.Sign(Vector3.Dot(traveller.PreviousOffsetFromPortal, transform.forward));
			

			// Teleport the traveller if it has crossed from one side of the portal to the other
			if (portalSide != portalSideOld)
			{
				var positionOld = travellerT.position;
				var rotOld = travellerT.rotation;

				Debug.Log("Teleport Player NOW!", this);

				// Teleport Player and update clone position and rotation at the same time
				traveller.Teleport(transform, LinkedPortal.transform, m.GetColumn(3), m.rotation);
				traveller.graphicsClone.transform.SetPositionAndRotation(positionOld, rotOld);

				// Can't rely on OnTriggerEnter/Exit to be called next frame because it depends on when FixedUpdate runs
				LinkedPortal.OnTravellerEnterPortal(traveller);
				_trackedTravellers.Remove(traveller);
				i--;
			}
			else
			{
				traveller.graphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

				traveller.PreviousOffsetFromPortal = offsetFromPortal;
			}
		}
	}


    #endregion

    #region Slicing Travellers

	void UpdateSlimeParams(PortalTraveller traveller)
    {
		// Calculate slice normal
		int side = SideOfPortal(traveller.transform.position);
		Vector3 sliceNormal = transform.forward * -side;
		Vector3 cloneSliceNormal = LinkedPortal.transform.forward * side;

		// Calculate slice center
		Vector3 slicePos = ScreenPos;
		Vector3 cloneSlicePos = LinkedPortal.ScreenPos;

		// Adjust slice offset so that when player standing on other side of portal to the object, the slice doesn't clip through
		float sliceOffsetDst = 0;
		float cloneSliceOffsetDst = 0;
        float screenThickness = PortalScreen.transform.localScale.z;

		bool playerSameSideAsTraveller = SameSideOfPortal(_playerCam.transform.position, traveller.transform.position);
		if (!playerSameSideAsTraveller)
		{
			sliceOffsetDst = -screenThickness;
		}
		bool playerSameSideAsCloneAppearing = side != LinkedPortal.SideOfPortal(_playerCam.transform.position);
		if (!playerSameSideAsCloneAppearing)
		{
			cloneSliceOffsetDst = -screenThickness;
		}

		// Apply parameters
		for (int i = 0; i < traveller.originalMaterials.Length; i++)
        {
			traveller.originalMaterials[i].SetVector("SliceCenter", slicePos);
			traveller.originalMaterials[i].SetVector("SliceNormal", sliceNormal);
			traveller.originalMaterials[i].SetFloat("SliceOffsetDst", sliceOffsetDst);

			traveller.cloneMaterials[i].SetVector("SliceCenter", cloneSlicePos);
			traveller.cloneMaterials[i].SetVector("SliceNormal", cloneSliceNormal);
            traveller.cloneMaterials[i].SetFloat("SliceOffsetDst", cloneSliceOffsetDst);
		}

	}


	// Use custom projection matrix to align portal camera's near clip plane with the surface of the portal
	// Note that this affects precision of the depth buffer, which can cause issues with effects like screenspace AO
	void SetNearClipPlane()
	{
		// Learning resource:
		// http://www.terathon.com/lengyel/Lengyel-Oblique.pdf

		Transform clipPlane = transform;
		int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - portalCamPos));

		Vector3 camSpacePos = _portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
		Vector3 camSpaceNormal = _portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
		float camSpaceDist = -Vector3.Dot(camSpacePos, camSpaceNormal);

		// pass in X, Y, Z and W variables to get clip plane view behind portals
		Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDist);

		// Update projection based on new clip plane
		// Calculate matrix with player cam so that player camera settings (fov, etc) are used
		_portalCam.projectionMatrix = _playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
	}

    #endregion

    #region Helper Functions

	// Set Display Mask if player can see other portals through linked portal
	void UpdateOtherPortalsVisibility()
    {
		for (int i = 0; i < _otherPortals.Length; i++)
        {
			var portal = _otherPortals[i];

			//if (!VisibleFromCamera(portal.PortalScreen, _playerCam))
			//{
				if (VisibleFromCamera(portal.PortalScreen, _portalCam))
                {
					portal.PortalScreen.material.SetInt("DisplayMask", 1);
				}
				
			//}

        }
    }

	// Returns true if the Renderer (object mesh) is inside the camera's view frustrum 
	static bool VisibleFromCamera(Renderer renderer, Camera camera)
	{
		Plane[] frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
		return GeometryUtility.TestPlanesAABB(frustrumPlanes, renderer.bounds);
	}

	// Determine which side of the portal this position is
	int SideOfPortal(Vector3 pos)
    {
		return Math.Sign(Vector3.Dot(pos - transform.position, transform.forward));
	}

	// Return whether these two positions are on the same side of this portal
	bool SameSideOfPortal(Vector3 posA, Vector3 posB)
	{
		return SideOfPortal(posA) == SideOfPortal(posB);
	}

	 
	Vector3 portalCamPos
	{
		get
		{
			return _portalCam.transform.position;
		}
	}

	public Vector3 ScreenPos
	{
		get
		{
			return PortalScreen.transform.position;
		}
	}

	// Check that other portal knows this portal exists
	void OnValidate()
	{
		if (LinkedPortal != null)
		{
			LinkedPortal.LinkedPortal = this;
		}
	}

	public static bool BoundsOverlap(MeshFilter nearObject, MeshFilter farObject, Camera camera)
	{

		var near = GetScreenRectFromBounds(nearObject, camera);
		var far = GetScreenRectFromBounds(farObject, camera);

		// ensure far object is indeed further away than near object
		if (far.zMax > near.zMin)
		{
			// Doesn't overlap on x axis
			if (far.xMax < near.xMin || far.xMin > near.xMax)
			{
				return false;
			}
			// Doesn't overlap on y axis
			if (far.yMax < near.yMin || far.yMin > near.yMax)
			{
				return false;
			}
			// Overlaps
			return true;
		}
		return false;
	}

	public static MinMax3D GetScreenRectFromBounds(MeshFilter renderer, Camera mainCamera)
	{
		MinMax3D minMax = new MinMax3D(float.MaxValue, float.MinValue);

		Vector3[] screenBoundsExtents = new Vector3[8];
		var localBounds = renderer.sharedMesh.bounds;
		bool anyPointIsInFrontOfCamera = false;

		for (int i = 0; i < 8; i++)
		{
			Vector3 localSpaceCorner = localBounds.center + Vector3.Scale(localBounds.extents, cubeCornerOffsets[i]);
			Vector3 worldSpaceCorner = renderer.transform.TransformPoint(localSpaceCorner);
			Vector3 viewportSpaceCorner = mainCamera.WorldToViewportPoint(worldSpaceCorner);

			if (viewportSpaceCorner.z > 0)
			{
				anyPointIsInFrontOfCamera = true;
			}
			else
			{
				// If point is behind camera, it gets flipped to the opposite side
				// So clamp to opposite edge to correct for this
				viewportSpaceCorner.x = (viewportSpaceCorner.x <= 0.5f) ? 1 : 0;
				viewportSpaceCorner.y = (viewportSpaceCorner.y <= 0.5f) ? 1 : 0;
			}

			// Update bounds with new corner point
			minMax.AddPoint(viewportSpaceCorner);
		}

		// All points are behind camera so just return empty bounds
		if (!anyPointIsInFrontOfCamera)
		{
			return new MinMax3D();
		}

		return minMax;
	}

	static readonly Vector3[] cubeCornerOffsets = {
		new Vector3 (1, 1, 1),
		new Vector3 (-1, 1, 1),
		new Vector3 (-1, -1, 1),
		new Vector3 (-1, -1, -1),
		new Vector3 (-1, 1, -1),
		new Vector3 (1, -1, -1),
		new Vector3 (1, 1, -1),
		new Vector3 (1, -1, 1),
	};

	public struct MinMax3D
	{
		public float xMin;
		public float xMax;
		public float yMin;
		public float yMax;
		public float zMin;
		public float zMax;

		public MinMax3D(float min, float max)
		{
			this.xMin = min;
			this.xMax = max;
			this.yMin = min;
			this.yMax = max;
			this.zMin = min;
			this.zMax = max;
		}

		public void AddPoint(Vector3 point)
		{
			xMin = Mathf.Min(xMin, point.x);
			xMax = Mathf.Max(xMax, point.x);
			yMin = Mathf.Min(yMin, point.y);
			yMax = Mathf.Max(yMax, point.y);
			zMin = Mathf.Min(zMin, point.z);
			zMax = Mathf.Max(zMax, point.z);
		}
	}

	#endregion


}
