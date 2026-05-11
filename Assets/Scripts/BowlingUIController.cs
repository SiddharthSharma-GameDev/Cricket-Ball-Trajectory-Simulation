using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BowlingUIController : MonoBehaviour
{
    public enum BowlingMode
    {
        None,
        Swing,
        Spin
    }

    public enum BowlingSide
    {
        Left,
        Right
    }

    [Header("Current Selection")]
    [SerializeField] private BowlingMode currentMode = BowlingMode.None;
    [SerializeField] private BowlingSide currentSide = BowlingSide.Left;

    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI currentModeText;
    [SerializeField] private TextMeshProUGUI bowlingSideText;
    [SerializeField] private TextMeshProUGUI deliveryTypeText;

    [Header("Button References")]
    [SerializeField] private Button swingButton;
    [SerializeField] private Button spinButton;
    [SerializeField] private Button bowlButton;
    [SerializeField] private Button changeSideButton;

    [Header("Button Colors")]
    [SerializeField] private Color swingNormalColor = new Color(0f, 0.67f, 1f);
    [SerializeField] private Color spinNormalColor = new Color(0.59f, 0.35f, 1f);
    [SerializeField] private Color selectedColor = Color.black;

    public BowlingMode CurrentMode => currentMode;
    public BowlingSide CurrentSide => currentSide;

    private void Start()
    {
        UpdateModeText();
        UpdateSideText();
        UpdateDeliveryTypeText();
        UpdateModeButtonVisuals();
    }

    public void SelectSwingMode()
    {
        currentMode = BowlingMode.Swing;
        UpdateModeText();
        UpdateDeliveryTypeText();
        UpdateModeButtonVisuals();
    }

    public void SelectSpinMode()
    {
        currentMode = BowlingMode.Spin;
        UpdateModeText();
        UpdateDeliveryTypeText();
        UpdateModeButtonVisuals();
    }

    public void ChangeBowlingSide()
    {
        if (currentSide == BowlingSide.Left)
        {
            currentSide = BowlingSide.Right;
        }
        else
        {
            currentSide = BowlingSide.Left;
        }

        UpdateSideText();
        UpdateDeliveryTypeText();
    }

    public void SetControlsInteractable(bool interactable)
    {
        if (swingButton != null)
        {
            swingButton.interactable = interactable;
        }

        if (spinButton != null)
        {
            spinButton.interactable = interactable;
        }

        if (bowlButton != null)
        {
            bowlButton.interactable = interactable;
        }

        if (changeSideButton != null)
        {
            changeSideButton.interactable = interactable;
        }
    }

    private void UpdateModeText()
    {
        if (currentModeText != null)
        {
            currentModeText.text = "Mode: " + currentMode;
        }
    }

    private void UpdateSideText()
    {
        if (bowlingSideText != null)
        {
            bowlingSideText.text = "Side: " + currentSide;
        }
    }

    private void UpdateDeliveryTypeText()
    {
        if (deliveryTypeText == null)
        {
            return;
        }

        if (currentMode == BowlingMode.None)
        {
            deliveryTypeText.text = "Delivery: None";
            return;
        }

        deliveryTypeText.text = "Delivery: " + currentSide + " " + currentMode;
    }

    private void UpdateModeButtonVisuals()
    {
        if (swingButton != null)
        {
            swingButton.image.color =
                currentMode == BowlingMode.Swing ? selectedColor : swingNormalColor;
        }

        if (spinButton != null)
        {
            spinButton.image.color =
                currentMode == BowlingMode.Spin ? selectedColor : spinNormalColor;
        }
    }
}