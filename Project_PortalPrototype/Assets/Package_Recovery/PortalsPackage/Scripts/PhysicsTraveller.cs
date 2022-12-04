using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsTraveller : PortalTraveller
{
    public Rigidbody Rb { get; private set; }
    public Transform VelocityT;

    public Vector3 eulerAngle;

    public ParticleSystem teleportPS;
    public ParticleSystem impactPS;

    void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        //VelocityT = Instantiate(new GameObject(), transform).transform;
        //VelocityT.name = "EnterVelocity";
    }



    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        var ps = Instantiate(teleportPS, transform.position, Quaternion.identity);
        

        float speed = Rb.velocity.magnitude;
        Vector3 velocityDirection = Rb.velocity.normalized;
        float angularSpeed = Rb.angularVelocity.magnitude;
        Vector3 angularDirection = Rb.angularVelocity.normalized;

        // Position new Transform in velocity direction of Physics Traveller
        Vector3 localVelocity = Vector3.zero + velocityDirection;
        VelocityT.LookAt(transform.position + localVelocity);

        Rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        Rb.isKinematic = true;

        // Move and Rotate Transform towards exit portal values
        transform.position = pos;
        transform.rotation = rot;

        Rb.isKinematic = false;

        Rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Rb.velocity = VelocityT.forward * speed;
        Rb.angularVelocity = angularDirection * angularSpeed;

    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;

        Instantiate(impactPS, pos, rot);

    }

    private void OnDrawGizmos()
    {
        if (Rb == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Rb.velocity);
    }

}
