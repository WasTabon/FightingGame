using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

public class FightResultCreatorEditor : EditorWindow
{
    private float panelWidth = 500f;
    private float panelHeight = 400f;
    private float iconSize = 120f;
    private float buttonWidth = 200f;
    private float buttonHeight = 60f;

    private Color panelBgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    private Color victoryColor = new Color(0.2f, 0.8f, 0.2f);
    private Color defeatColor = new Color(0.8f, 0.2f, 0.2f);
    private Color buttonColor = new Color(0.2f, 0.6f, 0.9f);

    private string prefabsPath = "Assets/FightingGame/Prefabs/UI";

    [MenuItem("Tools/Fight Result Creator")]
    public static void ShowWindow()
    {
        GetWindow<FightResultCreatorEditor>("Fight Result Creator");
    }

    void OnGUI()
    {
        GUILayout.Label("Fight Result Panel Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        GUILayout.Label("Size Settings", EditorStyles.boldLabel);
        panelWidth = EditorGUILayout.FloatField("Panel Width", panelWidth);
        panelHeight = EditorGUILayout.FloatField("Panel Height", panelHeight);
        iconSize = EditorGUILayout.FloatField("Icon Size", iconSize);
        buttonWidth = EditorGUILayout.FloatField("Button Width", buttonWidth);
        buttonHeight = EditorGUILayout.FloatField("Button Height", buttonHeight);

        EditorGUILayout.Space(5);
        GUILayout.Label("Colors", EditorStyles.boldLabel);
        panelBgColor = EditorGUILayout.ColorField("Panel Background", panelBgColor);
        victoryColor = EditorGUILayout.ColorField("Victory Color", victoryColor);
        defeatColor = EditorGUILayout.ColorField("Defeat Color", defeatColor);
        buttonColor = EditorGUILayout.ColorField("Button Color", buttonColor);

        EditorGUILayout.Space(5);
        prefabsPath = EditorGUILayout.TextField("Prefabs Path", prefabsPath);

        EditorGUILayout.Space(20);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Create Fight Result Panel", GUILayout.Height(40)))
        {
            CreateFightResultPanel();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("Select a Canvas in Hierarchy before clicking the button", MessageType.Info);
    }

    void CreateFightResultPanel()
    {
        Canvas canvas = FindCanvasInSelection();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a Canvas in the Hierarchy", "OK");
            return;
        }

        EnsureDirectoryExists(prefabsPath);
        EnsureDirectoryExists(prefabsPath + "/Textures");

        CreateTextures();
        AssetDatabase.Refresh();

        GameObject resultPanel = CreateResultPanelUI(canvas.transform);

        Selection.activeGameObject = resultPanel;

        EditorUtility.DisplayDialog("Success",
            "Fight Result Panel created!\n\n" +
            "Don't forget to:\n" +
            "1. Assign FightController reference\n" +
            "2. Assign GameController reference\n" +
            "3. Assign UIController reference (optional)",
            "OK");
    }

    Canvas FindCanvasInSelection()
    {
        if (Selection.activeGameObject == null) return null;

        Canvas canvas = Selection.activeGameObject.GetComponent<Canvas>();
        if (canvas != null) return canvas;

        return Selection.activeGameObject.GetComponentInParent<Canvas>();
    }

    void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    void CreateTextures()
    {
        CreateTrophyTexture("VictoryIcon", victoryColor, 128);
        CreateSkullTexture("DefeatIcon", defeatColor, 128);
    }

    void CreateTrophyTexture(string name, Color color, int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        Vector2 center = new Vector2(size / 2f, size / 2f);

        int cupTop = (int)(size * 0.75f);
        int cupBottom = (int)(size * 0.35f);
        int cupWidth = (int)(size * 0.5f);

        for (int y = cupBottom; y < cupTop; y++)
        {
            float t = (float)(y - cupBottom) / (cupTop - cupBottom);
            int width = (int)Mathf.Lerp(cupWidth * 0.6f, cupWidth, t);
            int startX = (size - width) / 2;

            for (int x = startX; x < startX + width; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        int baseTop = cupBottom;
        int baseBottom = (int)(size * 0.15f);
        int baseWidth = (int)(size * 0.4f);

        for (int y = baseBottom; y < baseTop; y++)
        {
            float t = (float)(y - baseBottom) / (baseTop - baseBottom);
            int width = (int)Mathf.Lerp(baseWidth, baseWidth * 0.5f, t);
            int startX = (size - width) / 2;

            for (int x = startX; x < startX + width; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        int handleY = (cupTop + cupBottom) / 2;
        int handleRadius = (int)(size * 0.12f);

        for (int angle = -90; angle <= 90; angle += 5)
        {
            float rad = angle * Mathf.Deg2Rad;
            int hx = (int)(center.x + cupWidth / 2 + handleRadius * Mathf.Cos(rad));
            int hy = (int)(handleY + handleRadius * Mathf.Sin(rad));

            for (int dx = -3; dx <= 3; dx++)
            {
                for (int dy = -3; dy <= 3; dy++)
                {
                    int px = hx + dx;
                    int py = hy + dy;
                    if (px >= 0 && px < size && py >= 0 && py < size)
                    {
                        pixels[py * size + px] = color;
                    }
                }
            }
        }

        for (int angle = -90; angle <= 90; angle += 5)
        {
            float rad = angle * Mathf.Deg2Rad;
            int hx = (int)(center.x - cupWidth / 2 - handleRadius * Mathf.Cos(rad));
            int hy = (int)(handleY + handleRadius * Mathf.Sin(rad));

            for (int dx = -3; dx <= 3; dx++)
            {
                for (int dy = -3; dy <= 3; dy++)
                {
                    int px = hx + dx;
                    int py = hy + dy;
                    if (px >= 0 && px < size && py >= 0 && py < size)
                    {
                        pixels[py * size + px] = color;
                    }
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        SaveTextureAsPNG(texture, $"{prefabsPath}/Textures/{name}.png");
        DestroyImmediate(texture);
    }

    void CreateSkullTexture(string name, Color color, int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        Vector2 center = new Vector2(size / 2f, size * 0.55f);
        float radiusX = size * 0.35f;
        float radiusY = size * 0.32f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center.x) / radiusX;
                float dy = (y - center.y) / radiusY;

                if (dx * dx + dy * dy <= 1)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        int jawTop = (int)(size * 0.35f);
        int jawBottom = (int)(size * 0.15f);
        int jawWidth = (int)(size * 0.5f);

        for (int y = jawBottom; y < jawTop; y++)
        {
            float t = (float)(y - jawBottom) / (jawTop - jawBottom);
            int width = (int)Mathf.Lerp(jawWidth * 0.7f, jawWidth, t);
            int startX = (size - width) / 2;

            for (int x = startX; x < startX + width; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        Vector2 leftEye = new Vector2(size * 0.35f, size * 0.6f);
        Vector2 rightEye = new Vector2(size * 0.65f, size * 0.6f);
        float eyeRadius = size * 0.1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distLeft = Vector2.Distance(new Vector2(x, y), leftEye);
                float distRight = Vector2.Distance(new Vector2(x, y), rightEye);

                if (distLeft <= eyeRadius || distRight <= eyeRadius)
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        Vector2 nose = new Vector2(size * 0.5f, size * 0.45f);
        float noseSize = size * 0.06f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - nose.x);
                float dy = nose.y - y;

                if (dy > 0 && dy < noseSize * 2 && dx < noseSize * (1 - dy / (noseSize * 2)))
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        SaveTextureAsPNG(texture, $"{prefabsPath}/Textures/{name}.png");
        DestroyImmediate(texture);
    }

    void SaveTextureAsPNG(Texture2D texture, string path)
    {
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(path, pngData);

        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }

    GameObject CreateResultPanelUI(Transform parent)
    {
        GameObject resultPanel = new GameObject("FightResultPanel");
        resultPanel.transform.SetParent(parent, false);

        RectTransform panelRect = resultPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelBg = resultPanel.AddComponent<Image>();
        panelBg.color = panelBgColor;

        CanvasGroup canvasGroup = resultPanel.AddComponent<CanvasGroup>();

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(resultPanel.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.7f);
        iconRect.anchorMax = new Vector2(0.5f, 0.7f);
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);
        iconRect.anchoredPosition = Vector2.zero;

        Image iconImage = iconObj.AddComponent<Image>();
        Sprite victorySprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{prefabsPath}/Textures/VictoryIcon.png");
        if (victorySprite != null)
        {
            iconImage.sprite = victorySprite;
        }
        iconImage.color = victoryColor;

        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(resultPanel.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.45f);
        titleRect.anchorMax = new Vector2(1, 0.6f);
        titleRect.offsetMin = new Vector2(20, 0);
        titleRect.offsetMax = new Vector2(-20, 0);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "VICTORY!";
        titleText.fontSize = 48;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = victoryColor;

        GameObject rewardsContainer = new GameObject("RewardsContainer");
        rewardsContainer.transform.SetParent(resultPanel.transform, false);
        RectTransform rewardsContainerRect = rewardsContainer.AddComponent<RectTransform>();
        rewardsContainerRect.anchorMin = new Vector2(0, 0.25f);
        rewardsContainerRect.anchorMax = new Vector2(1, 0.45f);
        rewardsContainerRect.offsetMin = new Vector2(20, 0);
        rewardsContainerRect.offsetMax = new Vector2(-20, 0);

        GameObject rewardsObj = new GameObject("RewardsText");
        rewardsObj.transform.SetParent(rewardsContainer.transform, false);
        RectTransform rewardsRect = rewardsObj.AddComponent<RectTransform>();
        rewardsRect.anchorMin = Vector2.zero;
        rewardsRect.anchorMax = Vector2.one;
        rewardsRect.offsetMin = Vector2.zero;
        rewardsRect.offsetMax = Vector2.zero;

        TextMeshProUGUI rewardsText = rewardsObj.AddComponent<TextMeshProUGUI>();
        rewardsText.text = "+30 Rank\n+150 EXP";
        rewardsText.fontSize = 28;
        rewardsText.alignment = TextAlignmentOptions.Center;
        rewardsText.color = Color.white;

        GameObject buttonObj = new GameObject("ContinueButton");
        buttonObj.transform.SetParent(resultPanel.transform, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.08f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.08f);
        buttonRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
        buttonRect.anchoredPosition = Vector2.zero;

        Image buttonBg = buttonObj.AddComponent<Image>();
        buttonBg.color = buttonColor;

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonBg;

        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(buttonColor.r * 1.2f, buttonColor.g * 1.2f, buttonColor.b * 1.2f);
        colors.pressedColor = new Color(buttonColor.r * 0.8f, buttonColor.g * 0.8f, buttonColor.b * 0.8f);
        button.colors = colors;

        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "CONTINUE";
        buttonText.fontSize = 24;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;

        FightResultController controller = resultPanel.AddComponent<FightResultController>();

        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("resultPanel").objectReferenceValue = resultPanel;
        so.FindProperty("panelCanvasGroup").objectReferenceValue = canvasGroup;
        so.FindProperty("titleText").objectReferenceValue = titleText;
        so.FindProperty("rewardsText").objectReferenceValue = rewardsText;
        so.FindProperty("rewardsContainer").objectReferenceValue = rewardsContainer;
        so.FindProperty("iconImage").objectReferenceValue = iconImage;
        so.FindProperty("continueButton").objectReferenceValue = button;
        so.FindProperty("victoryColor").colorValue = victoryColor;
        so.FindProperty("defeatColor").colorValue = defeatColor;

        Sprite defeatSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{prefabsPath}/Textures/DefeatIcon.png");
        so.FindProperty("victoryIcon").objectReferenceValue = victorySprite;
        so.FindProperty("defeatIcon").objectReferenceValue = defeatSprite;

        so.ApplyModifiedProperties();

        resultPanel.SetActive(false);

        return resultPanel;
    }
}
