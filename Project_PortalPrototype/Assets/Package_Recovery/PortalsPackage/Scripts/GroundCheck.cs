using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GroundCheck : MonoBehaviour
{
	private Collider _coll;
	private PlayerController _player;

    void Awake()
    {
        _coll = GetComponent<Collider>();
        _coll.isTrigger = true;

        _player = GetComponent<PlayerController>();
        if (_player == null) Debug.LogError("No Player Controller Found!!!", this);
    }

    void OnTriggerEnter(Collider other)
    {
        // IF Trigger hits the floor
        if (other.gameObject.layer == 6)
        {
            _player.SetCanJump(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // IF Trigger exits the floor
        if (other.gameObject.layer == 6)
        {
            _player.SetCanJump(false);
        }
    }
}
