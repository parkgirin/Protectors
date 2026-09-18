using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageContext : MonoBehaviour
{
    public List<AttackRuntimeData> RuntimeAttacks { get; } = new List<AttackRuntimeData>();
    // Compatibility accessor for UI or systems that deliberately operate on the primary attack.
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
        RuntimeAttacks.Add(new AttackRuntimeData(attackData));
        return true;
    }
}
