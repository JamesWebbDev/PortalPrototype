using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _moveAcceleration = 10;
    [SerializeField] float _maxSpeed = 2;
    [SerializeField] float _jumpForce;

	private Collider _groundColl;
	private Rigidbody _rb;

    private Camera _camera;
    private Vector2 _mouseDelta;
    private float _camXRotation;


    private Vector3 _camEulerAngles;

    private Vector2 _input;

    private bool _canJump = true;

    void Awake()
	{
		_groundColl = GetComponentInChildren<Collider>();
		_rb = GetComponent<Rigidbody>();
        _camera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    void Update()
    {
        GetMoveInput();
        GetJumpInput();
        GetMouseInput();
        MoveCamera();
    }

    void FixedUpdate()
    {
        //TMove();
        Move();
        ClampMove();
        
    }

    private void ClampMove()
    {

        Vector3 localVelocity = transform.InverseTransformDirection(_rb.velocity);

        localVelocity.x *= 0.6f;
        localVelocity.z *= 0.6f;

        _rb.velocity = transform.TransformDirection(localVelocity);
    }

    private void Move()
    {
        if (_input.magnitude <= 0) return;

        //Debug.Log("Moving");

        Vector3 moveVector = GetMoveVector();

        _rb.AddForce(moveVector * _moveAcceleration * 50, ForceMode.Force);
    }

    private void Jump()
    {
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
    }

    void GetMoveInput()
    {
        float xInput = Input.GetAxis("Horizontal");
        float yInput = Input.GetAxis("Vertical");

        _input = new Vector2(xInput, yInput);
    }

    void GetJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _canJump)
        {
            Jump();
        }
    }

    void GetMouseInput()
    {
        _mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    void MoveCamera()
    {

        //Debug.Log("Mouse Delta is " + _mouseDelta);

        _camXRotation -= _mouseDelta.y;
        _camXRotation = Mathf.Clamp(_camXRotation, -60f, 60f);
        //Debug.Log("Cam X Rotation is " + _camXRotation);

        // Turn up and Down
        _camera.transform.localRotation = Quaternion.Euler(_camXRotation, 0f, 0f);
        // Turn Left and Right
        transform.Rotate(Vector3.up * _mouseDelta.x);
    }

    Vector3 GetMoveVector()
    {
        return transform.right * _input.x + transform.forward * _input.y;
    }

    public void SetCanJump(bool value)
    {
        _canJump = value;
    }
}
