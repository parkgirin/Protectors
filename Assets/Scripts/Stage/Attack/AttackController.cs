using System.Collections.Generic;
using UnityEngine;

// 한 스테이지 동안 캐릭터의 전체 공격 구성을 관리한다.
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
        // timeScale이 0이어도 Update는 호출되므로 일시정지 중에는 준비된 공격을 실행하지 않는다.
        if (Time.timeScale <= 0f || !ResolveContext()) return;
        SynchronizeLoadout();

        foreach (AttackRuntimeData attack in context.RuntimeAttacks)
        {
            if (!cooldowns.ContainsKey(attack)) cooldowns.Add(attack, attack.EffectiveAttackInterval);
            cooldowns[attack] = Mathf.Min(
                attack.EffectiveAttackInterval,
                cooldowns[attack] + Time.deltaTime);
            if (cooldowns[attack] < attack.EffectiveAttackInterval) continue;

            // 실행기가 실제 공격에 성공할 때까지 충전 게이지를 가득 찬 상태로 유지한다.
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
            // 새로 활성화된 공격은 즉시 준비하며 실제 공격 후부터 재사용 대기시간을 계산한다.
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
    // 이번 호출에서 실제 공격 효과가 발생한 경우에만 true를 반환한다.
    bool Execute(AttackExecutionContext context);
}

public static class AttackExecutorFactory
{
    // 실제 효과를 실행할 수 있는 공격만 활성화 카드에 노출한다.
    public static bool IsSupported(AttackType type) => type == AttackType.Projectile;

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
