using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AttackSystemV2/BaseAttack")]
public class BaseAttackSO : ScriptableObject
{
    [Header("식별자")]
    public string attackId;

    [Header("유동 속성 목록")]
    public List<AttackProperty> properties = new List<AttackProperty>();

    /// <summary>
    /// 런타임에서 사용할 데이터 생성
    /// </summary>
    public AttackRuntimeData CreateRuntimeData()
    {
        var runtime = new AttackRuntimeData(attackId);

        foreach (var prop in properties)
        {
            runtime.SetProperty(prop.key, prop.GetValue());
        }

        return runtime;
    }
}
