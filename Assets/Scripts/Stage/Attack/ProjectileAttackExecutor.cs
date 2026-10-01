using UnityEngine;

public class ProjectileAttackExecutor : IAttackExecutor
{
    public bool Execute(AttackExecutionContext context)
    {
        GameObject target = context.FindTarget();
        if (target == null || StageManager.Instance == null) return false;

        int count = context.attack.EffectiveBulletCount;
        Vector3 direction = (target.transform.position - context.owner.position).normalized;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float step = Mathf.Clamp(context.attack.bulletSpreadAngle, 1f, 45f);
        bool fired = false;

        for (int i = 0; i < count; i++)
        {
            GameObject bullet = StageManager.Instance.GetBullet();
            if (bullet == null) continue;
            // 첫 탄환만 현재 표적을 향하고 추가 탄환은 첫 탄환의 방향을 기준으로
            // 좌우에 번갈아 퍼지도록 발사한다.
            float angle = baseAngle;
            if (i > 0)
            {
                int distanceFromCenter = (i + 1) / 2;
                float side = i % 2 == 1 ? -1f : 1f;
                angle += side * distanceFromCenter * step;
            }
            bullet.transform.position = context.owner.position;
            bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
            Bullet component = bullet.GetComponent<Bullet>();
            if (component == null) continue;
            component.Initialize(context.attack);
            bullet.SetActive(true);
            fired = true;
        }
        if (fired) AudioManager.Instance?.PlaySfx(GameSfx.AttackFire);
        return fired;
    }
}

// 포탑과 함정 프리팹 구현 전에도 데이터를 안전하게 불러오기 위한 임시 실행기다.
public class SummonAttackExecutor : IAttackExecutor
{
    public bool Execute(AttackExecutionContext context) { return false; }
}

public class DisabledAttackExecutor : IAttackExecutor
{
    private readonly AttackType type;
    public DisabledAttackExecutor(AttackType type) { this.type = type; }
    public bool Execute(AttackExecutionContext context) { return false; }
}
