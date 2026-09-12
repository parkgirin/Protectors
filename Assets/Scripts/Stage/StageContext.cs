using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageContext : MonoBehaviour
{
    public AttackRuntimeData RuntimeAttack { get; private set; }

    public void InitializeAttack(AttackRuntimeData runtimeAttack)
    {
        RuntimeAttack = runtimeAttack;
    }

    public bool HasAttackData => RuntimeAttack != null;
}
