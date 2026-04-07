using UnityEngine;
using UnityEngine.UI;

public class HarvestUI : MonoBehaviour
{
    public Image fillImage;
    public Text messageText;
    public string holdMessage = "Hold button to harvest";

    private void Awake()
    {
        if (messageText != null)
            messageText.text = holdMessage;
    }

    public void SetProgress(float value)
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(value);
    }

    public void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }

    public void Show(bool state)
    {
        if (messageText != null)
            messageText.text = holdMessage;

        if (fillImage != null)
            fillImage.fillAmount = 0f;

        gameObject.SetActive(state);
    }
}