using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Growth Tree", menuName = "Game/Attack Growth Tree")]
public class AttackGrowthTreeSO : ScriptableObject
{
    [Tooltip("Choices are evaluated in this order. Nodes in the same exclusive group cannot both be selected.")]
    public List<AttackGrowthNode> nodes = new List<AttackGrowthNode>();
}

[System.Serializable]
public class AttackGrowthNode
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public AttackUpgradeEffect effect;
    public float value = 1f;

    [Header("Requirements")]
    public int minimumLevel;
    public float minimumDamage;
    public float minimumRange;
    public int minimumBulletCount;
    public int minimumPierce;
    public AttackBranch requiredBranch = AttackBranch.None;

    [Header("Branch")]
    public AttackBranch resultBranch = AttackBranch.None;
    public string exclusiveGroup;
}

public enum AttackUpgradeEffect
{
    DamageMultiplier,
    AttackIntervalMultiplier,
    RangeMultiplier,
    BulletCount,
    Pierce,
    BulletSpeedMultiplier,
    RapidFireBranch,
    PiercingVolleyBranch,
    SniperShotBranch
}
