using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPreviewController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BallTrajectoryController ballTrajectoryController;
    [SerializeField] private BowlingUIController bowlingUIController;

    [Header("Preview Settings")]
    [SerializeField] private int preBouncePointCount = 20;
    [SerializeField] private int postBouncePointCount = 15;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (ballTrajectoryController == null || bowlingUIController == null)
        {
            HidePreview();
            return;
        }

        if (ballTrajectoryController.IsBowling ||
            !ballTrajectoryController.IsReadyForNextBall ||
            bowlingUIController.CurrentMode == BowlingUIController.BowlingMode.None)
        {
            HidePreview();
            return;
        }

        Vector3[] previewPoints = ballTrajectoryController.GeneratePreviewPoints(
            preBouncePointCount,
            postBouncePointCount
        );

        lineRenderer.enabled = true;
        lineRenderer.positionCount = previewPoints.Length;
        lineRenderer.SetPositions(previewPoints);
    }

    private void HidePreview()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }
    }
}