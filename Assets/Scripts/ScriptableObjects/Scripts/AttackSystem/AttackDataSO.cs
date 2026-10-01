using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Attack",
    menuName = "Game/Attack Data")]
public class AttackDataSO : ScriptableObject
{
    [Header("식별")]
    public string id;
    public string attackName;

    [Header("표시")]
    public Sprite icon;
    public Sprite attackSprite;
    [Min(0.01f)] public float attackSpriteScale = 1f;
    [TextArea]
    public string description;

    [Header("기본 능력치")]
    public float damage = 1f;
    public float attackInterval = 0.3f;
    public float range = 0.8f;

    [Header("분리 예정")]
    public int bulletCount = 1;
    public float bulletSpreadAngle = 0f;
    public int bulletHP = 1;
    public float bulletSpeed = 10f;

    [Header("분류")]
    public AttackType attackType;
    public List<AttackTag> tags = new();

    [Header("성장")]
    public List<GrowthNode> growthNodes = new();
    public AttackGrowthTreeSO growthTree;
}

public enum AttackType
{
    Projectile,    // 투사체
    Area,          // 범위
    Summon,        // 소환
    Beam,          // 레이저
    Passive        // 패시브
}

public enum AttackTag
{
    Fire,
    Ice,
    Electric,
    Drone,
    Sword,
    Explosion
}

[System.Serializable]
public class GrowthNode
{
    public string nodeName;
    public float nodeValue;

    [TextArea]
    public string description;

    // 아직은 비워두고,
    // 나중에 Modifier나 Effect를 추가
}
