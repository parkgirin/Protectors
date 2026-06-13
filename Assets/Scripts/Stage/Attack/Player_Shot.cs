using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Shot : MonoBehaviour
{
    public int bulletCount = 1;               // 발사 수
    public float attackTimer = 0f;           // 발사 주기 체크
    public float targetCheckTimer = 0f;      // 타겟 추적 주기 체크
    public GameObject currentTarget;         // 최근에 찾은 적 (Enemy GameObject)

    public float targetCheckInterval = 0.5f; // 타겟 갱신 간격 (초)

    private StageManager stageManager;

    private void OnEnable()
    {
        // 가능한 한 미리 찾아둠 (Inspector 대신 코드로 연결)
        stageManager = FindObjectOfType<StageManager>();
        if (stageManager == null)
            Debug.LogWarning("StageManager를 찾을 수 없습니다. 스테이지 관련 기능이 동작하지 않을 수 있습니다.");
    }

    void Update()
    {
        // 안전: EnemyManager/StageManager 준비 여부 확인
        if (EnemyManager.Instance == null)
        {
            // 적 관리자가 없으면 타겟 갱신 불가. 단, 발사 간격 기반 무조건 발사 옵션을 유지하려면 계속 진행할 수 있음.
        }
        bulletCount = stageManager.context.attackCount;

        attackTimer += Time.deltaTime;
        targetCheckTimer += Time.deltaTime;

        if (attackTimer >= StageManager.Instance.context.attackInterval)
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        // 안전 체크: StageManager 필요
        if (stageManager == null && StageManager.Instance != null)
            stageManager = StageManager.Instance;

        if (stageManager == null)
        {
            Debug.LogWarning("발사 실패: StageManager 미설정");
            attackTimer = 0f; // 선택: 실패 시에도 타이머를 리셋하거나 리셋하지 않음
            return;
        }

        currentTarget = EnemyManager.Instance.GetClosestEnemy(transform.position, StageManager.Instance.context.attackRange);
        Shoot(currentTarget, bulletCount);
        // Debug.Log("Attack!");
        attackTimer = 0f;
    }

    public void Shoot(GameObject target, int bulletCount, float spreadAngle = 10f)
    {
        // 1. 기준 방향 설정
        Vector3 baseDirection = (target != null)
            ? (target.transform.position - transform.position).normalized
            : transform.up;

        // 2. 중심 각도 (라디안 → 도)
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        // 3. 각도 간격 계산
        float angleStep = (bulletCount > 1) ? spreadAngle / (bulletCount - 1) : 0f;
        float startAngle = baseAngle - (spreadAngle / 2f);

        // 4. 총알 생성 루프
        for (int i = 0; i < bulletCount; i++)
        {
            GameObject bullet = (stageManager != null) ? stageManager.GetBullet() : null;
            if (bullet == null)
            {
                Debug.LogWarning("총알을 풀에서 가져올 수 없습니다.");
                continue;
            }

            // 현재 총알 각도
            float currentAngle = startAngle + (angleStep * i);

            // 각도를 방향 벡터로 변환
            Vector3 dir = new Vector3(
                Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                Mathf.Sin(currentAngle * Mathf.Deg2Rad),
                0
            );

            // 위치/각도 적용
            bullet.transform.position = transform.position;
            bullet.transform.rotation = Quaternion.Euler(0, 0, currentAngle - 90);
            bullet.SetActive(true);
        }
    }

    public void OnDrawGizmosSelected()
    {
        // 발사/타겟 기준 위치
        Vector3 origin = transform.position;

        // 최대 타겟 거리
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, stageManager.context.attackRange);
    }


}
