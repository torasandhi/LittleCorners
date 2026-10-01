using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LevelCompleteUI : MonoBehaviour
{
    [Tooltip("Order: Level 1, Level 2, Level 3")]
    [SerializeField] private Sprite[] levelImages;

    [Header("Animation")]
    [SerializeField] private float zoomOutAmount = 1.5f;
    [SerializeField] private float zoomDuration = 0.65f;
    [SerializeField] private float panelDuration = 0.35f;
    [SerializeField] private float imageFadeDuration = 0.65f;

    private GameObject overlay;
    private CanvasGroup overlayGroup;
    private CanvasGroup imageGroup;
    private RectTransform dialog;
    private Image completedLevelImage;
    private Text titleText;
    private Button continueButton;
    private Button mainMenuButton;
    private Coroutine introSequence;

    private Camera sequenceCamera;
    private OrthographicZoom zoomController;
    private bool zoomControllerWasEnabled;

    private void Awake()
    {
        BuildUI();
    }

    public void Show(int levelBuildIndex)
    {
        BuildUI();
        EnsureEventSystem();

        if (introSequence != null)
            StopCoroutine(introSequence);

        int imageIndex = levelBuildIndex - 1;
        completedLevelImage.sprite = imageIndex >= 0 && imageIndex < levelImages.Length
            ? levelImages[imageIndex]
            : null;

        titleText.text = $"LEVEL {levelBuildIndex} CLEARED";

        overlay.SetActive(true);
        overlayGroup.alpha = 0f;
        overlayGroup.interactable = false;
        overlayGroup.blocksRaycasts = true;
        imageGroup.alpha = 0f;
        dialog.localScale = Vector3.one * 0.88f;

        continueButton.interactable = false;
        mainMenuButton.interactable = false;

        // Freeze gameplay while still allowing the UI animation to use unscaled time.
        Time.timeScale = 0f;
        introSequence = StartCoroutine(PlayIntroSequence());
    }

    public void HideImmediately()
    {
        if (introSequence != null)
        {
            StopCoroutine(introSequence);
            introSequence = null;
        }

        if (zoomController != null)
            zoomController.enabled = zoomControllerWasEnabled;

        if (overlay != null)
            overlay.SetActive(false);

        Time.timeScale = 1f;
    }

    private IEnumerator PlayIntroSequence()
    {
        sequenceCamera = Camera.main;
        zoomController = sequenceCamera != null
            ? sequenceCamera.GetComponent<OrthographicZoom>()
            : null;

        zoomControllerWasEnabled = zoomController != null && zoomController.enabled;
        if (zoomController != null)
            zoomController.enabled = false;

        if (sequenceCamera != null && sequenceCamera.orthographic)
        {
            float startingSize = sequenceCamera.orthographicSize;
            float maximumSize = zoomController != null
                ? zoomController.maxZoom
                : startingSize + zoomOutAmount;
            float targetSize = Mathf.Min(startingSize + zoomOutAmount, maximumSize);

            yield return Animate(zoomDuration, progress =>
            {
                if (sequenceCamera != null)
                    sequenceCamera.orthographicSize = Mathf.Lerp(startingSize, targetSize, Ease(progress));
            });
        }

        yield return Animate(panelDuration, progress =>
        {
            float easedProgress = Ease(progress);
            overlayGroup.alpha = easedProgress;
            dialog.localScale = Vector3.one * Mathf.Lerp(0.88f, 1f, easedProgress);
        });

        yield return Animate(imageFadeDuration, progress =>
        {
            imageGroup.alpha = Ease(progress);
        });

        overlayGroup.alpha = 1f;
        imageGroup.alpha = 1f;
        dialog.localScale = Vector3.one;
        overlayGroup.interactable = true;
        continueButton.interactable = true;
        mainMenuButton.interactable = true;
        introSequence = null;
    }

    private static IEnumerator Animate(float duration, System.Action<float> update)
    {
        if (duration <= 0f)
        {
            update(1f);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            update(Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        update(1f);
    }

    private static float Ease(float progress)
    {
        return progress * progress * (3f - 2f * progress);
    }

    private void BuildUI()
    {
        if (overlay != null)
            return;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject canvasObject = new GameObject(
            "LevelCompleteCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasObject.layer = 5;
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        overlay = CreateImageObject("CompletionOverlay", canvasObject.transform, new Color(0f, 0f, 0f, 0.68f));
        Stretch(overlay.GetComponent<RectTransform>());
        overlayGroup = overlay.AddComponent<CanvasGroup>();

        GameObject dialogObject = CreateImageObject(
            "CompletionDialog",
            overlay.transform,
            new Color(0.12f, 0.09f, 0.065f, 0.98f));
        dialog = dialogObject.GetComponent<RectTransform>();
        Center(dialog, Vector2.zero, new Vector2(1040f, 820f));

        Outline outline = dialogObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.82f, 0.59f, 0.28f, 1f);
        outline.effectDistance = new Vector2(5f, -5f);

        titleText = CreateText(
            "ClearedTitle",
            dialog,
            font,
            "LEVEL CLEARED",
            58,
            new Color(1f, 0.86f, 0.58f, 1f));
        Center(titleText.rectTransform, new Vector2(0f, 337f), new Vector2(920f, 90f));

        Text subtitle = CreateText(
            "ClearedSubtitle",
            dialog,
            font,
            "ROOM RESTORED",
            28,
            new Color(0.83f, 0.76f, 0.64f, 1f));
        Center(subtitle.rectTransform, new Vector2(0f, 278f), new Vector2(800f, 50f));

        GameObject imageFrame = CreateImageObject(
            "CompletedLevelImageFrame",
            dialog,
            new Color(0.04f, 0.035f, 0.03f, 1f));
        RectTransform imageFrameRect = imageFrame.GetComponent<RectTransform>();
        Center(imageFrameRect, new Vector2(0f, 25f), new Vector2(900f, 480f));
        imageGroup = imageFrame.AddComponent<CanvasGroup>();

        GameObject imageObject = CreateImageObject(
            "CompletedLevelImage",
            imageFrame.transform,
            Color.white);
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        Stretch(imageRect);
        imageRect.offsetMin = new Vector2(12f, 12f);
        imageRect.offsetMax = new Vector2(-12f, -12f);
        completedLevelImage = imageObject.GetComponent<Image>();
        completedLevelImage.preserveAspect = true;
        completedLevelImage.raycastTarget = false;

        continueButton = CreateButton(
            "ContinueButton",
            dialog,
            font,
            "CONTINUE",
            new Vector2(-195f, -326f),
            new Color(0.80f, 0.58f, 0.27f, 1f));
        continueButton.onClick.AddListener(ContinueToNextLevel);

        mainMenuButton = CreateButton(
            "MainMenuButton",
            dialog,
            font,
            "MAIN MENU",
            new Vector2(195f, -326f),
            new Color(0.48f, 0.20f, 0.16f, 1f));
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        overlay.SetActive(false);
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        GameObject eventSystemObject = new GameObject(
            "LevelCompleteEventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule));
        eventSystemObject.transform.SetParent(transform, false);
    }

    private static GameObject CreateImageObject(string name, Transform parent, Color color)
    {
        GameObject gameObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        gameObject.layer = 5;
        gameObject.transform.SetParent(parent, false);

        Image image = gameObject.GetComponent<Image>();
        image.color = color;
        return gameObject;
    }

    private static Text CreateText(
        string name,
        Transform parent,
        Font font,
        string text,
        int fontSize,
        Color color)
    {
        GameObject textObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Text));
        textObject.layer = 5;
        textObject.transform.SetParent(parent, false);

        Text label = textObject.GetComponent<Text>();
        label.font = font;
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }

    private static Button CreateButton(
        string name,
        Transform parent,
        Font font,
        string label,
        Vector2 position,
        Color color)
    {
        GameObject buttonObject = CreateImageObject(name, parent, color);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        Center(buttonRect, position, new Vector2(330f, 82f));

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        Text buttonLabel = CreateText(
            "Label",
            buttonRect,
            font,
            label,
            34,
            new Color(1f, 0.94f, 0.82f, 1f));
        Stretch(buttonLabel.rectTransform);
        return button;
    }

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static void Center(RectTransform rectTransform, Vector2 position, Vector2 size)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
    }

    private void ContinueToNextLevel()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ContinueToNextLevel();
    }

    private void ReturnToMainMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }
}
