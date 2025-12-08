using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FightController))]
public class FightControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Auto-Fill Default Cards", GUILayout.Height(30)))
        {
            FillDefaultCards();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.HelpBox(
            "Default cards:\n" +
            "• Jab - 15 dmg, 0.019s timing\n" +
            "• Uppercut - 25 dmg, 0.014s timing\n" +
            "• Special - 35 dmg, 0.029s timing\n" +
            "• Defense - blocks incoming damage", 
            MessageType.Info);
    }
    
    void FillDefaultCards()
    {
        FightController controller = (FightController)target;
        SerializedObject so = new SerializedObject(controller);
        
        SerializedProperty cardsProp = so.FindProperty("cards");
        cardsProp.ClearArray();
        
        AddCard(cardsProp, 0, "Jab", "Jab 1", "Hit Body New", 0.019f, 15, false);
        AddCard(cardsProp, 1, "Uppercut", "Uppercut New", "Head Hit New", 0.014f, 25, false);
        AddCard(cardsProp, 2, "Special", "Mma Kick New", "Head Hit New", 0.029f, 35, false);
        AddCard(cardsProp, 3, "Defense", "Right Block New", "", 0f, 0, true);
        
        so.ApplyModifiedProperties();
        
        EditorUtility.SetDirty(controller);
        
        Debug.Log("Cards configuration filled with default values!");
    }
    
    void AddCard(SerializedProperty cardsProp, int index, string name, string animState, string victimAnimState, float timing, int damage, bool isDefense)
    {
        cardsProp.InsertArrayElementAtIndex(index);
        SerializedProperty card = cardsProp.GetArrayElementAtIndex(index);
        
        card.FindPropertyRelative("cardName").stringValue = name;
        card.FindPropertyRelative("animationStateName").stringValue = animState;
        card.FindPropertyRelative("victimAnimationStateName").stringValue = victimAnimState;
        card.FindPropertyRelative("hitTimingSeconds").floatValue = timing;
        card.FindPropertyRelative("baseDamage").intValue = damage;
        card.FindPropertyRelative("isDefense").boolValue = isDefense;
    }
}
