using TMPro;
using UnityEngine;

public class BowlingMeterController : MonoBehaviour
{
    [Header("Meter References")]
    [SerializeField] private RectTransform meterPanel;
    [SerializeField] private RectTransform meterIndicator;
    [SerializeField] private TextMeshProUGUI strengthText;

    [Header("Meter Settings")]
    [SerializeField] private float indicatorSpeed = 280f;
    [SerializeField] private float verticalPadding = 10f;

    [Header("Runtime Values")]
    [Range(0f, 100f)]
    [SerializeField] private float currentStrengthPercent;

    [Range(0f, 100f)]
    [SerializeField] private float lockedStrengthPercent;

    [SerializeField] private bool isMeterRunning = true;

    private float movementDirection = 1f;
    private float maxTravelY;

    private void Start()
    {
        CalculateTravelLimit();
        UpdateStrengthValue();
    }

    private void Update()
    {
        if (isMeterRunning)
        {
            MoveIndicator();
            UpdateStrengthValue();
        }
    }

    private void CalculateTravelLimit()
    {
        float halfPanelHeight = meterPanel.rect.height * 0.5f;
        float halfIndicatorHeight = meterIndicator.rect.height * 0.5f;

        maxTravelY = halfPanelHeight - halfIndicatorHeight - verticalPadding;
    }

    private void MoveIndicator()
    {
        Vector2 currentPosition = meterIndicator.anchoredPosition;

        currentPosition.y += movementDirection * indicatorSpeed * Time.deltaTime;

        if (currentPosition.y >= maxTravelY)
        {
            currentPosition.y = maxTravelY;
            movementDirection = -1f;
        }
        else if (currentPosition.y <= -maxTravelY)
        {
            currentPosition.y = -maxTravelY;
            movementDirection = 1f;
        }

        meterIndicator.anchoredPosition = currentPosition;
    }

    private void UpdateStrengthValue()
    {
        float distanceFromCenter = Mathf.Abs(meterIndicator.anchoredPosition.y);
        float normalizedDistance = distanceFromCenter / maxTravelY;

        currentStrengthPercent = Mathf.Lerp(100f, 0f, normalizedDistance);

        if (strengthText != null && isMeterRunning)
        {
            strengthText.text = "Strength: " + Mathf.RoundToInt(currentStrengthPercent) + "%";
        }
    }

    public void LockMeter()
    {
        if (!isMeterRunning)
        {
            return;
        }

        isMeterRunning = false;
        lockedStrengthPercent = currentStrengthPercent;

        if (strengthText != null)
        {
            strengthText.text = "Locked: " + Mathf.RoundToInt(lockedStrengthPercent) + "%";
        }
    }

    public void ResetMeter()
    {
        isMeterRunning = true;
        lockedStrengthPercent = 0f;

        UpdateStrengthValue();
    }

    public float GetCurrentStrengthPercent()
    {
        return currentStrengthPercent;
    }

    public float GetLockedStrengthPercent()
    {
        return lockedStrengthPercent;
    }

    public bool IsMeterRunning()
    {
        return isMeterRunning;
    }
}