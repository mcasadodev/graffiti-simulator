using UnityEngine;

public class ShowPallete : MonoBehaviour
{
    public GameObject canvasPallete;

    void Update()
    {
        canvasPallete.transform.localScale = Vector3.one * (0.000001f + 0.002499f * Input.GetAxis("grip_L"));
    }
}