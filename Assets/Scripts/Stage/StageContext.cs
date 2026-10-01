using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageContext : MonoBehaviour
{
    public List<AttackRuntimeData> RuntimeAttacks { get; } = new List<AttackRuntimeData>();
    // 주 공격만 사용하는 UI와 기존 시스템을 위한 호환 접근자다.
    public AttackRuntimeData RuntimeAttack => RuntimeAttacks.Count > 0 ? RuntimeAttacks[0] : null;

    public void InitializeAttack(AttackRuntimeData runtimeAttack)
    {
        RuntimeAttacks.Clear();
        if (runtimeAttack != null) RuntimeAttacks.Add(runtimeAttack);
    }

    public void InitializeAttacks(IEnumerable<AttackRuntimeData> runtimeAttacks)
    {
        RuntimeAttacks.Clear();
        foreach (AttackRuntimeData runtimeAttack in runtimeAttacks)
            if (runtimeAttack != null) RuntimeAttacks.Add(runtimeAttack);
    }

    public bool HasAttackData => RuntimeAttacks.Count > 0;

    public bool ActivateAttack(AttackDataSO attackData)
    {
        if (attackData == null) return false;
        foreach (AttackRuntimeData attack in RuntimeAttacks)
            if (attack.source == attackData) return false;
        // 강화가 원본 SO 에셋을 변경하지 않도록 이번 플레이용 복사본을 생성한다.
        RuntimeAttacks.Add(new AttackRuntimeData(attackData));
        return true;
    }
}
