using System.Collections;
using UnityEngine;

public class BallTrajectoryController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform ball;
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private Transform bounceMarker;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private BowlingUIController bowlingUIController;
    [SerializeField] private BowlingMeterController bowlingMeterController;

    [Header("Optional Visual References")]
    [SerializeField] private TrailRenderer ballTrail;

    [Header("Delivery Timing")]
    [SerializeField] private float airTravelDuration = 1.1f;
    [SerializeField] private float afterBounceDuration = 0.9f;
    [SerializeField] private float resetDelayAfterDelivery = 1f;

    [Header("Arc Heights")]
    [SerializeField] private float preBounceArcHeight = 0.45f;
    [SerializeField] private float postBounceArcHeight = 0.3f;

    [Header("Swing Settings")]
    [SerializeField] private float maxSwingCurveAmount = 0.75f;

    [Header("Spin Settings")]
    [SerializeField] private float maxSpinTurnAngle = 28f;

    [Header("After Bounce Travel")]
    [SerializeField] private float afterBounceTravelDistance = 3f;

    private bool isBowling;
    private bool isReadyForNextBall = true;
    private float timer;

    private Vector3 startPosition;
    private Vector3 bouncePosition;
    private Vector3 endPosition;
    private Vector3 postBounceDirection;
    private Vector3 swingControlPoint;

    private float lockedStrength01;
    private float directionSign;

    private BowlingUIController.BowlingMode activeMode;

    private enum BallPhase
    {
        Idle,
        BeforeBounce,
        AfterBounce
    }

    private BallPhase currentPhase = BallPhase.Idle;

    public bool IsBowling => isBowling;
    public bool IsReadyForNextBall => isReadyForNextBall;

    public float PreBounceArcHeight => preBounceArcHeight;
    public float PostBounceArcHeight => postBounceArcHeight;
    public float AfterBounceTravelDistance => afterBounceTravelDistance;

    private void Start()
    {
        if (ballTrail == null && ball != null)
        {
            ballTrail = ball.GetComponent<TrailRenderer>();
        }

        ResetBall();
        isReadyForNextBall = true;
    }

    public void TryBowlSelectedDelivery()
    {
        if (isBowling || !isReadyForNextBall)
        {
            return;
        }

        if (bowlingUIController.CurrentMode == BowlingUIController.BowlingMode.None)
        {
            Debug.LogWarning("Select Swing or Spin before bowling.");
            return;
        }

        bowlingMeterController.LockMeter();
        bowlingUIController.SetControlsInteractable(false);

        if (ballTrail != null)
        {
            ballTrail.Clear();
            ballTrail.emitting = true;
        }

        isReadyForNextBall = false;

        BuildTrajectoryData(
            bowlingUIController.CurrentMode,
            bowlingUIController.CurrentSide,
            bowlingMeterController.GetLockedStrengthPercent() / 100f
        );

        ball.position = startPosition;

        timer = 0f;
        isBowling = true;
        currentPhase = BallPhase.BeforeBounce;
    }

    private void Update()
    {
        if (!isBowling)
        {
            return;
        }

        if (currentPhase == BallPhase.BeforeBounce)
        {
            MoveBeforeBounce();
        }
        else if (currentPhase == BallPhase.AfterBounce)
        {
            MoveAfterBounce();
        }
    }

    private void MoveBeforeBounce()
    {
        timer += Time.deltaTime;

        float t = timer / airTravelDuration;
        t = Mathf.Clamp01(t);

        ball.position = GetPreBouncePosition(t);

        if (t >= 1f)
        {
            timer = 0f;
            ball.position = bouncePosition;
            currentPhase = BallPhase.AfterBounce;
        }
    }

    private void MoveAfterBounce()
    {
        timer += Time.deltaTime;

        float t = timer / afterBounceDuration;
        t = Mathf.Clamp01(t);

        ball.position = GetPostBouncePosition(t);

        if (t >= 1f)
        {
            isBowling = false;
            currentPhase = BallPhase.Idle;
            StartCoroutine(ResetAfterDelay());
        }
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelayAfterDelivery);

        ResetBall();

        bowlingMeterController.ResetMeter();
        bowlingUIController.SetControlsInteractable(true);

        isReadyForNextBall = true;
    }

    private void BuildTrajectoryData(
        BowlingUIController.BowlingMode mode,
        BowlingUIController.BowlingSide side,
        float strength01
    )
    {
        startPosition = ballSpawnPoint.position;

        float ballRadius = GetBallRadius();
        float markerTopY = GetMarkerTopY();

        bouncePosition = new Vector3(
            bounceMarker.position.x,
            markerTopY + ballRadius,
            bounceMarker.position.z
        );

        lockedStrength01 = strength01;
        activeMode = mode;

        directionSign = side == BowlingUIController.BowlingSide.Left ? 1f : -1f;

        if (activeMode == BowlingUIController.BowlingMode.Swing)
        {
            CreateSwingControlPoint();
            postBounceDirection = CalculateSwingTangentDirectionAtBounce();
        }
        else if (activeMode == BowlingUIController.BowlingMode.Spin)
        {
            postBounceDirection = CalculateSpinDirectionAfterBounce();
        }
        else
        {
            postBounceDirection = CalculateStraightDirectionAfterBounce();
        }

        endPosition = bouncePosition + postBounceDirection * afterBounceTravelDistance;
    }

    private Vector3 GetPreBouncePosition(float t)
    {
        Vector3 currentPosition;

        if (activeMode == BowlingUIController.BowlingMode.Swing)
        {
            currentPosition = CalculateQuadraticBezierPoint(
                startPosition,
                swingControlPoint,
                bouncePosition,
                t
            );
        }
        else
        {
            currentPosition = Vector3.Lerp(startPosition, bouncePosition, t);
        }

        float arcHeight = Mathf.Sin(t * Mathf.PI) * preBounceArcHeight;
        currentPosition.y += arcHeight;

        return currentPosition;
    }

    private Vector3 GetPostBouncePosition(float t)
    {
        Vector3 currentPosition = Vector3.Lerp(bouncePosition, endPosition, t);

        float bounceArc = Mathf.Sin(t * Mathf.PI) * postBounceArcHeight;
        currentPosition.y += bounceArc;

        return currentPosition;
    }

    private void CreateSwingControlPoint()
    {
        Vector3 midpoint = Vector3.Lerp(startPosition, bouncePosition, 0.5f);

        float swingAmount = maxSwingCurveAmount * lockedStrength01 * directionSign;

        swingControlPoint = new Vector3(
            midpoint.x + swingAmount,
            midpoint.y,
            midpoint.z
        );
    }

    private Vector3 CalculateSwingTangentDirectionAtBounce()
    {
        Vector3 tangent = bouncePosition - swingControlPoint;
        tangent.y = 0f;
        return tangent.normalized;
    }

    private Vector3 CalculateSpinDirectionAfterBounce()
    {
        Vector3 incomingHorizontalDirection = bouncePosition - startPosition;
        incomingHorizontalDirection.y = 0f;
        incomingHorizontalDirection.Normalize();

        float spinAngle = maxSpinTurnAngle * lockedStrength01 * directionSign;

        Vector3 turnedDirection = Quaternion.AngleAxis(spinAngle, Vector3.up) * incomingHorizontalDirection;
        turnedDirection.y = 0f;

        return turnedDirection.normalized;
    }

    private Vector3 CalculateStraightDirectionAfterBounce()
    {
        Vector3 direction = targetPoint.position - bouncePosition;
        direction.y = 0f;
        return direction.normalized;
    }

    private Vector3 CalculateQuadraticBezierPoint(Vector3 pointA, Vector3 pointB, Vector3 pointC, float t)
    {
        float oneMinusT = 1f - t;

        return
            oneMinusT * oneMinusT * pointA +
            2f * oneMinusT * t * pointB +
            t * t * pointC;
    }

    private float GetBallRadius()
    {
        SphereCollider sphereCollider = ball.GetComponent<SphereCollider>();

        if (sphereCollider != null)
        {
            float largestScale = Mathf.Max(
                ball.lossyScale.x,
                ball.lossyScale.y,
                ball.lossyScale.z
            );

            return sphereCollider.radius * largestScale;
        }

        return 0.09f;
    }

    private float GetMarkerTopY()
    {
        float markerHalfHeight = bounceMarker.lossyScale.y;
        return bounceMarker.position.y + markerHalfHeight;
    }

    public Vector3[] GeneratePreviewPoints(int preBouncePointCount, int postBouncePointCount)
    {
        BowlingUIController.BowlingMode previewMode = bowlingUIController.CurrentMode;
        BowlingUIController.BowlingSide previewSide = bowlingUIController.CurrentSide;

        float previewStrength01;

        if (bowlingMeterController.IsMeterRunning())
        {
            previewStrength01 = bowlingMeterController.GetCurrentStrengthPercent() / 100f;
        }
        else
        {
            previewStrength01 = bowlingMeterController.GetLockedStrengthPercent() / 100f;
        }

        BuildTrajectoryData(previewMode, previewSide, previewStrength01);

        int totalPointCount = preBouncePointCount + postBouncePointCount;
        Vector3[] points = new Vector3[totalPointCount];

        for (int i = 0; i < preBouncePointCount; i++)
        {
            float t = i / (float)(preBouncePointCount - 1);
            points[i] = GetPreBouncePosition(t);
        }

        for (int i = 0; i < postBouncePointCount; i++)
        {
            float t = i / (float)(postBouncePointCount - 1);
            points[preBouncePointCount + i] = GetPostBouncePosition(t);
        }

        return points;
    }

    public void ResetBall()
    {
        isBowling = false;
        currentPhase = BallPhase.Idle;

        if (ballTrail != null)
        {
            ballTrail.emitting = false;
            ballTrail.Clear();
        }

        if (ball != null && ballSpawnPoint != null)
        {
            ball.position = ballSpawnPoint.position;
            ball.rotation = ballSpawnPoint.rotation;
        }

        if (ballTrail != null)
        {
            ballTrail.Clear();
        }
    }
}