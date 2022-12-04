using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Template Setup by James
[RequireComponent(typeof(Collider))]
public class OnTrigger : MonoBehaviour
{
	[SerializeField] string _targetTag;

	private Collider _collider;

	public UnityEvent TriggerEnterEvent = new UnityEvent();
    public UnityEvent TriggerStayEvent = new UnityEvent();
    public UnityEvent TriggerExitEvent = new UnityEvent();

    void Awake()
	{
		_collider = GetComponent<Collider>();
		_collider.isTrigger = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.LogWarning("Detected Enter");
		if (!other.CompareTag(_targetTag)) return;

        Debug.LogWarning("Player Entered");

        TriggerEnterEvent.Invoke();
	}

	private void OnTriggerStay(Collider other)
	{
        if (!other.CompareTag(_targetTag)) return;

        TriggerStayEvent.Invoke();
	}

	private void OnTriggerExit(Collider other)
	{
        if (!other.CompareTag(_targetTag)) return;

        TriggerExitEvent.Invoke();
	}
}
