using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookTowardCamera : MonoBehaviour
{
    [SerializeField] Camera _targetCamera;

    private Quaternion _previousDirection;
    private Vector3 _previousPosition;
    private Vector3 _previousTargetPosition;
    private Transform _camParent;
    private Vector3 _previousParentPos;

    void Awake()
    {
        if (_targetCamera != null) return;

        _targetCamera = Camera.main;
        _camParent = _targetCamera.GetComponentInParent<Transform>();
    }

    void Update()
    {
        if (_targetCamera == null) return;

        Quaternion lookRot = Quaternion.LookRotation(_targetCamera.transform.forward, _targetCamera.transform.up);


        if (_previousPosition == transform.position &&
            lookRot == _previousDirection && 
            _previousTargetPosition == _targetCamera.transform.position) return;



        transform.rotation = lookRot;
        _previousDirection = lookRot;
        _previousPosition = transform.position;
        _previousTargetPosition = _targetCamera.transform.position;
    }

    public void SetTargetCamera(Camera camera)
    {
        _targetCamera = camera;
    }
}
