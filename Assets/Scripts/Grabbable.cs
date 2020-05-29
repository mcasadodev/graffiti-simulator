using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
    // options when grabbing the item
    public enum GrabAction { nothing, snapToPosition, facesForward }
    // options when releasing the item
    public enum ReleaseAction { nothing, backToOrigin, throws }

    public int snapPosition = 0;

    [Tooltip("What happens when grabbing")]
    public GrabAction grabAction;

    [Tooltip("What happens when releasing")]
    public ReleaseAction releaseAction;

    public float velocityMultiplier = 1;
    public float angularVelocityMultiplier = 3;

    // who is grabbing this
    //[HideInInspector]
    public Grabber grabCtrl;

    [HideInInspector]
    public bool isGrabbed;

    // original position and rotation
    [HideInInspector]
    public Vector3 originalPosition;
    [HideInInspector]
    public Quaternion originalRotation;

    // original parent
    [HideInInspector]
    public Transform originalParent;

    // rigid body
    [HideInInspector]
    public Rigidbody rb;

    // original kinematic status
    [HideInInspector]
    public bool isKinematic;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        isKinematic = rb.isKinematic;

        //if (transform.parent)
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }
}