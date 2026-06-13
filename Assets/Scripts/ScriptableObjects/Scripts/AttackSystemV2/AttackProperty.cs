using System;

public enum PropertyType
{
    Int,
    Float,
    Bool,
    String
}

[Serializable]
public class AttackProperty
{
    public string key;           // 예: damage, cooldown, pierceCount
    public PropertyType type;

    // 실제 값 (타입별 하나만 사용)
    public int intValue;
    public float floatValue;
    public bool boolValue;
    public string stringValue;

    public object GetValue()
    {
        return type switch
        {
            PropertyType.Int => intValue,
            PropertyType.Float => floatValue,
            PropertyType.Bool => boolValue,
            PropertyType.String => stringValue,
            _ => null
        };
    }
}
