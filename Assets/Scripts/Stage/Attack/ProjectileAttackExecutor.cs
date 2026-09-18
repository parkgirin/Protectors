using UnityEngine;

public class ProjectileAttackExecutor : IAttackExecutor
{
    public bool Execute(AttackExecutionContext context)
    {
        GameObject target = context.FindTarget();
        if (target == null || StageManager.Instance == null) return false;

        int count = context.attack.bulletCount;
        Vector3 direction = (target.transform.position - context.owner.position).normalized;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        const float step = 8f;
        bool fired = false;

        for (int i = 0; i < count; i++)
        {
            GameObject bullet = StageManager.Instance.GetBullet();
            if (bullet == null) continue;
            // The first shell alone uses the current target. Extra shells fan out
            // alternately left/right from that first shell's direction.
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
        return fired;
    }
}

// A safe placeholder: data can be loaded before the turret/trap prefabs are implemented.
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
