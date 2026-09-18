using System.Collections.Generic;
using UnityEngine;

// Owns a character's complete attack loadout during one stage.
public class AttackController : MonoBehaviour
{
    private readonly Dictionary<AttackRuntimeData, float> cooldowns = new Dictionary<AttackRuntimeData, float>();
    private readonly Dictionary<AttackRuntimeData, IAttackExecutor> executors = new Dictionary<AttackRuntimeData, IAttackExecutor>();
    private StageContext context;

    public float GetCooldownRemaining(AttackRuntimeData attack)
    {
        if (attack == null || !cooldowns.TryGetValue(attack, out float elapsed)) return 0f;
        return Mathf.Max(0f, attack.EffectiveAttackInterval - elapsed);
    }

    public bool IsActive(AttackRuntimeData attack)
    {
        return attack != null && executors.ContainsKey(attack);
    }

    private void Update()
    {
        if (!ResolveContext()) return;
        SynchronizeLoadout();

        foreach (AttackRuntimeData attack in context.RuntimeAttacks)
        {
            if (!cooldowns.ContainsKey(attack)) cooldowns.Add(attack, attack.EffectiveAttackInterval);
            cooldowns[attack] = Mathf.Min(
                attack.EffectiveAttackInterval,
                cooldowns[attack] + Time.deltaTime);
            if (cooldowns[attack] < attack.EffectiveAttackInterval) continue;

            // A full gauge stays full until an executor actually performs its attack.
            if (executors[attack].Execute(new AttackExecutionContext(transform, attack)))
                cooldowns[attack] = 0f;
        }
    }

    private bool ResolveContext()
    {
        if (context == null && StageManager.Instance != null) context = StageManager.Instance.context;
        return context != null && EnemyManager.Instance != null;
    }

    private void SynchronizeLoadout()
    {
        foreach (AttackRuntimeData attack in context.RuntimeAttacks)
        {
            if (executors.ContainsKey(attack)) continue;
            // Newly activated attacks are ready. Their cooldown starts only after a real attack.
            cooldowns.Add(attack, attack.EffectiveAttackInterval);
            executors.Add(attack, AttackExecutorFactory.Create(attack.source.attackType));
        }
    }
}

public readonly struct AttackExecutionContext
{
    public readonly Transform owner;
    public readonly AttackRuntimeData attack;

    public AttackExecutionContext(Transform owner, AttackRuntimeData attack)
    {
        this.owner = owner;
        this.attack = attack;
    }

    public GameObject FindTarget()
    {
        return EnemyManager.Instance == null ? null : EnemyManager.Instance.GetClosestEnemy(owner.position, attack.EffectiveRange);
    }
}

public interface IAttackExecutor
{
    // Returns true only when this update produced the attack's actual gameplay effect.
    bool Execute(AttackExecutionContext context);
}

public static class AttackExecutorFactory
{
    public static IAttackExecutor Create(AttackType type)
    {
        switch (type)
        {
            case AttackType.Projectile: return new ProjectileAttackExecutor();
            case AttackType.Summon: return new SummonAttackExecutor();
            default: return new DisabledAttackExecutor(type);
        }
    }
}
