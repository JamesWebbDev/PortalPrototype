using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : PortalTraveller
{
	private CharacterController _controller;
    private Camera _fpCamera;

    [Header("Game/Debugging")]
    [Tooltip("Tick this if the cursor should disappear on Play.")]
    [SerializeField] bool _lockCursor = true;
    [Tooltip("Press this key to stop the game in Editor.")]
    [SerializeField] KeyCode _stopKey = KeyCode.P;
    [Tooltip("Press this key to disable this Player Controller.")]
    [SerializeField] KeyCode _disableKey = KeyCode.O;

    [Header("Key Bindings")]
    [Tooltip("Hold this key to sprint.")]
    [SerializeField] KeyCode _sprintKey = KeyCode.LeftShift;
    [Tooltip("Press this key to jump.")]
    [SerializeField] KeyCode _jumpKey = KeyCode.Space;

    [Header("Movement")]
    [Range(0f, 10f)]
    [SerializeField] float _walkSpeed = 5;
    [Range(0f, 20f)]
    [SerializeField] float _runSpeed = 10;
    [Tooltip("This controls how quickly the player comes to a stop.")]
    [Range(0f, 1f)]
    [SerializeField] float _smoothMoveTime = 0.1f;

    [Header("Jumping")]
    [SerializeField] float _gravity = 12;
    [Range(1f, 30f)]
    [SerializeField] float _jumpForce = 10;

    //[Space]
    [Header("Camera Movement")]
    [Tooltip("Clamp the camera pitch (looking up and down) between these two floats.")]
    [SerializeField] Vector2 _pitchMinMax = new Vector2(-60f, 60f);
    [Range(1f, 50f)]
    [SerializeField] float _mouseSensitivity = 1;
    [Space]
    [Tooltip("Tick this box if the camera should use smoothing")]
    [SerializeField] bool _smoothCamera = true;
    [Tooltip("This controls how quickly the camera stops moving.")]
    [Range(0f, 1f)]
    [SerializeField] float _camRotateSmoothTime = 0.1f;

    // Private Variables

    private bool _isDisabled;

	private Vector2 _input;
    private Vector3 _velocity;
    private float _verticalVelocity;
    private Vector3 _smoothVelocity;

    private bool _isJumping;
    private float _lastGroundedTime;

    private float _camYaw;
    private float _camPitch;
    private float _yawSmoothVelocity;
    private float _pitchSmoothVelocity;
    private float _smoothYaw;
    private float _smoothPitch;

    bool _isTeleporting = false;

    

    void Awake()
	{
		_controller = GetComponent<CharacterController>();
        _fpCamera = GetComponentInChildren<Camera>();

        if (_lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        _camYaw = transform.eulerAngles.y;
        _camPitch = _fpCamera.transform.localEulerAngles.x;
        _smoothYaw = _camYaw;
        _smoothPitch = _camPitch;
    }

    #region Regular Script

    void Update()
    {
        if (Input.GetKeyDown(_stopKey))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Break();
        }
        if (Input.GetKeyDown(_disableKey))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _isDisabled = !_isDisabled;
        }

        if (_isDisabled)
        {
            return;
        }

        _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 inputDir = new Vector3(_input.x, 0, _input.y);
        Vector3 worldInputDir = transform.TransformDirection(inputDir);

        float currentSpeed = (Input.GetKey(_sprintKey)) ? _runSpeed : _walkSpeed;
        Vector3 targetVelocity = worldInputDir * currentSpeed;
        _velocity = Vector3.SmoothDamp(_velocity, targetVelocity, ref _smoothVelocity, _smoothMoveTime);

        _verticalVelocity -= _gravity * Time.deltaTime;
        _velocity = new Vector3(_velocity.x, _verticalVelocity, _velocity.z);

        var flags = _controller.Move(_velocity * Time.deltaTime);
        if (flags == CollisionFlags.Below)
        {
            _isJumping = false;
            _lastGroundedTime = Time.time;
            _verticalVelocity = 0;
        }

        if (Input.GetKeyDown(_jumpKey))
        {
            float timeSinceLastTouchedGround = Time.time - _lastGroundedTime;
            if (_controller.isGrounded || (!_isJumping && timeSinceLastTouchedGround < 0.15f))
            {
                _isJumping = true;
                _verticalVelocity = _jumpForce;
            }
        }

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        // Verrrrrry gross hack to stop camera swinging down at start
        float mMag = Mathf.Sqrt(mouseX * mouseX + mouseY * mouseY);
        if (mMag > 5)
        {
            mouseX = 0;
            mouseY = 0;
        }

        _camYaw += mouseX * (_mouseSensitivity / 10);
        _camPitch -= mouseY * (_mouseSensitivity / 10);
        _camPitch = Mathf.Clamp(_camPitch, _pitchMinMax.x, _pitchMinMax.y);

        if (_isTeleporting) return;

        if (_smoothCamera)
        {
            _smoothPitch = Mathf.SmoothDampAngle(_smoothPitch, _camPitch, ref _pitchSmoothVelocity, _camRotateSmoothTime);
            _smoothYaw = Mathf.SmoothDampAngle(_smoothYaw, _camYaw, ref _yawSmoothVelocity, _camRotateSmoothTime);

            transform.eulerAngles = Vector3.up * _smoothYaw;
            _fpCamera.transform.localEulerAngles = Vector3.right * _smoothPitch;
        }
        else
        {
            transform.eulerAngles = Vector3.up * _camYaw;
            _fpCamera.transform.localEulerAngles = Vector3.right * _camPitch;

        }
    }

    #endregion



    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        _controller.enabled = false;

        // Get the difference in rotation based on Euler angles
        Vector3 inEuler = fromPortal.eulerAngles;
        Vector3 outEuler = toPortal.eulerAngles;
        Vector3 eulerDifference = outEuler - inEuler;
        _camYaw += eulerDifference.y;

        float velocitySpeed = _velocity.magnitude;
        Vector3 velocityDirection = _velocity.normalized;

        // Position new Transform in velocity direction of Physics Traveller
        Transform enterVelocity = Instantiate(new GameObject(), transform).transform;
        enterVelocity.name = "DirectionTransform";
        Vector3 localVelocity = Vector3.zero + velocityDirection;
        enterVelocity.LookAt(transform.position + localVelocity);

        transform.position = pos;
        transform.rotation = rot;

        Debug.LogError("I Got Teleported!", this);

        _controller.enabled = true;

        _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 inputDir = new Vector3(_input.x, 0, _input.y);
        Vector3 worldInputDir = transform.TransformDirection(inputDir);

        float currentSpeed = (Input.GetKey(_sprintKey)) ? _runSpeed : _walkSpeed;
        Vector3 targetVelocity = inputDir != Vector3.zero ? worldInputDir * currentSpeed : enterVelocity.forward * velocitySpeed;
        _velocity = targetVelocity;

        _verticalVelocity -= _gravity * Time.deltaTime;
        _velocity = new Vector3(_velocity.x, _verticalVelocity, _velocity.z);

        var flags = _controller.Move(_velocity * Time.deltaTime);
        if (flags == CollisionFlags.Below)
        {
            _isJumping = false;
            _lastGroundedTime = Time.time;
            _verticalVelocity = 0;
        }

        DestroyImmediate(enterVelocity.gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + _velocity);
    }

}
