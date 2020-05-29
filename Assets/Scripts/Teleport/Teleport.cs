using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Teleport : MonoBehaviour
{
    /* 
    - INPUT LEGACY - 
    public string hand; // cambiar a XRNode
    */

    /* 
    - INPUT MANAGER CUSTOM - 
    [HideInInspector]
    public Hand hand;
    public InputManager.ButtonOptions teleportButton;
    public InputManager.Axis2DOptions teleportEnableAxis;
    */

    [HideInInspector]
    public Hand hand;

    public Animator animFadeImage;
    public BezierCurve bezierCurve;
    public GameObject teleportMarker;

    public bool teleportEnabled;

    private void Start()
    {
        // INPUT MANAGER CUSTOM - hand = GetComponentInParent<Hand>();
        hand = GetComponentInParent<Hand>();

        teleportEnabled = false;
        teleportMarker.SetActive(false);
    }

    void Update()
    {
        ToggleTeleportMode();

        if (teleportEnabled)
        {
            HandleTeleport();
        }
    }

    void UpdateTeleportEnabled()
    {
    }

    void HandleTeleport()
    {
        if (bezierCurve.endPointDetected)
        {
            if (bezierCurve.validTeleport)
            {
                // There is a point to teleport to
                // Display the teleport point.
                teleportMarker.SetActive(true);
                teleportMarker.transform.position = bezierCurve.EndPoint;
                //teleportMarker.transform.position = Vector3.Lerp(teleportMarker.transform.position, bezier.EndPoint, 5 * Time.deltaTime);

                // Teleport to the position                
                // WITH AXIS - if (Input.GetAxis("trigger_" + hand.hand) > 0.3f)
                if (Input.GetButtonDown("triggerButton_" + hand.hand))
                    // INPUT MANAGER CUSTOM - if (hand.input.GetButtonDown(teleportButton, hand.hand))
                    TeleportToPosition(bezierCurve.EndPoint);
            }
            else
                teleportMarker.SetActive(false);
        }
        else
            teleportMarker.SetActive(false);
    }

    void ToggleTeleportMode()
    {
        teleportEnabled = Input.GetAxis("primary2DAxis_Y_" + hand.hand) > 0.75f;
        // INPUT MANAGER CUSTOM - teleportEnabled = hand.input.GetAxis2D(teleportEnableAxis, hand.hand).y > 0.75f;

        bezierCurve.ToggleDraw(teleportEnabled);

        if (!teleportEnabled)
            teleportMarker.SetActive(false);

    }

    void TeleportToPosition(Vector3 teleportPos)
    {
        StartCoroutine(TeleportCO(0.25f, teleportPos));
    }

    IEnumerator TeleportCO(float secs, Vector3 teleportPos)
    {
        animFadeImage.Play("FadeIn");
        yield return new WaitForSeconds(secs);
        teleportMarker.SetActive(false);
        GameObject.FindWithTag("Player").transform.position = teleportPos;
        animFadeImage.Play("FadeOut");
    }
}