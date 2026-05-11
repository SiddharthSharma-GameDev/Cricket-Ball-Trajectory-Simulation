using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BounceMarkerRingController : MonoBehaviour
{
    [Header("Ring Shape")]
    [SerializeField] private int segments = 64;
    [SerializeField] private float radius = 0.55f;

    [Header("Pulse Effect")]
    [SerializeField] private bool usePulse = true;
    [SerializeField] private float baseWidth = 0.03f;
    [SerializeField] private float pulseWidthAmount = 0.012f;
    [SerializeField] private float pulseSpeed = 3f;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        CreateRing();
    }

    private void Update()
    {
        if (!usePulse)
        {
            return;
        }

        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseWidthAmount;
        lineRenderer.startWidth = baseWidth + pulse;
        lineRenderer.endWidth = baseWidth + pulse;
    }

    private void CreateRing()
    {
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            lineRenderer.SetPosition(i, new Vector3(x, 0f, z));
        }

        lineRenderer.startWidth = baseWidth;
        lineRenderer.endWidth = baseWidth;
    }
}