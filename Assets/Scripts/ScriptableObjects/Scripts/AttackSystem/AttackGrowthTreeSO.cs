using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Growth Tree", menuName = "Game/Attack Growth Tree")]
public class AttackGrowthTreeSO : ScriptableObject
{
    [Tooltip("선택 조건을 만족한 노드 중 최대 세 개를 무작위로 표시합니다.")]
    public List<AttackGrowthNode> nodes = new List<AttackGrowthNode>();
}

[System.Serializable]
public class AttackGrowthNode
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public List<AttackGrowthModifier> modifiers = new List<AttackGrowthModifier>();

    [Header("선택 조건")]
    [Min(0)] public int minimumLevel;
    [Min(0)] public int minimumBulletCount;
    [Min(0)] public int minimumPierce;
    [Min(1)] public int maxSelectionCount = 1;
    public List<AttackGrowthRequirement> requirements = new List<AttackGrowthRequirement>();
    public List<AttackBehavior> requiredBehaviors = new List<AttackBehavior>();

    [Header("선택 결과")]
    public List<AttackBehavior> grantedBehaviors = new List<AttackBehavior>();
    public List<string> lockedNodeIds = new List<string>();
    public string exclusiveGroup;
}

[System.Serializable]
public class AttackGrowthModifier
{
    public AttackUpgradeEffect effect;
    public float value = 1f;
}

[System.Serializable]
public class AttackGrowthRequirement
{
    public string nodeId;
    [Min(1)] public int selectionCount = 1;
}

public enum AttackUpgradeEffect
{
    DamageMultiplier,
    AttackIntervalMultiplier,
    RangeMultiplier,
    BulletCount,
    Pierce,
    BulletSpeedMultiplier,
    SpreadAngle,
    SpriteScaleMultiplier,
    MergeDamageCoefficient,
    FragmentCount,
    FragmentDamageRatio,
    FragmentPierce,
    ExplosionDamageRatio,
    ExplosionRadiusMultiplier,
    LifeStealRatio,
    CriticalChance,
    CriticalDamageMultiplier,
    FragmentChance,
    FragmentCountVariance,
    FragmentExplosionChance
}

public enum AttackBehavior
{
    RapidFire,
    MergeProjectiles,
    FragmentOnHit,
    LargeProjectile,
    ExplodeOnHit,
    HomingFragment,
    FragmentExplosion,
    LingeringFire,
    GiantMergedProjectile,
    MergedFragmentProjectile,
    ChainExplosionProjectile,
    SiegeMergedProjectile
}
