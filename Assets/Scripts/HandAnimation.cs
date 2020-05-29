using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandAnimation : MonoBehaviour
{
    // INPUT LEGACY - public string hand; // cambiar a XRNode

    [HideInInspector]
    public Hand hand;

    // INPUT MANAGER CUSTOM - 
    // [HideInInspector]
    // public Hand hand;
    // public InputManager.ButtonOptions fistButton;

    private void Start()
    {
        // INPUT MANAGER CUSTOM - hand = GetComponentInParent<Hand>();
        hand = GetComponentInParent<Hand>();
    }

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("gripButton_" + hand.hand))
        // INPUT MANAGER CUSTOM - if (hand.input.GetButtonDown(fistButton, hand.hand))
        {
            animator.SetBool("Fist", true);
        }
        if (Input.GetButtonUp("gripButton_" + hand.hand))
        // INPUT MANAGER CUSTOM - if (hand.input.GetButtonUp(fistButton, hand.hand))
        {
            animator.SetBool("Fist", false);
        }
    }
}
