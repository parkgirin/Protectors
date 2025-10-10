using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;

    private float attackTimer = 0f;   // 발사 주기 체크
    private float targetCheckTimer = 0f; // 타겟 추적 주기 체크
    private GameObject currentTarget;

    public float targetCheckInterval = 0.5f; // 고정된 추적 간격 (0.5초마다 타겟 갱신)

    void Update()
    {
        attackTimer += Time.deltaTime;
        targetCheckTimer += Time.deltaTime;

        // 일정 간격마다 타겟 갱신
        if (targetCheckTimer >= targetCheckInterval)
        {
            targetCheckTimer = 0f;
            currentTarget = EnemyManager.Instance.GetClosestEnemy(transform.position, StageManager.Instance.attackRange);
        }

        // 공격 간격은 StageManager 값에 따라 발사
        if (attackTimer >= StageManager.Instance.attackInterval)
        {
            attackTimer = 0f;
            TryAttack();
        }
    }

    void TryAttack()
    {
        if (EnemyManager.Instance == null) return;

        if (currentTarget == null)
            currentTarget = EnemyManager.Instance.GetClosestEnemy(transform.position, StageManager.Instance.attackRange);

        if (currentTarget != null)
            Shoot(currentTarget);
    }


    void Shoot(GameObject target)
    {
        // 방향 계산
        Vector3 dir = (target.transform.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 총알 생성 + 회전 맞추기
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle - 90));
    }
}
