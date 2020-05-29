using UnityEngine;

public class CanvasRay : MonoBehaviour
{
    public float length = 10;
    public LineRenderer lineRenderer = null;
    public Transform rayOrigin;
    public LayerMask interact;
    public bool pointingCP;
    //public GameObject dot;

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, length * 1000, interact)) //Mathf.Infinity
        {
            pointingCP = true;
            lineRenderer.gameObject.SetActive(true);
            lineRenderer.SetPosition(0, rayOrigin.position);
            lineRenderer.SetPosition(1, hit.point);

            GameManager.GM.pointingCP = true;
        }
        else
        {
            pointingCP = false;
            lineRenderer.gameObject.SetActive(false);
            //lineRenderer.SetPosition(1, rayOrigin.forward * length);

            GameManager.GM.pointingCP = false;
        }
    }

}
