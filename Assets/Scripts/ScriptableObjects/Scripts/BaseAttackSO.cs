using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BaseAttack", menuName = "ScriptableObjects/Attack/BaseAttack", order = 1)]
public class BaseAttackSO : ScriptableObject
{
    [Header("공통")]
    public string attackId;
    public string displayName;
    public float baseDamage = 10f;
    public float baseCooldown = 1f;
    public int baseCount = 1;

    public AttackPatternSO pattern;
    public List<PropertyValue> patternValues = new List<PropertyValue>();

    public List<AttackElementSO> element;
    public List<PropertyValue> elementValues = new List<PropertyValue>();

    public List<AttackEffectSO> effect;
    public List<PropertyValue> effectValues = new List<PropertyValue>();

}

[System.Serializable]
public class PropertyValue
{
    public string propertyName;
    public PropertyType propertyType;

    // 값은 타입별로 저장 (실제로는 하나만 사용)
    public int intValue;
    public float floatValue;
    public string stringValue;
    public bool boolValue;
}
