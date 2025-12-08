using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class RouletteCreatorEditor : EditorWindow
{
    private int textureSize = 512;
    private float rouletteSize = 300f;
    private float arrowSize = 60f;
    
    private Color greenZoneColor = new Color(0.2f, 0.8f, 0.2f);
    private Color yellowZoneColor = new Color(1f, 0.9f, 0.2f);
    private Color redZoneColor = new Color(0.9f, 0.2f, 0.2f);
    
    private float greenAngle = 72f;
    private float yellowAngle = 180f;
    private float redAngle = 108f;
    
    private float greenMultiplier = 1.5f;
    private float yellowMultiplier = 1f;
    private float redMultiplier = 0.5f;
    
    [MenuItem("Tools/Roulette Creator")]
    public static void ShowWindow()
    {
        GetWindow<RouletteCreatorEditor>("Roulette Creator");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Roulette Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        GUILayout.Label("Size Settings", EditorStyles.boldLabel);
        rouletteSize = EditorGUILayout.FloatField("Roulette Size", rouletteSize);
        arrowSize = EditorGUILayout.FloatField("Arrow Size", arrowSize);
        textureSize = EditorGUILayout.IntField("Texture Resolution", textureSize);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("Zone Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Green Zone (Critical)");
        greenZoneColor = EditorGUILayout.ColorField("Color", greenZoneColor);
        greenAngle = EditorGUILayout.Slider("Angle", greenAngle, 10f, 180f);
        greenMultiplier = EditorGUILayout.FloatField("Multiplier", greenMultiplier);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Yellow Zone (Normal)");
        yellowZoneColor = EditorGUILayout.ColorField("Color", yellowZoneColor);
        yellowAngle = EditorGUILayout.Slider("Angle", yellowAngle, 10f, 180f);
        yellowMultiplier = EditorGUILayout.FloatField("Multiplier", yellowMultiplier);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Red Zone (Weak)");
        redZoneColor = EditorGUILayout.ColorField("Color", redZoneColor);
        redAngle = EditorGUILayout.Slider("Angle", redAngle, 10f, 180f);
        redMultiplier = EditorGUILayout.FloatField("Multiplier", redMultiplier);
        EditorGUILayout.EndVertical();
        
        float totalAngle = greenAngle + yellowAngle + redAngle;
        if (Mathf.Abs(totalAngle - 360f) > 0.1f)
        {
            EditorGUILayout.HelpBox($"Total: {totalAngle}° (should be 360°)", MessageType.Warning);
            if (GUILayout.Button("Normalize to 360°"))
            {
                float scale = 360f / totalAngle;
                greenAngle *= scale;
                yellowAngle *= scale;
                redAngle *= scale;
            }
        }
        else
        {
            EditorGUILayout.HelpBox($"Total: {totalAngle}° ✓", MessageType.Info);
        }
        
        EditorGUILayout.Space(20);
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Create Roulette in Selected Canvas", GUILayout.Height(40)))
        {
            CreateRoulette();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("Select a Canvas in Hierarchy before clicking the button", MessageType.Info);
    }
    
    void CreateRoulette()
    {
        Canvas canvas = FindCanvasInSelection();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a Canvas or any object inside a Canvas in the Hierarchy", "OK");
            return;
        }
        
        GameObject roulettePanel = CreateRoulettePanel(canvas.transform);
        GameObject wheel = CreateWheel(roulettePanel.transform);
        GameObject arrow = CreateArrow(roulettePanel.transform);
        GameObject resultText = CreateResultText(roulettePanel.transform);
        
        RouletteController controller = roulettePanel.AddComponent<RouletteController>();
        SetupRouletteController(controller, roulettePanel, wheel, arrow, resultText);
        
        Selection.activeGameObject = roulettePanel;
        
        EditorUtility.DisplayDialog("Success", "Roulette created successfully!\n\nThe RouletteController component has been added and configured.", "OK");
    }
    
    Canvas FindCanvasInSelection()
    {
        if (Selection.activeGameObject == null) return null;
        
        Canvas canvas = Selection.activeGameObject.GetComponent<Canvas>();
        if (canvas != null) return canvas;
        
        return Selection.activeGameObject.GetComponentInParent<Canvas>();
    }
    
    GameObject CreateRoulettePanel(Transform parent)
    {
        GameObject panel = new GameObject("RoulettePanel");
        panel.transform.SetParent(parent, false);
        
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(rouletteSize + 100, rouletteSize + 150);
        rectTransform.anchoredPosition = Vector2.zero;
        
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);
        
        panel.SetActive(false);
        
        return panel;
    }
    
    GameObject CreateWheel(Transform parent)
    {
        GameObject wheel = new GameObject("RouletteWheel");
        wheel.transform.SetParent(parent, false);
        
        RectTransform rectTransform = wheel.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(rouletteSize, rouletteSize);
        rectTransform.anchoredPosition = new Vector2(0, 20);
        
        Image image = wheel.AddComponent<Image>();
        
        Texture2D wheelTexture = GenerateWheelTexture();
        string path = "Assets/RouletteWheel.png";
        SaveTextureAsPNG(wheelTexture, path);
        AssetDatabase.Refresh();
        
        Sprite wheelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (wheelSprite != null)
        {
            image.sprite = wheelSprite;
        }
        else
        {
            image.color = Color.gray;
        }
        
        return wheel;
    }
    
    GameObject CreateArrow(Transform parent)
    {
        GameObject arrowContainer = new GameObject("Arrow");
        arrowContainer.transform.SetParent(parent, false);
        
        RectTransform containerRect = arrowContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(arrowSize, rouletteSize / 2 + arrowSize);
        containerRect.anchoredPosition = new Vector2(0, 20);
        containerRect.pivot = new Vector2(0.5f, 0f);
        
        GameObject arrowVisual = new GameObject("ArrowVisual");
        arrowVisual.transform.SetParent(arrowContainer.transform, false);
        
        RectTransform arrowRect = arrowVisual.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(0.5f, 1f);
        arrowRect.anchorMax = new Vector2(0.5f, 1f);
        arrowRect.sizeDelta = new Vector2(arrowSize, arrowSize);
        arrowRect.anchoredPosition = Vector2.zero;
        arrowRect.pivot = new Vector2(0.5f, 0f);
        
        Image arrowImage = arrowVisual.AddComponent<Image>();
        
        Texture2D arrowTexture = GenerateArrowTexture();
        string path = "Assets/RouletteArrow.png";
        SaveTextureAsPNG(arrowTexture, path);
        AssetDatabase.Refresh();
        
        Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (arrowSprite != null)
        {
            arrowImage.sprite = arrowSprite;
        }
        else
        {
            arrowImage.color = Color.white;
        }
        
        GameObject centerDot = new GameObject("CenterDot");
        centerDot.transform.SetParent(arrowContainer.transform, false);
        
        RectTransform dotRect = centerDot.AddComponent<RectTransform>();
        dotRect.anchorMin = new Vector2(0.5f, 0f);
        dotRect.anchorMax = new Vector2(0.5f, 0f);
        dotRect.sizeDelta = new Vector2(30, 30);
        dotRect.anchoredPosition = Vector2.zero;
        
        Image dotImage = centerDot.AddComponent<Image>();
        dotImage.color = Color.white;
        
        Texture2D circleTexture = GenerateCircleTexture(64);
        string circlePath = "Assets/RouletteCenter.png";
        SaveTextureAsPNG(circleTexture, circlePath);
        AssetDatabase.Refresh();
        
        Sprite circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(circlePath);
        if (circleSprite != null)
        {
            dotImage.sprite = circleSprite;
        }
        
        return arrowContainer;
    }
    
    GameObject CreateResultText(Transform parent)
    {
        GameObject textObj = new GameObject("ResultText");
        textObj.transform.SetParent(parent, false);
        
        RectTransform rectTransform = textObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.sizeDelta = new Vector2(400, 80);
        rectTransform.anchoredPosition = new Vector2(0, 50);
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "";
        tmp.fontSize = 48;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return textObj;
    }
    
    void SetupRouletteController(RouletteController controller, GameObject panel, GameObject wheel, GameObject arrow, GameObject resultText)
    {
        SerializedObject so = new SerializedObject(controller);
        
        so.FindProperty("roulettePanel").objectReferenceValue = panel;
        so.FindProperty("rouletteWheel").objectReferenceValue = wheel.GetComponent<Image>();
        so.FindProperty("arrowTransform").objectReferenceValue = arrow.GetComponent<RectTransform>();
        so.FindProperty("resultText").objectReferenceValue = resultText.GetComponent<TextMeshProUGUI>();
        
        SerializedProperty zonesProp = so.FindProperty("zones");
        zonesProp.ClearArray();
        
        zonesProp.InsertArrayElementAtIndex(0);
        SerializedProperty greenZone = zonesProp.GetArrayElementAtIndex(0);
        greenZone.FindPropertyRelative("zoneName").stringValue = "Critical";
        greenZone.FindPropertyRelative("zoneColor").colorValue = greenZoneColor;
        greenZone.FindPropertyRelative("angleSize").floatValue = greenAngle;
        greenZone.FindPropertyRelative("damageMultiplier").floatValue = greenMultiplier;
        
        zonesProp.InsertArrayElementAtIndex(1);
        SerializedProperty yellowZone = zonesProp.GetArrayElementAtIndex(1);
        yellowZone.FindPropertyRelative("zoneName").stringValue = "Normal";
        yellowZone.FindPropertyRelative("zoneColor").colorValue = yellowZoneColor;
        yellowZone.FindPropertyRelative("angleSize").floatValue = yellowAngle;
        yellowZone.FindPropertyRelative("damageMultiplier").floatValue = yellowMultiplier;
        
        zonesProp.InsertArrayElementAtIndex(2);
        SerializedProperty redZone = zonesProp.GetArrayElementAtIndex(2);
        redZone.FindPropertyRelative("zoneName").stringValue = "Weak";
        redZone.FindPropertyRelative("zoneColor").colorValue = redZoneColor;
        redZone.FindPropertyRelative("angleSize").floatValue = redAngle;
        redZone.FindPropertyRelative("damageMultiplier").floatValue = redMultiplier;
        
        so.ApplyModifiedProperties();
    }
    
    Texture2D GenerateWheelTexture()
    {
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[textureSize * textureSize];
        
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        float radius = textureSize / 2f - 5;
        
        float[] angles = { greenAngle, yellowAngle, redAngle };
        Color[] colors = { greenZoneColor, yellowZoneColor, redZoneColor };
        
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float dist = Vector2.Distance(pos, center);
                
                if (dist <= radius)
                {
                    Vector2 dir = (pos - center).normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    if (angle < 0) angle += 360f;
                    
                    float checkAngle = 90f;
                    Color pixelColor = colors[0];
                    
                    for (int i = 0; i < angles.Length; i++)
                    {
                        float startAngle = checkAngle;
                        float endAngle = checkAngle - angles[i];
                        
                        bool inZone = false;
                        if (endAngle < 0)
                        {
                            inZone = angle <= startAngle || angle >= (360f + endAngle);
                        }
                        else
                        {
                            inZone = angle <= startAngle && angle > endAngle;
                        }
                        
                        if (inZone)
                        {
                            pixelColor = colors[i];
                            break;
                        }
                        
                        checkAngle -= angles[i];
                        if (checkAngle < 0) checkAngle += 360f;
                    }
                    
                    if (dist > radius - 3)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.black, 0.5f);
                    }
                    
                    pixels[y * textureSize + x] = pixelColor;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    Texture2D GenerateArrowTexture()
    {
        int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        Vector2 tip = new Vector2(size / 2f, size - 5);
        Vector2 left = new Vector2(10, 5);
        Vector2 right = new Vector2(size - 10, 5);
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x, y);
                if (PointInTriangle(p, tip, left, right))
                {
                    pixels[y * size + x] = Color.white;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    Texture2D GenerateCircleTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    float alpha = Mathf.Clamp01((radius - dist) / 2f);
                    pixels[y * size + x] = new Color(1, 1, 1, alpha);
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);
        
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        
        return !(hasNeg && hasPos);
    }
    
    float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
    
    void SaveTextureAsPNG(Texture2D texture, string path)
    {
        byte[] pngData = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, pngData);
        
        AssetDatabase.Refresh();
        
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
        
        DestroyImmediate(texture);
    }
}