using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 하나의 공격에 대한 "설계도"
/// - 실행 로직 없음
/// - 레벨별로 어떤 값과 옵션이 적용되는지만 저장
/// </summary>
[CreateAssetMenu(menuName = "Attack/Attack Data")]
public class AttackDataSO : ScriptableObject
{
    [Header("공격 기본 정보")]
    [Tooltip("내부 식별용 ID (저장/로드, 비교용)")]
    public string attackId;

    [Tooltip("UI에 표시될 공격 이름")]
    public string attackName;

    [Header("레벨별 공격 데이터")]
    [Tooltip("레벨 1부터 순서대로 들어가는 데이터 리스트")]
    public List<AttackLevelData> levelDatas;

    /// <summary>
    /// 현재 레벨에 해당하는 데이터 반환
    /// (레벨은 1부터 시작한다고 가정)
    /// </summary>
    public AttackLevelData GetLevelData(int level)
    {
        // 레벨 범위 보호
        if (level <= 0)
            level = 1;

        if (level > levelDatas.Count)
            level = levelDatas.Count;

        return levelDatas[level - 1];
    }
}
