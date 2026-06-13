using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AttackSystemV2/AttackBuilder")]
public class AttackBuilder : ScriptableObject
{
    public List<BaseAttackSO> baseAttacks = new List<BaseAttackSO>();

    public AttackRuntimeData Build(string finalAttackId)
    {
        var finalData = new AttackRuntimeData(finalAttackId);

        foreach (var so in baseAttacks)
        {
            if (so == null) continue;

            foreach (var prop in so.properties)
            {
                // 동일 key면 덮어쓰기 (룰라이트에 매우 적합)
                finalData.SetProperty(prop.key, prop.GetValue());
            }
        }

        return finalData;
    }
}
