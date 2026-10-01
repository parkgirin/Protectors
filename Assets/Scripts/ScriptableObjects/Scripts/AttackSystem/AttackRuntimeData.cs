using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AttackRuntimeData
{
    public const float MaxCriticalChance = 0.75f;
    public const float MaxFragmentChance = 1f;
    public const float MaxFragmentExplosionChance = 1f;

    public int level;
    public HashSet<string> selectedGrowthNodeIds = new HashSet<string>();
    public HashSet<string> selectedGrowthGroups = new HashSet<string>();
    public Dictionary<string, int> growthNodeSelectionCounts = new Dictionary<string, int>();
    public HashSet<AttackBehavior> behaviors = new HashSet<AttackBehavior>();
    public HashSet<string> lockedGrowthNodeIds = new HashSet<string>();
    // 원본 데이터
    public AttackDataSO source;

    // 런타임에서 변경되는 값
    public float damage;
    public float attackInterval;
    public float range;

    // 현재는 분리 예정이지만 런타임에서 변경될 값
    public int bulletCount;
    public float bulletSpreadAngle;
    public int bulletHP;
    public float bulletSpeed;
    public float attackSpriteScale;

    // 각 값에 곱해져 스테이지 전반에서 관리되는 배수
    public float damageMultiplier = 1f;
    public float attackIntervalMultiplier = 1f;
    public float rangeMultiplier = 1f;
    public float bulletSpeedMultiplier = 1f;
    public float attackSpriteScaleMultiplier = 1f;
    public float mergeDamageCoefficient = 0.85f;
    public int fragmentCount = 3;
    public int fragmentCountVariance;
    public float fragmentDamageRatio = 0.12f;
    public float fragmentChance = 0.7f;
    public float fragmentExplosionChance;
    public int fragmentPierce = 1;
    public float explosionDamageRatio = 0.65f;
    public float explosionRadiusMultiplier = 1f;
    public float lifeStealRatio;
    public float criticalChance = 0.05f;
    public float criticalDamageMultiplier = 1.5f;

    // SO는 변경하지 않는 원본이며 강화는 이 런타임 복사본에만 적용한다.
    public float EffectiveDamage => damage * Mathf.Max(0.01f, damageMultiplier);
    public float EffectiveAttackInterval => Mathf.Max(0.05f, attackInterval * attackIntervalMultiplier);
    public float EffectiveRange => range * rangeMultiplier;
    public float EffectiveBulletSpeed => bulletSpeed * Mathf.Max(0.1f, bulletSpeedMultiplier);
    public float EffectiveAttackSpriteScale => Mathf.Max(0.01f, attackSpriteScale * attackSpriteScaleMultiplier);
    public float EffectiveCriticalChance => Mathf.Clamp(criticalChance, 0f, MaxCriticalChance);
    public float EffectiveCriticalDamageMultiplier => Mathf.Clamp(criticalDamageMultiplier, 1f, 4f);

    public float GetProjectileCriticalChance(float activeTime)
    {
        // 치명타 확률은 빠른 탄환일수록 높아진다. 활공 시간은 확률에 섞지 않아
        // 오래 빗나간 탄환이 의도치 않게 치명타에 유리해지는 상황을 막는다.
        float speed = Mathf.Max(0.1f, bulletSpeedMultiplier);
        return Mathf.Clamp(EffectiveCriticalChance * speed, 0f, MaxCriticalChance);
    }

    public float GetProjectileCriticalDamageMultiplier(float activeTime)
    {
        float flightTime = Mathf.Max(0f, activeTime);
        // 치명타 피해는 오래 활공한 포탄일수록 커진다. 10초 수명에서는 최대 두 배
        // 보정이지만 전체 치명타 배수는 기존 상한(4배)을 유지한다.
        return Mathf.Clamp(EffectiveCriticalDamageMultiplier * (1f + flightTime * 0.1f), 1f, 4f);
    }

    public int GetGrowthNodeSelectionCount(string nodeId)
    {
        return !string.IsNullOrEmpty(nodeId) && growthNodeSelectionCounts.TryGetValue(nodeId, out int count) ? count : 0;
    }

    public void RecordGrowthNodeSelection(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId)) return;
        growthNodeSelectionCounts[nodeId] = GetGrowthNodeSelectionCount(nodeId) + 1;
        selectedGrowthNodeIds.Add(nodeId);
    }

    public bool HasBehavior(AttackBehavior behavior) => behaviors.Contains(behavior);

    // 발사 수는 합체 재료로 보존하고 실제 발사 수만 한 발로 고정한다.
    public int EffectiveBulletCount => HasBehavior(AttackBehavior.MergeProjectiles) ? 1 : Mathf.Max(1, bulletCount);
    public float ProjectileDamage => EffectiveDamage
        * (HasBehavior(AttackBehavior.MergeProjectiles) ? mergeDamageCoefficient * Mathf.Max(1, bulletCount) : 1f)
        * ProjectileSizeDamageMultiplier;
    public int ProjectilePierce => HasBehavior(AttackBehavior.SiegeMergedProjectile) ? 1 : Mathf.Max(1,
        bulletHP + (HasBehavior(AttackBehavior.MergeProjectiles) ? Mathf.Max(0, bulletCount - 1) : 0)
        - (HasBehavior(AttackBehavior.ExplodeOnHit) ? 1 : 0)
        - (HasBehavior(AttackBehavior.MergedFragmentProjectile) ? 1 : 0));
    public float ProjectileScale => EffectiveAttackSpriteScale * (HasBehavior(AttackBehavior.MergeProjectiles) ? Mathf.Sqrt(Mathf.Max(1, bulletCount)) : 1f);
    // 폭발 범위에는 실제 크기를 반영하되 초대형 합체탄이 화면을 과도하게 덮지 않도록 제한한다.
    public float ExplosionScaleMultiplier => Mathf.Clamp(ProjectileScale, 1f, 2.5f);
    // 크기 100% 증가당 피해는 10%만 증가시켜 크기 강화가 피해를 과도하게 키우지 않게 한다.
    public float ProjectileSizeDamageMultiplier => Mathf.Clamp(1f + (ProjectileScale / Mathf.Max(0.01f, attackSpriteScale) - 1f) * 0.1f, 0.9f, 1.25f);

    public float RollProjectileDamage(float activeTime = 0f)
    {
        return ProjectileDamage * (Random.value < GetProjectileCriticalChance(activeTime) ? GetProjectileCriticalDamageMultiplier(activeTime) : 1f);
    }

    public bool CanGrantBehavior(AttackBehavior behavior)
    {
        if (HasBehavior(behavior)) return false;
        switch (behavior)
        {
            case AttackBehavior.RapidFire: return true;
            case AttackBehavior.MergeProjectiles: return bulletCount >= 2;
            case AttackBehavior.FragmentOnHit: return bulletCount >= 2 || ProjectilePierce >= 2;
            case AttackBehavior.HomingFragment: return HasBehavior(AttackBehavior.FragmentOnHit);
            case AttackBehavior.LargeProjectile:
                return GetGrowthNodeSelectionCount("artillery_damage") >= 1 || HasBehavior(AttackBehavior.MergeProjectiles);
            case AttackBehavior.ExplodeOnHit:
                return level >= 6 && (HasBehavior(AttackBehavior.MergeProjectiles) || HasBehavior(AttackBehavior.LargeProjectile));
            case AttackBehavior.FragmentExplosion:
            case AttackBehavior.ChainExplosionProjectile:
                return HasBehavior(AttackBehavior.ExplodeOnHit) && HasBehavior(AttackBehavior.FragmentOnHit);
            case AttackBehavior.LingeringFire:
                return HasBehavior(AttackBehavior.ExplodeOnHit) && HasBehavior(AttackBehavior.LargeProjectile);
            case AttackBehavior.GiantMergedProjectile:
                return bulletCount >= 3 && HasBehavior(AttackBehavior.MergeProjectiles) && HasBehavior(AttackBehavior.LargeProjectile);
            case AttackBehavior.MergedFragmentProjectile:
                return bulletCount >= 3 && HasBehavior(AttackBehavior.MergeProjectiles) && HasBehavior(AttackBehavior.FragmentOnHit);
            case AttackBehavior.SiegeMergedProjectile:
                return GetGrowthNodeSelectionCount("artillery_damage") >= 1 && HasBehavior(AttackBehavior.MergeProjectiles) && HasBehavior(AttackBehavior.ExplodeOnHit);
            default: return false;
        }
    }

    public AttackRuntimeData(AttackDataSO data)
    {
        source = data;

        // 기본 능력치 복사
        damage = data.damage;
        attackInterval = data.attackInterval;
        range = data.range;

        // 공격 전용 데이터 복사
        bulletCount = data.bulletCount;
        bulletSpreadAngle = data.bulletSpreadAngle;
        bulletHP = data.bulletHP;
        bulletSpeed = data.bulletSpeed;
        attackSpriteScale = data.attackSpriteScale;
    }
}
