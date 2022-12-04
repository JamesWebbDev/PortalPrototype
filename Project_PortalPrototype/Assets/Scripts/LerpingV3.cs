using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpingV3 : MonoBehaviour
{
    [SerializeField] float _speed = 1;

    [SerializeField] bool _isTargetLocal = true;

    [Header("Global Variables")]
    [SerializeField] Vector3 _target;
    [Header("Local Variables")]
    [SerializeField] Vector3 _offset;

    private Vector3 _origPosition;
    private Vector3 _offsetPosition;

    
    private bool isIncreasing = true;


    private float _normalisedTime;
    

    void Start()
    {
        _origPosition = transform.position;
        _normalisedTime = 0;
    }

    void Update()
    {
        if (_normalisedTime <= 0 || _normalisedTime >= 1f) isIncreasing = !isIncreasing;
        _normalisedTime += Time.deltaTime * _speed * (isIncreasing ? 1f : -1f);

        _normalisedTime = Mathf.Clamp(_normalisedTime, 0f, 1f);

        if (_isTargetLocal)
        {
            LocalLerp();
        }
        else
        {
            GlobalLerp();
        }
    }

    void LocalLerp()
    {
        transform.position = Vector3.Lerp(_origPosition, _origPosition + _offset, _normalisedTime);
    }

    void GlobalLerp()
    {
        transform.position = Vector3.Lerp(_origPosition, _target, _normalisedTime);
    }


}
