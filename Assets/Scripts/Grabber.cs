using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Grabber : MonoBehaviour
{

    // INPUT LEGACY - public string hand; // cambiar a XRNode

    [HideInInspector]
    public Hand hand;

    // INPUT MANAGER CUSTOM - 
    // [HideInInspector]
    // public Hand hand;
    // public InputManager.ButtonOptions gripButton;

    private void Start()
    {
        // INPUT MANAGER CUSTOM - hand = GetComponentInParent<Hand>();
        hand = GetComponentInParent<Hand>();
    }

    public bool isGripping;
    public GameObject[] snapPositions;

    [HideInInspector]
    public Grabbable itemGrabbed;
    [HideInInspector]
    public List<Grabbable> itemsInReach = new List<Grabbable>();

    // keep track of the controller velocity for throwing
    Vector3 ctrlVelocity;
    Vector3 prevPosition;

    private Quaternion lastRotation, currentRotation;

    private void Awake()
    {
        // INPUT MANAGER CUSTOM - hand = GetComponentInParent<Hand>().hand;
        hand = GetComponentInParent<Hand>();
    }


    private void Update()
    {
        if (Input.GetButtonDown("gripButton_" + hand.hand))
            // INPUT MANAGER CUSTOM - if (hand.input.GetButtonDown(gripButton, hand.hand))
            Grab();

        if (Input.GetButtonUp("gripButton_" + hand.hand))
            // INPUT MANAGER CUSTOM - if (hand.input.GetButtonUp(gripButton, hand.hand))
            Release();
    }

    void FixedUpdate()
    {

        if (itemGrabbed && itemGrabbed.releaseAction == Grabbable.ReleaseAction.throws)
        {
            ctrlVelocity = (transform.position - prevPosition) / Time.fixedDeltaTime;
            prevPosition = transform.position;

            lastRotation = currentRotation;
            currentRotation = transform.rotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Grabbable>())
            itemsInReach.Add(other.gameObject.GetComponent<Grabbable>());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Grabbable>())
            itemsInReach.Remove(other.gameObject.GetComponent<Grabbable>());
    }

    private Grabbable GetNearestGrabbable()
    {
        Grabbable nearest = null;

        float maxDist = 1000; // UN NUMERO MUY GRANDE

        foreach (Grabbable item in itemsInReach)
        {
            float distance = (item.transform.position - this.transform.position).magnitude;
            if (distance < maxDist)
            {
                if (nearest != item)
                {
                    nearest = item;
                    maxDist = distance;
                }
            }
        }

        return nearest;
    }

    private void Grab()
    {
        // Asignar el grabbable mas cercano
        itemGrabbed = GetNearestGrabbable();

        // Comprobar nulidad
        if (!itemGrabbed) return;

        itemGrabbed.isGrabbed = true;

        // Comprobar que no este ya cogido y si lo esta que se suelte
        if (itemGrabbed.grabCtrl)
            itemGrabbed.grabCtrl.ChangeHand();

        // set as child of the controller
        itemGrabbed.transform.parent = this.transform;
        // set rigidbody to kinematic
        itemGrabbed.rb.isKinematic = true;
        // Asignar esta mano al objeto
        itemGrabbed.grabCtrl = this;

        // asignar posicion
        if (itemGrabbed.grabAction == Grabbable.GrabAction.snapToPosition)
        {
            itemGrabbed.transform.position = snapPositions[itemGrabbed.snapPosition].transform.position;
            itemGrabbed.transform.rotation = snapPositions[itemGrabbed.snapPosition].transform.rotation;
        }
        // face to the same direction as the controller
        else if (itemGrabbed.grabAction == Grabbable.GrabAction.facesForward)
        {
            itemGrabbed.transform.forward = transform.forward;
        }

    }

    private void Release()
    {
        isGripping = false;

        // Comprobar si hay algo cogido
        if (!itemGrabbed)
            return;

        // return to it's original parent if any
        itemGrabbed.transform.parent = itemGrabbed.originalParent;

        // restore rigid body
        itemGrabbed.rb.isKinematic = itemGrabbed.isKinematic;

        // handle release actions
        switch (itemGrabbed.releaseAction)
        {
            case Grabbable.ReleaseAction.backToOrigin:
                BackToOrigin();
                break;
            case Grabbable.ReleaseAction.throws:
                ThrowItem();
                break;
        }

        // No poner item.isGrabbed a false si lo ha cogido la otra mano
        itemGrabbed.isGrabbed = false;

        // Poner a null la mano para ese objeto 
        itemGrabbed.grabCtrl = null;
        // Poner a null el objeto cogido y 
        itemGrabbed = null;
    }

    private void ChangeHand()
    {
        isGripping = false;

        // Poner a null la mano para ese objeto 
        itemGrabbed.grabCtrl = null;

        // Poner a null el objeto cogido y 
        itemGrabbed = null;
    }

    void BackToOrigin()
    {
        print("setting back to original pos");
        itemGrabbed.transform.position = itemGrabbed.originalPosition;
        itemGrabbed.transform.rotation = itemGrabbed.originalRotation;
    }

    void ThrowItem()
    {
        print(hand + " is throwing");

        // needs a non-kinematic RB
        itemGrabbed.rb.isKinematic = false;

        // set controller velocity
        itemGrabbed.rb.velocity = ctrlVelocity * itemGrabbed.velocityMultiplier;
        itemGrabbed.rb.angularVelocity = GetAngularVelocity() * itemGrabbed.angularVelocityMultiplier;
    }

    Vector3 GetAngularVelocity()
    {
        Quaternion deltaRotation = currentRotation * Quaternion.Inverse(lastRotation);
        return new Vector3(Mathf.DeltaAngle(0, deltaRotation.eulerAngles.x), Mathf.DeltaAngle(0, deltaRotation.eulerAngles.y), Mathf.DeltaAngle(0, deltaRotation.eulerAngles.z));
    }

}
