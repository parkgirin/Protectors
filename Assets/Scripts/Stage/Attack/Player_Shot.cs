using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Shot : MonoBehaviour
{
    public float attackTimer = 0f;           // 발사 주기 체크
    public float targetCheckTimer = 0f;      // 타겟 추적 주기 체크
    public GameObject currentTarget;         // 최근에 찾은 적 (Enemy GameObject)

    private void OnEnable()
    {

    }

    private void Update()
    {
        if (StageManager.Instance == null || StageManager.Instance.context == null)
            return;

        AttackRuntimeData attack = StageManager.Instance.context.RuntimeAttack;
        if (attack == null)
            return;

        attackTimer += Time.deltaTime;
        targetCheckTimer += Time.deltaTime;

        if (attackTimer >= attack.attackInterval)
        {
            TryAttack(attack);
        }
    }

    private void TryAttack(AttackRuntimeData attack)
    {
        currentTarget = EnemyManager.Instance.GetClosestEnemy(
            transform.position,
            attack.range
        );

        Shoot(currentTarget, attack.bulletCount, attack.bulletSpreadAngle);
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
            GameObject bullet = (StageManager.Instance != null) ? StageManager.Instance.GetBullet() : null;
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

    private bool showRange;

    private void OnDrawGizmos()
    {
        if (!showRange)
            return;

        if (StageManager.Instance == null ||
            StageManager.Instance.context == null ||
            StageManager.Instance.context.RuntimeAttack == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            StageManager.Instance.context.RuntimeAttack.range
        );
    }
}
