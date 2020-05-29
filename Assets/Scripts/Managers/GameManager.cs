using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager GM;

    // VARIABLES GLOBALES
    public bool pointingCP;

    void Awake()
    {
        GM = this;
    }

    void Update()
    {

    }
}
