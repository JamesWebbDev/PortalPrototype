using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushPhysicsObject : MonoBehaviour
{
	[SerializeField] Rigidbody _rb;
    [SerializeField] float _force;
    [SerializeField] KeyCode _key;

    void Update()
    {
        if (Input.GetKey(_key))
        {
            var direction = _rb.velocity.normalized;
            _rb.AddForce(direction * _force, ForceMode.Force);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

}
