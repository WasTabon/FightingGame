using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class BattlePassCreatorEditor : EditorWindow
{
    private int totalLevels = 50;
    private int coinsCount = 30;
    private int gemsCount = 17;
    private int skinsCount = 3;
    
    private float levelWidth = 120f;
    private float levelSpacing = 10f;
    private float rewardSize = 80f;
    
    private Color coinsColor = new Color(1f, 0.84f, 0f);
    private Color gemsColor = new Color(0.2f, 0.6f, 1f);
    private Color skinsColor = new Color(0.7f, 0.3f, 0.9f);
    private Color freeRowColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    private Color premiumRowColor = new Color(0.6f, 0.5f, 0.2f, 0.8f);
    private Color progressBarBgColor = new Color(0.2f, 0.2f, 0.2f);
    private Color progressBarFillColor = new Color(0.2f, 0.8f, 0.3f);
    
    private string prefabsPath = "Assets/FightingGame/Prefabs/BattlePass";
    
    [MenuItem("Tools/Battle Pass Creator")]
    public static void ShowWindow()
    {
        GetWindow<BattlePassCreatorEditor>("Battle Pass Creator");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Battle Pass Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        GUILayout.Label("Level Settings", EditorStyles.boldLabel);
        totalLevels = EditorGUILayout.IntField("Total Levels", totalLevels);
        
        EditorGUILayout.Space(5);
        GUILayout.Label("Reward Distribution (per row)", EditorStyles.boldLabel);
        coinsCount = EditorGUILayout.IntField("Coins Levels", coinsCount);
        gemsCount = EditorGUILayout.IntField("Gems Levels", gemsCount);
        skinsCount = EditorGUILayout.IntField("Skins Levels", skinsCount);
        
        int total = coinsCount + gemsCount + skinsCount;
        if (total != totalLevels)
        {
            EditorGUILayout.HelpBox($"Total rewards ({total}) must equal total levels ({totalLevels})", MessageType.Warning);
        }
        
        EditorGUILayout.Space(5);
        GUILayout.Label("Size Settings", EditorStyles.boldLabel);
        levelWidth = EditorGUILayout.FloatField("Level Width", levelWidth);
        levelSpacing = EditorGUILayout.FloatField("Level Spacing", levelSpacing);
        rewardSize = EditorGUILayout.FloatField("Reward Icon Size", rewardSize);
        
        EditorGUILayout.Space(5);
        GUILayout.Label("Colors", EditorStyles.boldLabel);
        coinsColor = EditorGUILayout.ColorField("Coins", coinsColor);
        gemsColor = EditorGUILayout.ColorField("Gems", gemsColor);
        skinsColor = EditorGUILayout.ColorField("Skins", skinsColor);
        freeRowColor = EditorGUILayout.ColorField("Free Row BG", freeRowColor);
        premiumRowColor = EditorGUILayout.ColorField("Premium Row BG", premiumRowColor);
        
        EditorGUILayout.Space(5);
        prefabsPath = EditorGUILayout.TextField("Prefabs Path", prefabsPath);
        
        EditorGUILayout.Space(20);
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Create Battle Pass", GUILayout.Height(40)))
        {
            CreateBattlePass();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("Select a Canvas in Hierarchy before clicking the button", MessageType.Info);
    }
    
    void CreateBattlePass()
    {
        Canvas canvas = FindCanvasInSelection();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a Canvas in the Hierarchy", "OK");
            return;
        }
        
        EnsureDirectoryExists(prefabsPath);
        EnsureDirectoryExists(prefabsPath + "/Textures");
        
        CreateRewardTextures();
        AssetDatabase.Refresh();
        
        GameObject coinsPrefab = CreateRewardPrefab("CoinsReward", coinsColor, "Coins");
        GameObject gemsPrefab = CreateRewardPrefab("GemsReward", gemsColor, "Gems");
        GameObject skinsPrefab = CreateRewardPrefab("SkinsReward", skinsColor, "Skin");
        
        GameObject levelPrefab = CreateLevelPrefab();
        
        GameObject battlePassUI = CreateBattlePassUI(canvas.transform, levelPrefab, coinsPrefab, gemsPrefab, skinsPrefab);
        
        Selection.activeGameObject = battlePassUI;
        
        EditorUtility.DisplayDialog("Success", 
            "Battle Pass created!\n\n" +
            "Prefabs saved to:\n" + prefabsPath + "\n\n" +
            "To customize rewards, edit the prefabs.", "OK");
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
    
    void CreateRewardTextures()
    {
        CreateCircleTexture("CoinsIcon", coinsColor, 128);
        CreateDiamondTexture("GemsIcon", gemsColor, 128);
        CreateSquareTexture("SkinsIcon", skinsColor, 128);
        CreateCircleTexture("LockIcon", Color.gray, 64);
        CreateCheckTexture("CheckIcon", Color.white, 64);
    }
    
    void CreateCircleTexture(string name, Color color, int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 4;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    float edge = Mathf.Clamp01((radius - dist) / 2f);
                    pixels[y * size + x] = new Color(color.r, color.g, color.b, edge);
                }
                else
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
    
    void CreateDiamondTexture(string name, Color color, int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float halfSize = size / 2f - 4;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - center.x);
                float dy = Mathf.Abs(y - center.y);
                float dist = dx + dy;
                
                if (dist <= halfSize)
                {
                    float edge = Mathf.Clamp01((halfSize - dist) / 2f);
                    pixels[y * size + x] = new Color(color.r, color.g, color.b, edge);
                }
                else
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
    
    void CreateSquareTexture(string name, Color color, int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        
        int border = 8;
        int cornerRadius = 16;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = x >= border && x < size - border && y >= border && y < size - border;
                
                if (inside)
                {
                    int localX = x - border;
                    int localY = y - border;
                    int innerSize = size - border * 2;
                    
                    bool inCorner = false;
                    float cornerDist = 0;
                    
                    if (localX < cornerRadius && localY < cornerRadius)
                    {
                        cornerDist = Vector2.Distance(new Vector2(localX, localY), new Vector2(cornerRadius, cornerRadius));
                        inCorner = true;
                    }
                    else if (localX >= innerSize - cornerRadius && localY < cornerRadius)
                    {
                        cornerDist = Vector2.Distance(new Vector2(localX, localY), new Vector2(innerSize - cornerRadius, cornerRadius));
                        inCorner = true;
                    }
                    else if (localX < cornerRadius && localY >= innerSize - cornerRadius)
                    {
                        cornerDist = Vector2.Distance(new Vector2(localX, localY), new Vector2(cornerRadius, innerSize - cornerRadius));
                        inCorner = true;
                    }
                    else if (localX >= innerSize - cornerRadius && localY >= innerSize - cornerRadius)
                    {
                        cornerDist = Vector2.Distance(new Vector2(localX, localY), new Vector2(innerSize - cornerRadius, innerSize - cornerRadius));
                        inCorner = true;
                    }
                    
                    if (inCorner && cornerDist > cornerRadius)
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                    else
                    {
                        pixels[y * size + x] = color;
                    }
                }
                else
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
    
    void CreateCheckTexture(string name, Color color, int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        int thickness = 6;
        Vector2 p1 = new Vector2(size * 0.2f, size * 0.5f);
        Vector2 p2 = new Vector2(size * 0.4f, size * 0.25f);
        Vector2 p3 = new Vector2(size * 0.8f, size * 0.75f);
        
        DrawLine(pixels, size, p1, p2, color, thickness);
        DrawLine(pixels, size, p2, p3, color, thickness);
        
        texture.SetPixels(pixels);
        texture.Apply();
        SaveTextureAsPNG(texture, $"{prefabsPath}/Textures/{name}.png");
        DestroyImmediate(texture);
    }
    
    void DrawLine(Color[] pixels, int size, Vector2 from, Vector2 to, Color color, int thickness)
    {
        float dist = Vector2.Distance(from, to);
        for (float t = 0; t <= 1; t += 1f / dist)
        {
            Vector2 point = Vector2.Lerp(from, to, t);
            for (int dx = -thickness; dx <= thickness; dx++)
            {
                for (int dy = -thickness; dy <= thickness; dy++)
                {
                    int px = Mathf.RoundToInt(point.x + dx);
                    int py = Mathf.RoundToInt(point.y + dy);
                    if (px >= 0 && px < size && py >= 0 && py < size)
                    {
                        if (dx * dx + dy * dy <= thickness * thickness)
                        {
                            pixels[py * size + px] = color;
                        }
                    }
                }
            }
        }
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
    
    GameObject CreateRewardPrefab(string name, Color color, string labelText)
    {
        GameObject reward = new GameObject(name);
        
        RectTransform rect = reward.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rewardSize, rewardSize);
        
        Image bg = reward.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(reward.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(10, 20);
        iconRect.offsetMax = new Vector2(-10, -10);
        
        Image iconImage = iconObj.AddComponent<Image>();
        string textureName = name.Replace("Reward", "Icon");
        Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{prefabsPath}/Textures/{textureName}.png");
        if (iconSprite != null)
        {
            iconImage.sprite = iconSprite;
        }
        else
        {
            iconImage.color = color;
        }
        
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(reward.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 0);
        labelRect.pivot = new Vector2(0.5f, 0);
        labelRect.offsetMin = new Vector2(2, 2);
        labelRect.offsetMax = new Vector2(-2, 18);
        
        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = labelText;
        label.fontSize = 12;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        
        GameObject lockOverlay = new GameObject("LockOverlay");
        lockOverlay.transform.SetParent(reward.transform, false);
        RectTransform lockRect = lockOverlay.AddComponent<RectTransform>();
        lockRect.anchorMin = Vector2.zero;
        lockRect.anchorMax = Vector2.one;
        lockRect.offsetMin = Vector2.zero;
        lockRect.offsetMax = Vector2.zero;
        
        Image lockBg = lockOverlay.AddComponent<Image>();
        lockBg.color = new Color(0, 0, 0, 0.7f);
        
        GameObject lockIcon = new GameObject("LockIcon");
        lockIcon.transform.SetParent(lockOverlay.transform, false);
        RectTransform lockIconRect = lockIcon.AddComponent<RectTransform>();
        lockIconRect.anchorMin = new Vector2(0.5f, 0.5f);
        lockIconRect.anchorMax = new Vector2(0.5f, 0.5f);
        lockIconRect.sizeDelta = new Vector2(30, 30);
        
        Image lockIconImage = lockIcon.AddComponent<Image>();
        Sprite lockSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{prefabsPath}/Textures/LockIcon.png");
        if (lockSprite != null)
        {
            lockIconImage.sprite = lockSprite;
        }
        
        lockOverlay.SetActive(false);
        
        GameObject checkMark = new GameObject("CheckMark");
        checkMark.transform.SetParent(reward.transform, false);
        RectTransform checkRect = checkMark.AddComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(1, 1);
        checkRect.anchorMax = new Vector2(1, 1);
        checkRect.pivot = new Vector2(1, 1);
        checkRect.sizeDelta = new Vector2(24, 24);
        checkRect.anchoredPosition = new Vector2(-2, -2);
        
        Image checkBg = checkMark.AddComponent<Image>();
        checkBg.color = new Color(0.2f, 0.8f, 0.2f);
        
        GameObject checkIcon = new GameObject("CheckIcon");
        checkIcon.transform.SetParent(checkMark.transform, false);
        RectTransform checkIconRect = checkIcon.AddComponent<RectTransform>();
        checkIconRect.anchorMin = Vector2.zero;
        checkIconRect.anchorMax = Vector2.one;
        checkIconRect.offsetMin = new Vector2(4, 4);
        checkIconRect.offsetMax = new Vector2(-4, -4);
        
        Image checkIconImage = checkIcon.AddComponent<Image>();
        Sprite checkSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{prefabsPath}/Textures/CheckIcon.png");
        if (checkSprite != null)
        {
            checkIconImage.sprite = checkSprite;
        }
        
        checkMark.SetActive(false);
        
        string prefabPath = $"{prefabsPath}/{name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(reward, prefabPath);
        DestroyImmediate(reward);
        
        return prefab;
    }
    
    GameObject CreateLevelPrefab()
    {
        GameObject level = new GameObject("BattlePassLevel");
        
        RectTransform rect = level.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(levelWidth, 220);
        
        VerticalLayoutGroup layout = level.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 5;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(5, 5, 5, 5);
        
        GameObject freeSlot = new GameObject("FreeRewardSlot");
        freeSlot.transform.SetParent(level.transform, false);
        RectTransform freeRect = freeSlot.AddComponent<RectTransform>();
        freeRect.sizeDelta = new Vector2(levelWidth - 10, rewardSize + 10);
        Image freeBg = freeSlot.AddComponent<Image>();
        freeBg.color = freeRowColor;
        
        GameObject levelNumObj = new GameObject("LevelNumber");
        levelNumObj.transform.SetParent(level.transform, false);
        RectTransform numRect = levelNumObj.AddComponent<RectTransform>();
        numRect.sizeDelta = new Vector2(levelWidth - 10, 30);
        
        Image numBg = levelNumObj.AddComponent<Image>();
        numBg.color = new Color(0.25f, 0.25f, 0.25f);
        
        GameObject numText = new GameObject("Text");
        numText.transform.SetParent(levelNumObj.transform, false);
        RectTransform numTextRect = numText.AddComponent<RectTransform>();
        numTextRect.anchorMin = Vector2.zero;
        numTextRect.anchorMax = Vector2.one;
        numTextRect.offsetMin = Vector2.zero;
        numTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI levelLabel = numText.AddComponent<TextMeshProUGUI>();
        levelLabel.text = "1";
        levelLabel.fontSize = 18;
        levelLabel.fontStyle = FontStyles.Bold;
        levelLabel.alignment = TextAlignmentOptions.Center;
        levelLabel.color = Color.white;
        
        GameObject premiumSlot = new GameObject("PremiumRewardSlot");
        premiumSlot.transform.SetParent(level.transform, false);
        RectTransform premRect = premiumSlot.AddComponent<RectTransform>();
        premRect.sizeDelta = new Vector2(levelWidth - 10, rewardSize + 10);
        Image premBg = premiumSlot.AddComponent<Image>();
        premBg.color = premiumRowColor;
        
        GameObject premiumIcon = new GameObject("PremiumIcon");
        premiumIcon.transform.SetParent(premiumSlot.transform, false);
        RectTransform premIconRect = premiumIcon.AddComponent<RectTransform>();
        premIconRect.anchorMin = new Vector2(0, 1);
        premIconRect.anchorMax = new Vector2(0, 1);
        premIconRect.pivot = new Vector2(0, 1);
        premIconRect.sizeDelta = new Vector2(20, 20);
        premIconRect.anchoredPosition = new Vector2(2, -2);
        
        Image premIconImage = premiumIcon.AddComponent<Image>();
        premIconImage.color = new Color(1f, 0.84f, 0f);
        
        string prefabPath = $"{prefabsPath}/BattlePassLevel.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(level, prefabPath);
        DestroyImmediate(level);
        
        return prefab;
    }
    
    GameObject CreateBattlePassUI(Transform parent, GameObject levelPrefab, GameObject coinsPrefab, GameObject gemsPrefab, GameObject skinsPrefab)
    {
        GameObject battlePass = new GameObject("BattlePassUI");
        battlePass.transform.SetParent(parent, false);
        
        RectTransform bpRect = battlePass.AddComponent<RectTransform>();
        bpRect.anchorMin = Vector2.zero;
        bpRect.anchorMax = Vector2.one;
        bpRect.offsetMin = Vector2.zero;
        bpRect.offsetMax = Vector2.zero;
        
        Image bpBg = battlePass.AddComponent<Image>();
        bpBg.color = new Color(0.1f, 0.1f, 0.12f);
        
        GameObject header = CreateHeader(battlePass.transform);
        GameObject progressBar = CreateProgressBar(battlePass.transform);
        GameObject scrollArea = CreateScrollArea(battlePass.transform, levelPrefab, coinsPrefab, gemsPrefab, skinsPrefab);
        GameObject footer = CreateFooter(battlePass.transform);
        
        return battlePass;
    }
    
    GameObject CreateHeader(Transform parent)
    {
        GameObject header = new GameObject("Header");
        header.transform.SetParent(parent, false);
        
        RectTransform rect = header.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(0, 60);
        rect.anchoredPosition = Vector2.zero;
        
        Image bg = header.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.18f);
        
        GameObject title = new GameObject("Title");
        title.transform.SetParent(header.transform, false);
        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.offsetMin = new Vector2(20, 0);
        titleRect.offsetMax = new Vector2(0, 0);
        
        TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "BATTLE PASS";
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.MidlineLeft;
        titleText.color = Color.white;
        
        GameObject seasonInfo = new GameObject("SeasonInfo");
        seasonInfo.transform.SetParent(header.transform, false);
        RectTransform seasonRect = seasonInfo.AddComponent<RectTransform>();
        seasonRect.anchorMin = new Vector2(0.5f, 0);
        seasonRect.anchorMax = new Vector2(0.75f, 1);
        seasonRect.offsetMin = Vector2.zero;
        seasonRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI seasonText = seasonInfo.AddComponent<TextMeshProUGUI>();
        seasonText.text = "Season 1\n<size=14>29 days remaining</size>";
        seasonText.fontSize = 18;
        seasonText.alignment = TextAlignmentOptions.Center;
        seasonText.color = new Color(0.8f, 0.8f, 0.8f);
        
        GameObject buyButton = new GameObject("BuyPremiumButton");
        buyButton.transform.SetParent(header.transform, false);
        RectTransform buyRect = buyButton.AddComponent<RectTransform>();
        buyRect.anchorMin = new Vector2(1, 0.5f);
        buyRect.anchorMax = new Vector2(1, 0.5f);
        buyRect.pivot = new Vector2(1, 0.5f);
        buyRect.sizeDelta = new Vector2(150, 40);
        buyRect.anchoredPosition = new Vector2(-20, 0);
        
        Image buyBg = buyButton.AddComponent<Image>();
        buyBg.color = new Color(0.9f, 0.7f, 0.1f);
        
        Button buyBtn = buyButton.AddComponent<Button>();
        buyBtn.targetGraphic = buyBg;
        
        GameObject buyText = new GameObject("Text");
        buyText.transform.SetParent(buyButton.transform, false);
        RectTransform buyTextRect = buyText.AddComponent<RectTransform>();
        buyTextRect.anchorMin = Vector2.zero;
        buyTextRect.anchorMax = Vector2.one;
        buyTextRect.offsetMin = Vector2.zero;
        buyTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI buyLabel = buyText.AddComponent<TextMeshProUGUI>();
        buyLabel.text = "BUY PREMIUM";
        buyLabel.fontSize = 16;
        buyLabel.fontStyle = FontStyles.Bold;
        buyLabel.alignment = TextAlignmentOptions.Center;
        buyLabel.color = Color.black;
        
        return header;
    }
    
    GameObject CreateProgressBar(Transform parent)
    {
        GameObject progressBar = new GameObject("ProgressBar");
        progressBar.transform.SetParent(parent, false);
        
        RectTransform rect = progressBar.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(0, 40);
        rect.anchoredPosition = new Vector2(0, -60);
        
        Image bg = progressBar.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.12f, 0.15f);
        
        GameObject levelIndicator = new GameObject("LevelIndicator");
        levelIndicator.transform.SetParent(progressBar.transform, false);
        RectTransform levelRect = levelIndicator.AddComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0, 0);
        levelRect.anchorMax = new Vector2(0, 1);
        levelRect.pivot = new Vector2(0, 0.5f);
        levelRect.sizeDelta = new Vector2(100, 0);
        levelRect.anchoredPosition = new Vector2(10, 0);
        
        TextMeshProUGUI levelText = levelIndicator.AddComponent<TextMeshProUGUI>();
        levelText.text = "LVL 1";
        levelText.fontSize = 20;
        levelText.fontStyle = FontStyles.Bold;
        levelText.alignment = TextAlignmentOptions.MidlineLeft;
        levelText.color = Color.white;
        
        GameObject barBg = new GameObject("BarBackground");
        barBg.transform.SetParent(progressBar.transform, false);
        RectTransform barBgRect = barBg.AddComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0, 0.5f);
        barBgRect.anchorMax = new Vector2(1, 0.5f);
        barBgRect.pivot = new Vector2(0.5f, 0.5f);
        barBgRect.sizeDelta = new Vector2(-240, 16);
        barBgRect.anchoredPosition = new Vector2(0, 0);
        
        Image barBgImage = barBg.AddComponent<Image>();
        barBgImage.color = progressBarBgColor;
        
        GameObject barFill = new GameObject("BarFill");
        barFill.transform.SetParent(barBg.transform, false);
        RectTransform barFillRect = barFill.AddComponent<RectTransform>();
        barFillRect.anchorMin = new Vector2(0, 0);
        barFillRect.anchorMax = new Vector2(0.35f, 1);
        barFillRect.offsetMin = Vector2.zero;
        barFillRect.offsetMax = Vector2.zero;
        
        Image barFillImage = barFill.AddComponent<Image>();
        barFillImage.color = progressBarFillColor;
        
        GameObject xpText = new GameObject("XPText");
        xpText.transform.SetParent(progressBar.transform, false);
        RectTransform xpRect = xpText.AddComponent<RectTransform>();
        xpRect.anchorMin = new Vector2(1, 0);
        xpRect.anchorMax = new Vector2(1, 1);
        xpRect.pivot = new Vector2(1, 0.5f);
        xpRect.sizeDelta = new Vector2(100, 0);
        xpRect.anchoredPosition = new Vector2(-10, 0);
        
        TextMeshProUGUI xpLabel = xpText.AddComponent<TextMeshProUGUI>();
        xpLabel.text = "350/1000 XP";
        xpLabel.fontSize = 14;
        xpLabel.alignment = TextAlignmentOptions.MidlineRight;
        xpLabel.color = new Color(0.7f, 0.7f, 0.7f);
        
        return progressBar;
    }
    
    GameObject CreateScrollArea(Transform parent, GameObject levelPrefab, GameObject coinsPrefab, GameObject gemsPrefab, GameObject skinsPrefab)
    {
        GameObject scrollArea = new GameObject("ScrollArea");
        scrollArea.transform.SetParent(parent, false);
        
        RectTransform scrollRect = scrollArea.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = new Vector2(0, 50);
        scrollRect.offsetMax = new Vector2(0, -100);
        
        Image scrollBg = scrollArea.AddComponent<Image>();
        scrollBg.color = new Color(0.08f, 0.08f, 0.1f);
        
        ScrollRect scroll = scrollArea.AddComponent<ScrollRect>();
        scroll.horizontal = true;
        scroll.vertical = false;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.elasticity = 0.1f;
        
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollArea.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        viewport.AddComponent<Image>().color = Color.white;
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(0, 1);
        contentRect.pivot = new Vector2(0, 0.5f);
        contentRect.sizeDelta = new Vector2(totalLevels * (levelWidth + levelSpacing), 0);
        contentRect.anchoredPosition = Vector2.zero;
        
        HorizontalLayoutGroup contentLayout = content.AddComponent<HorizontalLayoutGroup>();
        contentLayout.spacing = levelSpacing;
        contentLayout.childAlignment = TextAnchor.MiddleLeft;
        contentLayout.childControlWidth = false;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = false;
        contentLayout.childForceExpandHeight = true;
        contentLayout.padding = new RectOffset(20, 20, 10, 10);
        
        ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        
        scroll.viewport = viewportRect;
        scroll.content = contentRect;
        
        PopulateLevels(content.transform, levelPrefab, coinsPrefab, gemsPrefab, skinsPrefab);
        
        return scrollArea;
    }
    
    void PopulateLevels(Transform content, GameObject levelPrefab, GameObject coinsPrefab, GameObject gemsPrefab, GameObject skinsPrefab)
    {
        List<int> freeRewards = GenerateRewardDistribution();
        List<int> premiumRewards = GenerateRewardDistribution();
        
        GameObject[] rewardPrefabs = { coinsPrefab, gemsPrefab, skinsPrefab };
        
        for (int i = 0; i < totalLevels; i++)
        {
            GameObject level = (GameObject)PrefabUtility.InstantiatePrefab(levelPrefab, content);
            level.name = $"Level_{i + 1}";
            
            Transform levelNumText = level.transform.Find("LevelNumber/Text");
            if (levelNumText != null)
            {
                TextMeshProUGUI tmp = levelNumText.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = (i + 1).ToString();
                }
            }
            
            Transform freeSlot = level.transform.Find("FreeRewardSlot");
            if (freeSlot != null)
            {
                GameObject freeReward = (GameObject)PrefabUtility.InstantiatePrefab(rewardPrefabs[freeRewards[i]], freeSlot);
                RectTransform rewardRect = freeReward.GetComponent<RectTransform>();
                rewardRect.anchorMin = new Vector2(0.5f, 0.5f);
                rewardRect.anchorMax = new Vector2(0.5f, 0.5f);
                rewardRect.anchoredPosition = Vector2.zero;
            }
            
            Transform premiumSlot = level.transform.Find("PremiumRewardSlot");
            if (premiumSlot != null)
            {
                GameObject premiumReward = (GameObject)PrefabUtility.InstantiatePrefab(rewardPrefabs[premiumRewards[i]], premiumSlot);
                RectTransform rewardRect = premiumReward.GetComponent<RectTransform>();
                rewardRect.anchorMin = new Vector2(0.5f, 0.5f);
                rewardRect.anchorMax = new Vector2(0.5f, 0.5f);
                rewardRect.anchoredPosition = Vector2.zero;
            }
        }
    }
    
    List<int> GenerateRewardDistribution()
    {
        List<int> distribution = new List<int>();
        
        for (int i = 0; i < coinsCount; i++) distribution.Add(0);
        for (int i = 0; i < gemsCount; i++) distribution.Add(1);
        for (int i = 0; i < skinsCount; i++) distribution.Add(2);
        
        for (int i = distribution.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = distribution[i];
            distribution[i] = distribution[j];
            distribution[j] = temp;
        }
        
        return distribution;
    }
    
    GameObject CreateFooter(Transform parent)
    {
        GameObject footer = new GameObject("Footer");
        footer.transform.SetParent(parent, false);
        
        RectTransform rect = footer.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.sizeDelta = new Vector2(0, 50);
        rect.anchoredPosition = Vector2.zero;
        
        Image bg = footer.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.18f);
        
        GameObject freeLabel = new GameObject("FreeLabel");
        freeLabel.transform.SetParent(footer.transform, false);
        RectTransform freeRect = freeLabel.AddComponent<RectTransform>();
        freeRect.anchorMin = new Vector2(0, 0);
        freeRect.anchorMax = new Vector2(0.3f, 1);
        freeRect.offsetMin = new Vector2(20, 0);
        freeRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI freeText = freeLabel.AddComponent<TextMeshProUGUI>();
        freeText.text = "<color=#888888>■</color> FREE TRACK";
        freeText.fontSize = 14;
        freeText.alignment = TextAlignmentOptions.MidlineLeft;
        freeText.color = Color.white;
        
        GameObject premLabel = new GameObject("PremiumLabel");
        premLabel.transform.SetParent(footer.transform, false);
        RectTransform premRect = premLabel.AddComponent<RectTransform>();
        premRect.anchorMin = new Vector2(0.3f, 0);
        premRect.anchorMax = new Vector2(0.6f, 1);
        premRect.offsetMin = Vector2.zero;
        premRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI premText = premLabel.AddComponent<TextMeshProUGUI>();
        premText.text = "<color=#FFD700>■</color> PREMIUM TRACK";
        premText.fontSize = 14;
        premText.alignment = TextAlignmentOptions.MidlineLeft;
        premText.color = Color.white;
        
        return footer;
    }
}
