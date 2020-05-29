using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Hand : MonoBehaviour
{
    public string hand, oppositeHand;

    // [HideInInspector]
    // public InputManager input, inputOpposite;
    // public XRNode handIM, oppositeHandIM;
    // public InputManager.ButtonOptions button1, button2;

    private void Awake()
    {
        // input = new InputManager(handIM);
        // inputOpposite = new InputManager(oppositeHand);
    }

    private void Update()
    {
        // Debug.Log("Button 1 - ButtonDown: " + input.GetButtonDown(button1, handIM));
        // Debug.Log("Button 2 - ButtonDown: " + input.GetButtonDown(button2, handIM));
        // Debug.Log("Button 1 - ButtonUp: " + input.GetButtonUp(button1, handIM));
        // Debug.Log("Button 2 - ButtonUp: " + input.GetButtonUp(button2, handIM));

        // if (input.GetButtonDown(button1, handIM))
        // {
        //     cube.SetActive(true);
        // }
        // if (input.GetButtonDown(button2, handIM))
        // {
        //     cube.SetActive(true);
        // }
        // if (input.GetButtonUp(button1, handIM))
        // {
        //     cube.SetActive(false);
        // }
        // if (input.GetButtonUp(button2, handIM))
        // {
        //     cube.SetActive(false);
        // }
    }
}
