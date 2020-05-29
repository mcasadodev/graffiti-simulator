using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]

public class BezierCurve : MonoBehaviour
{

    public bool endPointDetected;
    public LayerMask teleportMask;

    public float raycastDistance;

    //public GameObject teleportCircle;

    public Vector3 EndPoint
    {
        get { return endpoint; }
    }

    public float ExtensionFactor
    {
        set { extensionFactor = value; }
    }

    public Vector3 endpoint;
    private float extensionFactor;
    public Vector3[] controlPoints;
    public LineRenderer lineRenderer;
    private float extendStep;
    private int SEGMENT_COUNT = 50;

    public bool validTeleport;
    public Gradient validColor, invalidColor;

    void Start()
    {
        controlPoints = new Vector3[3];
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        extendStep = 5f;
        extensionFactor = 0f;
    }

    void Update()
    {
        UpdateControlPoints();
        HandleExtension();
        DrawCurve();

        if (validTeleport)
            lineRenderer.colorGradient = validColor;
        else
            lineRenderer.colorGradient = invalidColor;
    }

    public void ToggleDraw(bool draw)
    {
        lineRenderer.enabled = draw;
    }


    void HandleExtension()
    {
        if (extensionFactor == 0f)
            return;

        float finalExtension = extendStep + Time.deltaTime * extensionFactor * 2f;
        extendStep = Mathf.Clamp(finalExtension, 2.5f, 7.5f);
    }

    // The first control is the beggining. The second is a forward projection. The third is a forward and downward projection.
    void UpdateControlPoints()
    {
        controlPoints[0] = gameObject.transform.position; // Get Controller Position
        controlPoints[1] = controlPoints[0] + (gameObject.transform.forward * (raycastDistance / 2));
        controlPoints[2] = controlPoints[1] + (gameObject.transform.forward * raycastDistance) + Vector3.up * -35;
    }

    // Draw the bezier curve.
    void DrawCurve()
    {
        if (!lineRenderer.enabled)
            return;
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, controlPoints[0]);

        Vector3 prevPosition = controlPoints[0];
        Vector3 nextPosition = prevPosition;
        for (int i = 1; i <= SEGMENT_COUNT; i++)
        {
            float t = i / (float)SEGMENT_COUNT;
            lineRenderer.positionCount = i + 1;

            if (i == SEGMENT_COUNT)
            {
                // For the last point, project out the curve two more meters.
                Vector3 endDirection = Vector3.Normalize(prevPosition - lineRenderer.GetPosition(i - 2));
                nextPosition = prevPosition + endDirection * 2f;
            }
            else
            {
                nextPosition = CalculateBezierPoint(t, controlPoints[0], controlPoints[1], controlPoints[2]);
            }

            if (CheckColliderIntersection(prevPosition, nextPosition))
            {
                // If the segment intersects a surface, draw the point and return.
                lineRenderer.SetPosition(i, endpoint);
                endPointDetected = true;
                return;
            }
            else
            {
                // If the point does not intersect, continue to draw the curve.
                lineRenderer.SetPosition(i, nextPosition);
                endPointDetected = false;
                prevPosition = nextPosition;
            }
        }
    }

    // Check if the line between start and end intersect a collider.
    bool CheckColliderIntersection(Vector3 start, Vector3 end)
    {
        Ray r = new Ray(start, end - start);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, Vector3.Distance(start, end)))
        {
            // HAY QUE CURRARSELO UN POCO MAS PARA QUE ATRAVIESE COLISIONES INDESEADAS COMO LA DE PLAYER (MANOS, POR EJEMPLO)
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Teleport")) // || hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                validTeleport = true;
                //teleportCircle.transform.forward = hit.normal;
            }
            else
                validTeleport = false;

            endpoint = hit.point;
            return true;
        }

        return false;
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return
            Mathf.Pow((1f - t), 2) * p0 +
            2f * (1f - t) * t * p1 +
            Mathf.Pow(t, 2) * p2;
    }
}
