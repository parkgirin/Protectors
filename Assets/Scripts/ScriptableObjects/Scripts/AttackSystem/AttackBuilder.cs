using UnityEngine;

public class AttackBuilder : MonoBehaviour
{
    public AttackRuntimeData Build(AttackDataSO data)
    {
        return new AttackRuntimeData(data);
    }
}