using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseAttackSO))]
public class BaseAttackSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BaseAttackSO baseAttack = (BaseAttackSO)target;

        // 기본 Inspector 표시
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pattern Properties", EditorStyles.boldLabel);
        if (baseAttack.pattern != null)
        {
            SyncProperties(baseAttack.pattern.properties, baseAttack.patternValues);
            DrawPropertyValues(baseAttack.patternValues);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Element Properties", EditorStyles.boldLabel);
        if (baseAttack.element != null)
        {
            foreach (var elem in baseAttack.element)
            {
                if (elem == null) continue;

                EditorGUILayout.LabelField(elem.name, EditorStyles.boldLabel);
                SyncProperties(elem.properties, baseAttack.elementValues);
                DrawPropertyValues(baseAttack.elementValues);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Effect Properties", EditorStyles.boldLabel);
        if (baseAttack.effect != null)
        {
            foreach (var eff in baseAttack.effect)
            {
                if (eff == null) continue;

                EditorGUILayout.LabelField(eff.name, EditorStyles.boldLabel);
                SyncProperties(eff.properties, baseAttack.effectValues);
                DrawPropertyValues(baseAttack.effectValues);
            }
        }

        // 변경 사항 저장
        if (GUI.changed)
        {
            EditorUtility.SetDirty(baseAttack);
        }
    }

    private void SyncProperties(System.Collections.Generic.List<Property> source, System.Collections.Generic.List<PropertyValue> target)
    {
        // 누락된 속성 추가
        foreach (var p in source)
        {
            if (!target.Exists(v => v.propertyName == p.propertyName))
            {
                target.Add(new PropertyValue
                {
                    propertyName = p.propertyName,
                    propertyType = p.propertyType
                });
            }
        }

        // source에서 제거된 속성 삭제
        target.RemoveAll(v => !source.Exists(p => p.propertyName == v.propertyName));
    }

    private void DrawPropertyValues(System.Collections.Generic.List<PropertyValue> values)
    {
        foreach (var propValue in values)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(propValue.propertyName, GUILayout.Width(120));

            switch (propValue.propertyType)
            {
                case PropertyType.Int:
                    propValue.intValue = EditorGUILayout.IntField(propValue.intValue);
                    break;
                case PropertyType.Float:
                    propValue.floatValue = EditorGUILayout.FloatField(propValue.floatValue);
                    break;
                case PropertyType.String:
                    propValue.stringValue = EditorGUILayout.TextField(propValue.stringValue);
                    break;
                case PropertyType.Bool:
                    propValue.boolValue = EditorGUILayout.Toggle(propValue.boolValue);
                    break;
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
