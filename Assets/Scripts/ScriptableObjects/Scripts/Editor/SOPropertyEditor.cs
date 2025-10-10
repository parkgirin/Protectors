using UnityEngine;
using UnityEditor;

// 공통화된 ScriptableObject 프로퍼티 에디터
public class SOPropertyEditor<T> : Editor where T : ScriptableObject
{
    public override void OnInspectorGUI()
    {
        T targetSO = (T)target;

        SerializedObject so = new SerializedObject(targetSO);
        so.Update();

        // 이름 필드 (SO별로 다른 변수명을 가질 수 있으므로 확인)
        SerializedProperty nameProp = so.FindProperty("patternName");
        if (nameProp == null) nameProp = so.FindProperty("elementName");
        if (nameProp == null) nameProp = so.FindProperty("effectName");

        if (nameProp != null)
            EditorGUILayout.PropertyField(nameProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);

        // 속성 리스트 표시
        SerializedProperty propertiesProp = so.FindProperty("properties");
        if (propertiesProp != null)
        {
            for (int i = 0; i < propertiesProp.arraySize; i++)
            {
                SerializedProperty prop = propertiesProp.GetArrayElementAtIndex(i);

                SerializedProperty propName = prop.FindPropertyRelative("propertyName");
                SerializedProperty propType = prop.FindPropertyRelative("propertyType");

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(propName, new GUIContent("Property Name"));
                EditorGUILayout.PropertyField(propType, new GUIContent("Property Type"));

                if (GUILayout.Button("Remove Property"))
                {
                    propertiesProp.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndVertical();
            }

            // 새 속성 추가 버튼
            if (GUILayout.Button("Add Property"))
            {
                propertiesProp.InsertArrayElementAtIndex(propertiesProp.arraySize);
                SerializedProperty newProp = propertiesProp.GetArrayElementAtIndex(propertiesProp.arraySize - 1);
                newProp.FindPropertyRelative("propertyName").stringValue = "NewProperty";
                newProp.FindPropertyRelative("propertyType").enumValueIndex = 0;
            }
        }

        so.ApplyModifiedProperties();
    }
}

// 각각의 SO에 맞게 커스텀에디터 지정
[CustomEditor(typeof(AttackPatternSO))]
public class AttackPatternSOEditor : SOPropertyEditor<AttackPatternSO> { }

[CustomEditor(typeof(AttackElementSO))]
public class AttackElementSOEditor : SOPropertyEditor<AttackElementSO> { }

[CustomEditor(typeof(AttackEffectSO))]
public class AttackEffectSOEditor : SOPropertyEditor<AttackEffectSO> { }
