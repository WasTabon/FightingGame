using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(RouletteController))]
public class RouletteControllerEditor : Editor
{
    private SerializedProperty roulettePanelProp;
    private SerializedProperty rouletteWheelProp;
    private SerializedProperty arrowTransformProp;
    private SerializedProperty resultTextProp;
    private SerializedProperty zonesProp;
    private SerializedProperty spinDurationProp;
    private SerializedProperty minRotationsProp;
    private SerializedProperty maxRotationsProp;
    private SerializedProperty resultTextDurationProp;
    private SerializedProperty criticalTextsProp;
    private SerializedProperty normalTextsProp;
    private SerializedProperty weakTextsProp;
    private SerializedProperty defenseGoodTextsProp;
    private SerializedProperty defenseNormalTextsProp;
    private SerializedProperty defenseBadTextsProp;
    
    private bool showPreview = true;
    private float previewSize = 150f;
    
    void OnEnable()
    {
        roulettePanelProp = serializedObject.FindProperty("roulettePanel");
        rouletteWheelProp = serializedObject.FindProperty("rouletteWheel");
        arrowTransformProp = serializedObject.FindProperty("arrowTransform");
        resultTextProp = serializedObject.FindProperty("resultText");
        zonesProp = serializedObject.FindProperty("zones");
        spinDurationProp = serializedObject.FindProperty("spinDuration");
        minRotationsProp = serializedObject.FindProperty("minRotations");
        maxRotationsProp = serializedObject.FindProperty("maxRotations");
        resultTextDurationProp = serializedObject.FindProperty("resultTextDuration");
        criticalTextsProp = serializedObject.FindProperty("criticalTexts");
        normalTextsProp = serializedObject.FindProperty("normalTexts");
        weakTextsProp = serializedObject.FindProperty("weakTexts");
        defenseGoodTextsProp = serializedObject.FindProperty("defenseGoodTexts");
        defenseNormalTextsProp = serializedObject.FindProperty("defenseNormalTexts");
        defenseBadTextsProp = serializedObject.FindProperty("defenseBadTexts");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("UI References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(roulettePanelProp);
        EditorGUILayout.PropertyField(rouletteWheelProp);
        EditorGUILayout.PropertyField(arrowTransformProp);
        EditorGUILayout.PropertyField(resultTextProp);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Zones Configuration", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(zonesProp, true);
        
        float totalAngle = 0f;
        for (int i = 0; i < zonesProp.arraySize; i++)
        {
            var zone = zonesProp.GetArrayElementAtIndex(i);
            totalAngle += zone.FindPropertyRelative("angleSize").floatValue;
        }
        
        if (Mathf.Abs(totalAngle - 360f) > 0.01f)
        {
            EditorGUILayout.HelpBox($"Total angle: {totalAngle}° (should be 360°)", MessageType.Warning);
            
            if (GUILayout.Button("Normalize to 360°"))
            {
                NormalizeAngles();
            }
        }
        else
        {
            EditorGUILayout.HelpBox($"Total angle: {totalAngle}° ✓", MessageType.Info);
        }
        
        EditorGUILayout.Space(10);
        showPreview = EditorGUILayout.Foldout(showPreview, "Roulette Preview", true);
        
        if (showPreview && zonesProp.arraySize > 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            Rect previewRect = GUILayoutUtility.GetRect(previewSize * 2, previewSize * 2);
            previewRect.x = (EditorGUIUtility.currentViewWidth - previewSize * 2) / 2;
            
            DrawRoulettePreview(previewRect);
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            DrawZoneLegend();
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(spinDurationProp);
        EditorGUILayout.PropertyField(minRotationsProp);
        EditorGUILayout.PropertyField(maxRotationsProp);
        EditorGUILayout.PropertyField(resultTextDurationProp);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Result Text Effects", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(criticalTextsProp, true);
        EditorGUILayout.PropertyField(normalTextsProp, true);
        EditorGUILayout.PropertyField(weakTextsProp, true);
        EditorGUILayout.PropertyField(defenseGoodTextsProp, true);
        EditorGUILayout.PropertyField(defenseNormalTextsProp, true);
        EditorGUILayout.PropertyField(defenseBadTextsProp, true);
        
        if (GUILayout.Button("Generate Wheel Texture"))
        {
            GenerateWheelTexture();
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    void NormalizeAngles()
    {
        if (zonesProp.arraySize == 0) return;
        
        float totalAngle = 0f;
        for (int i = 0; i < zonesProp.arraySize; i++)
        {
            totalAngle += zonesProp.GetArrayElementAtIndex(i).FindPropertyRelative("angleSize").floatValue;
        }
        
        if (totalAngle <= 0) return;
        
        float scale = 360f / totalAngle;
        for (int i = 0; i < zonesProp.arraySize; i++)
        {
            var angleProp = zonesProp.GetArrayElementAtIndex(i).FindPropertyRelative("angleSize");
            angleProp.floatValue *= scale;
        }
    }
    
    void DrawRoulettePreview(Rect rect)
    {
        Vector2 center = rect.center;
        float radius = previewSize;
        
        Handles.BeginGUI();
        
        float currentAngle = 90f;
        
        for (int i = 0; i < zonesProp.arraySize; i++)
        {
            var zone = zonesProp.GetArrayElementAtIndex(i);
            Color zoneColor = zone.FindPropertyRelative("zoneColor").colorValue;
            float angleSize = zone.FindPropertyRelative("angleSize").floatValue;
            
            DrawPieSlice(center, radius, currentAngle, angleSize, zoneColor);
            currentAngle -= angleSize;
        }
        
        Handles.color = Color.black;
        Handles.DrawWireDisc(center, Vector3.forward, radius);
        
        Vector2 arrowTip = center + Vector2.up * (radius + 20);
        Vector2 arrowLeft = center + Vector2.up * radius + Vector2.left * 10;
        Vector2 arrowRight = center + Vector2.up * radius + Vector2.right * 10;
        
        Handles.color = Color.white;
        Handles.DrawAAConvexPolygon(arrowTip, arrowLeft, arrowRight);
        Handles.color = Color.black;
        Handles.DrawLine(arrowTip, arrowLeft);
        Handles.DrawLine(arrowTip, arrowRight);
        Handles.DrawLine(arrowLeft, arrowRight);
        
        Handles.EndGUI();
    }
    
    void DrawPieSlice(Vector2 center, float radius, float startAngle, float angleSize, Color color)
    {
        int segments = Mathf.Max(3, (int)(angleSize / 5));
        Vector3[] vertices = new Vector3[segments + 2];
        
        vertices[0] = center;
        
        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle - (angleSize * i / segments);
            float rad = angle * Mathf.Deg2Rad;
            vertices[i + 1] = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        }
        
        Handles.color = color;
        Handles.DrawAAConvexPolygon(vertices);
    }
    
    void DrawZoneLegend()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Zone Legend", EditorStyles.boldLabel);
        
        for (int i = 0; i < zonesProp.arraySize; i++)
        {
            var zone = zonesProp.GetArrayElementAtIndex(i);
            string name = zone.FindPropertyRelative("zoneName").stringValue;
            Color color = zone.FindPropertyRelative("zoneColor").colorValue;
            float angle = zone.FindPropertyRelative("angleSize").floatValue;
            float multiplier = zone.FindPropertyRelative("damageMultiplier").floatValue;
            
            EditorGUILayout.BeginHorizontal();
            
            Rect colorRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20));
            EditorGUI.DrawRect(colorRect, color);
            
            EditorGUILayout.LabelField($"{name}: {angle:F1}° ({angle/360f*100:F1}%) - x{multiplier:F2}");
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    void GenerateWheelTexture()
    {
        int size = 512;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 5;
        
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        float currentAngle = 90f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float dist = Vector2.Distance(pos, center);
                
                if (dist <= radius)
                {
                    Vector2 dir = (pos - center).normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    if (angle < 0) angle += 360f;
                    
                    float checkAngle = 90f;
                    Color pixelColor = Color.gray;
                    
                    for (int i = 0; i < zonesProp.arraySize; i++)
                    {
                        var zone = zonesProp.GetArrayElementAtIndex(i);
                        float angleSize = zone.FindPropertyRelative("angleSize").floatValue;
                        
                        float startAngle = checkAngle;
                        float endAngle = checkAngle - angleSize;
                        
                        if (endAngle < 0)
                        {
                            if (angle <= startAngle || angle >= 360 + endAngle)
                            {
                                pixelColor = zone.FindPropertyRelative("zoneColor").colorValue;
                                break;
                            }
                        }
                        else if (angle <= startAngle && angle > endAngle)
                        {
                            pixelColor = zone.FindPropertyRelative("zoneColor").colorValue;
                            break;
                        }
                        
                        checkAngle -= angleSize;
                        if (checkAngle < 0) checkAngle += 360f;
                    }
                    
                    pixels[y * size + x] = pixelColor;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        byte[] pngData = texture.EncodeToPNG();
        string path = EditorUtility.SaveFilePanel("Save Roulette Texture", "Assets", "RouletteWheel", "png");
        
        if (!string.IsNullOrEmpty(path))
        {
            System.IO.File.WriteAllBytes(path, pngData);
            AssetDatabase.Refresh();
            Debug.Log($"Roulette texture saved to: {path}");
        }
        
        DestroyImmediate(texture);
    }
}
