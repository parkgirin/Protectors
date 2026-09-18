using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AttackRuntimeData
{
    // A branch is selected once per stage; the source SO is never changed.
    public AttackBranch branch = AttackBranch.None;
    public int level;
    public HashSet<string> selectedGrowthNodeIds = new HashSet<string>();
    public HashSet<string> selectedGrowthGroups = new HashSet<string>();
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

    // SO is the immutable template; only this runtime copy receives upgrades.
    public float EffectiveDamage => damage * damageMultiplier;
    public float EffectiveAttackInterval => Mathf.Max(0.05f, attackInterval * attackIntervalMultiplier);
    public float EffectiveRange => range * rangeMultiplier;
    public float EffectiveBulletSpeed => bulletSpeed * bulletSpeedMultiplier;
    public float EffectiveAttackSpriteScale => Mathf.Max(0.01f, attackSpriteScale * attackSpriteScaleMultiplier);

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

public enum AttackBranch
{
    None,
    RapidFire,
    PiercingVolley,
    SniperShot
}
