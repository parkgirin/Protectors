using UnityEngine;

/// <summary>
/// 특정 레벨에서 공격이 가지는 "최종 상태 값"
/// - 이 데이터 자체는 계산하지 않음
/// - 공격 로직이 읽어서 사용
/// </summary>
[System.Serializable]
public class AttackLevelData
{
    [Header("기본 공격 구조")]

    public float damage = 100f;

    [Tooltip("1회 발사 시 생성되는 탄막 수")]
    public int projectilesPerShot = 3;

    [Tooltip("한 번 공격 시 몇 회 연속 발사되는지")]
    public int attacksPerFire = 1;

    [Tooltip("공격 간 쿨타임 (초)")]
    public float cooldown = 1f;

    [Tooltip("발화 지연 시간 (초) - 0이면 즉시 발사")]
    public float ignitionTime = 0f;

    [Header("확장 효과")]

    [Tooltip("업그레이드 리스트에 분화 활성화 여부")]
    public bool enableSplit = false;

    [Tooltip("분화 시 생성되는 추가 탄막 수")]
    public int splitProjectileCount = 0;

    [Tooltip("분화 시 탄막이 퍼지는 각도 범위 (0~180)")]
    public float splitAngleRange = 0f;

    [Tooltip("업그레이드 리스트에 관통 활성화 여부")]
    public bool enablePierce = false;

    [Tooltip("탄막이 관통할 수 있는 횟수 (0이면 관통 없음)")]
    public int pierceCount = 0;

    [Tooltip("업그레이드 리스트에 경로 발화 활성화 여부")]
    public bool enableBurn = false;

    [Tooltip("발화(도트) 지속 시간 (0이면 효과 없음)")]
    public float burnDuration = 0f;

    [Tooltip("발화(도트) 초당 피해량")]
    public float burnDamagePerSecond = 0f;

    [Tooltip("진화 탄막 활성화 여부")]
    public bool enableHoming = false;
}
