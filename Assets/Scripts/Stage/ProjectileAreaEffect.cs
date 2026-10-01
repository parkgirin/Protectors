using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAreaEffect : MonoBehaviour
{
    private const float FireDuration = 3f;
    private const float TickInterval = 0.5f;
    private float radius;
    private float fireDamage;
    private float elapsed;
    private int appliedTicks;
    private Enemy excludedEnemy;
    private float lifeStealRatio;
    private Action<Enemy, Vector3> onExplosionKill;
    private LineRenderer ring;
    private Material ringMaterial;
    private Color explosionColor = new Color(1f, 0.5f, 0.1f, 0.9f);
    private Color fireColor = new Color(1f, 0.35f, 0.05f, 0.9f);

    public static void Create(Vector3 position, float radius, float damage, float fireDamagePerSecond,
        Enemy excludedEnemy, float lifeStealRatio,
        Action<Enemy, Vector3> onExplosionKill = null, GameColorPalette colorPalette = null)
    {
        if (StageManager.Instance == null) return;
        ProjectileAreaEffect effect = StageManager.Instance.GetAreaEffect();
        effect.Configure(position, radius, fireDamagePerSecond, excludedEnemy, lifeStealRatio,
            onExplosionKill, colorPalette);
        effect.gameObject.SetActive(true);
        effect.DamageEnemies(damage, true);
        // Only the direct blast can create fragments; lingering fire cannot retain the source projectile callback.
        effect.onExplosionKill = null;
    }

    private void Configure(Vector3 position, float effectRadius, float fireDamagePerSecond,
        Enemy excludedEnemy, float lifeStealRatio, Action<Enemy, Vector3> onExplosionKill,
        GameColorPalette colorPalette)
    {
        transform.position = position;
        radius = Mathf.Max(0.01f, effectRadius);
        fireDamage = fireDamagePerSecond;
        this.excludedEnemy = excludedEnemy;
        this.lifeStealRatio = Mathf.Max(0f, lifeStealRatio);
        this.onExplosionKill = onExplosionKill;
        elapsed = 0f;
        appliedTicks = 0;
        explosionColor = new Color(1f, 0.5f, 0.1f, 0.9f);
        fireColor = new Color(1f, 0.35f, 0.05f, 0.9f);
        if (colorPalette != null)
        {
            explosionColor = colorPalette.Get(PaletteColor.StageRedBright);
            fireColor = colorPalette.Get(PaletteColor.StageRed);
        }
        CreateRing();
    }

    private void CreateRing()
    {
        if (ring == null)
        {
            ring = gameObject.AddComponent<LineRenderer>();
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = 40;
            ring.widthMultiplier = 0.035f;
            ring.sortingOrder = 5;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                ringMaterial = new Material(shader);
                ring.sharedMaterial = ringMaterial;
            }
        }
        for (int i = 0; i < ring.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / ring.positionCount;
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }
        ring.startColor = ring.endColor = explosionColor;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (fireDamage > 0f)
        {
            // 프레임 속도와 관계없이 3초 동안 여섯 번만 화염 피해를 적용한다.
            int dueTicks = Mathf.FloorToInt(Mathf.Min(elapsed, FireDuration) / TickInterval);
            while (appliedTicks < dueTicks)
            {
                if (Time.timeScale <= 0f) return;
                appliedTicks++;
                DamageEnemies(fireDamage * TickInterval, false);
            }
        }
        float duration = fireDamage > 0f ? FireDuration : 0.2f;
        Color color = fireDamage > 0f ? fireColor : explosionColor;
        color.a *= Mathf.Clamp01(1f - elapsed / duration);
        ring.startColor = ring.endColor = color;
        if (elapsed >= duration) ReleaseToPool();
    }

    private void DamageEnemies(float damage, bool isDirectExplosion)
    {
        if (damage <= 0f || EnemyManager.Instance == null) return;
        // 사망 처리로 활성 적 목록이 바뀌어도 이번 범위 판정의 대상은 유지한다.
        var targets = new List<Enemy>();
        foreach (GameObject candidate in EnemyManager.Instance.activeEnemies)
        {
            if (candidate == null || !candidate.activeInHierarchy) continue;
            Enemy enemy = candidate.GetComponent<Enemy>();
            if (enemy == null || enemy == excludedEnemy || enemy.hp <= 0f || targets.Contains(enemy)) continue;
            Collider2D collider = candidate.GetComponent<Collider2D>();
            Vector3 closest = collider != null ? (Vector3)collider.ClosestPoint(transform.position) : candidate.transform.position;
            if ((closest - transform.position).sqrMagnitude <= radius * radius) targets.Add(enemy);
        }
        float appliedDamage = 0f;
        int defeatedEnemies = 0;
        foreach (Enemy enemy in targets)
            if (enemy != null && enemy.gameObject.activeInHierarchy && enemy.hp > 0f)
            {
                Vector3 defeatedPosition = enemy.transform.position;
                float dealtDamage = enemy.TakeDamage(damage);
                appliedDamage += dealtDamage;
                if (dealtDamage > 0f && enemy.hp <= 0f)
                {
                    defeatedEnemies++;
                    if (isDirectExplosion) onExplosionKill?.Invoke(enemy, defeatedPosition);
                }
            }
        if (appliedDamage > 0f && defeatedEnemies > 0 && lifeStealRatio > 0f && StageManager.Instance != null)
            for (int i = 0; i < defeatedEnemies; i++)
                StageManager.Instance.HealWallFromMissingHealth(lifeStealRatio);
    }

    private void OnDestroy()
    {
        if (ringMaterial != null) Destroy(ringMaterial);
    }

    private void ReleaseToPool()
    {
        onExplosionKill = null;
        excludedEnemy = null;
        if (StageManager.Instance != null) StageManager.Instance.ReleaseAreaEffect(this);
        else gameObject.SetActive(false);
    }
}
