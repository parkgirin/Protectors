using System.Collections.Generic;

public class AttackRuntimeData
{
    public string attackId;

    private Dictionary<string, object> values
        = new Dictionary<string, object>();

    public AttackRuntimeData(string attackId)
    {
        this.attackId = attackId;
    }

    public void SetProperty(string key, object value)
    {
        values[key] = value;
    }

    public bool Has(string key)
    {
        return values.ContainsKey(key);
    }

    public T Get<T>(string key)
    {
        if (!values.TryGetValue(key, out var value))
            return default;

        return (T)value;
    }

    public void Modify<T>(string key, T newValue)
    {
        if (values.ContainsKey(key))
            values[key] = newValue;
    }
}
