using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class DeathScreenUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show(float duration)
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        StartCoroutine(CountdownCoroutine(duration));
    }

    public void Hide()
    {
        StopAllCoroutines();

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator CountdownCoroutine(float duration)
    {
        float timer = duration;
        while (timer > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.CeilToInt(timer).ToString();
            }
            timer -= Time.deltaTime;
            yield return null;
        }
    }
}