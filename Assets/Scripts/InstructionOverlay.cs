using UnityEngine;
using UnityEngine.UI;

public class InstructionOverlay : MonoBehaviour
{
    private static InstructionOverlay instance;

    private GameObject panel;
    private Text instructionText;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        Hide();
    }

    private void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        panel = new GameObject("Panel");
        panel.transform.SetParent(transform, false);
        var image = panel.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.6f);

        var rect = image.rectTransform;
        rect.anchorMin = new Vector2(0.02f, 0.72f);
        rect.anchorMax = new Vector2(0.42f, 0.97f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var textObject = new GameObject("Instructions");
        textObject.transform.SetParent(panel.transform, false);
        instructionText = textObject.AddComponent<Text>();
        instructionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        instructionText.color = Color.white;
        instructionText.alignment = TextAnchor.UpperLeft;
        instructionText.fontSize = 26;
        instructionText.text = "WASD = move\nArrow Keys = shoot projectile\nWalk into chest to open it";

        var textRect = instructionText.rectTransform;
        textRect.anchorMin = new Vector2(0.05f, 0.05f);
        textRect.anchorMax = new Vector2(0.95f, 0.95f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    public static void Show()
    {
        EnsureInstance();
        instance.panel.SetActive(true);
    }

    public static void Hide()
    {
        if (instance == null) return;
        instance.panel.SetActive(false);
    }

    private static void EnsureInstance()
    {
        if (instance != null) return;
        var go = new GameObject("InstructionOverlay");
        instance = go.AddComponent<InstructionOverlay>();
    }
}
