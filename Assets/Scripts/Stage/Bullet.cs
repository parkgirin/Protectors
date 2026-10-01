// 탄환 동작과 풀 재사용 상태를 관리한다.
using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    private float speed;
    private int bullet_HP;        // 현재 내구도
    private AttackRuntimeData attackData;
    private SpriteRenderer spriteRenderer;
    private Sprite defaultSprite;
    private Vector3 defaultLocalScale;
    private float damage;
    private float spriteScale;
    private float fragmentBaseScale;
    private float fragmentRange;
    private Sprite attackSpriteSnapshot;
    private bool createsFragments;
    private bool aimsFragments;
    private bool initialized;
    private int fragmentCount;
    private int fragmentCountVariance;
    private int fragmentPierce;
    private float fragmentDamageRatio;
    private float fragmentChance;
    private float fragmentExplosionChance;
    private bool explodes;
    private bool fragmentsExplode;
    private bool createsFragmentsOnExplosionKill;
    private bool leavesFire;
    private float explosionDamage;
    private float explosionRadius;
    private float fireDamagePerSecond;
    private float lifeStealRatio;
    private float criticalChance;
    private float criticalDamageMultiplier;
    private float criticalSpeedMultiplier;
    private float explosionDamageRatio;
    private bool criticalResolved;
    private bool criticalHit;
    private float lifetime;
    private Enemy lastHitEnemy;
    private Vector3 lastHitPosition;
    private float baseRadius = 0.075f;
    private Enemy explosionExcludedEnemy;
    private readonly HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) defaultSprite = spriteRenderer.sprite;
        defaultLocalScale = transform.localScale;
        Collider2D hitbox = GetComponent<Collider2D>();
        if (hitbox is CapsuleCollider2D capsule)
            baseRadius = Mathf.Max(capsule.size.x * Mathf.Abs(defaultLocalScale.x), capsule.size.y * Mathf.Abs(defaultLocalScale.y)) * 0.5f;
        else if (hitbox is CircleCollider2D circle)
            baseRadius = circle.radius * Mathf.Max(Mathf.Abs(defaultLocalScale.x), Mathf.Abs(defaultLocalScale.y));
        else if (hitbox is BoxCollider2D box)
            baseRadius = Mathf.Max(box.size.x * Mathf.Abs(defaultLocalScale.x), box.size.y * Mathf.Abs(defaultLocalScale.y)) * 0.5f;
    }

    public void Initialize(AttackRuntimeData attack)
    {
        // 풀링된 탄환은 SetActive로 OnEnable이 호출되기 전에 자신의 공격 데이터를 받아야 한다.
        attackData = attack;
        speed = attack.EffectiveBulletSpeed;
        bullet_HP = attack.ProjectilePierce;
        // 비행 중 강화와 원본 SO 변경이 이미 발사된 탄환에 영향을 주지 않도록 복사한다.
        // 치명타는 첫 적중 시 스냅샷 수치와 활공 시간을 사용해 판정한다.
        damage = attack.ProjectileDamage;
        criticalChance = attack.EffectiveCriticalChance;
        criticalDamageMultiplier = attack.EffectiveCriticalDamageMultiplier;
        criticalSpeedMultiplier = Mathf.Max(0.1f, attack.bulletSpeedMultiplier);
        criticalResolved = false;
        criticalHit = false;
        spriteScale = attack.ProjectileScale;
        fragmentBaseScale = Mathf.Max(0.01f, attack.attackSpriteScale);
        fragmentRange = attack.EffectiveRange;
        attackSpriteSnapshot = attack.source != null ? attack.source.attackSprite : null;
        createsFragments = attack.HasBehavior(AttackBehavior.FragmentOnHit);
        aimsFragments = attack.HasBehavior(AttackBehavior.HomingFragment);
        fragmentCount = Mathf.Max(1, attack.fragmentCount);
        fragmentCountVariance = Mathf.Max(0, attack.fragmentCountVariance);
        fragmentPierce = Mathf.Max(1, attack.fragmentPierce);
        fragmentDamageRatio = Mathf.Max(0.01f, attack.fragmentDamageRatio);
        fragmentChance = Mathf.Clamp(attack.fragmentChance, 0f, AttackRuntimeData.MaxFragmentChance);
        fragmentExplosionChance = Mathf.Clamp(attack.fragmentExplosionChance, 0f, AttackRuntimeData.MaxFragmentExplosionChance);
        explodes = attack.HasBehavior(AttackBehavior.ExplodeOnHit);
        fragmentsExplode = attack.HasBehavior(AttackBehavior.FragmentExplosion) || attack.HasBehavior(AttackBehavior.ChainExplosionProjectile);
        createsFragmentsOnExplosionKill = attack.HasBehavior(AttackBehavior.ChainExplosionProjectile);
        leavesFire = attack.HasBehavior(AttackBehavior.LingeringFire);
        explosionDamageRatio = Mathf.Max(0f, attack.explosionDamageRatio);
        explosionDamage = damage * explosionDamageRatio;
        // 합체·대형화의 실제 투사체 크기가 폭발 범위 보상으로 이어지되, 기본 반경은
        // 한 단계 낮춰 직접 피격보다 범위 피해가 앞서지 않게 한다.
        explosionRadius = baseRadius * 1.75f * attack.ExplosionScaleMultiplier * Mathf.Max(0.1f, attack.explosionRadiusMultiplier);
        fireDamagePerSecond = damage * 0.2f;
        lifeStealRatio = Mathf.Max(0f, attack.lifeStealRatio);
        explosionExcludedEnemy = null;
        lastHitEnemy = null;
        lastHitPosition = Vector3.zero;
        lifetime = 10f;
        hitEnemies.Clear();
        initialized = true;
        ApplyAttackSprite();
    }

    private void ApplyAttackSprite()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        Sprite attackSprite = attackSpriteSnapshot;
        spriteRenderer.sprite = attackSprite != null ? attackSprite : defaultSprite;
        float scale = spriteScale;
        // 풀 재사용 시 크기가 누적되지 않도록 항상 프리팹의 기본 크기를 기준으로 계산한다.
        transform.localScale = defaultLocalScale * scale;
    }

    private void OnEnable()
    {
        if (initialized) return;
        // 총알 이동 속도 세팅
        if (attackData == null && StageManager.Instance != null && StageManager.Instance.context != null)
            attackData = StageManager.Instance.context.RuntimeAttack;
        if (attackData != null) Initialize(attackData);
        else { speed = 5f; bullet_HP = 1; damage = 0f; spriteScale = 1f; ApplyAttackSprite(); }
    }

    private void OnDisable()
    {
        initialized = false;
        attackData = null;
        createsFragments = false;
        fragmentChance = 0f;
        fragmentCountVariance = 0;
        fragmentExplosionChance = 0f;
        fragmentBaseScale = 1f;
        aimsFragments = false;
        explodes = false;
        fragmentsExplode = false;
        createsFragmentsOnExplosionKill = false;
        leavesFire = false;
        lifeStealRatio = 0f;
        criticalChance = 0f;
        criticalDamageMultiplier = 1f;
        criticalSpeedMultiplier = 1f;
        explosionDamageRatio = 0f;
        criticalResolved = false;
        criticalHit = false;
        explosionExcludedEnemy = null;
        lastHitEnemy = null;
        lastHitPosition = Vector3.zero;
        hitEnemies.Clear();
    }

    private void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            ReleaseFragmentsAtLastHit();
            gameObject.SetActive(false);
            return;
        }
        // 내구도 다 쓰면 비활성화
        if (bullet_HP <= 0)
        {
            gameObject.SetActive(false);
        }
        else transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("enemy"))
        {
            // 내구도 다 쓰면 비활성화
            if (bullet_HP <= 0)
            {
                gameObject.SetActive(false);
            }
            else
            {
                // 적 체력 깎기
                Enemy enemy = collision.GetComponentInParent<Enemy>();
                if (enemy == null || enemy.hp <= 0f || !hitEnemies.Add(enemy)) return;
                ResolveCriticalDamage();
                Vector3 hitPosition = collision.ClosestPoint(transform.position);
                float appliedDamage = enemy.TakeDamage(damage, criticalHit);
                AudioManager.Instance?.PlaySfx(GameSfx.ProjectileHit);
                ApplyLifeStealOnKill(appliedDamage, enemy.hp <= 0f);

                // 총알 내구도 감소
                bullet_HP--;
                // 파편은 관통이 끝난 마지막 유효 적중의 위치에서 한 번만 생성한다.
                lastHitEnemy = enemy;
                lastHitPosition = hitPosition;
                if (explodes)
                {
                    // 첫 적중에서만 폭발하며 범위 피해는 투사체 적중 행동을 재호출하지 않는다.
                    explodes = false;
                    ProjectileAreaEffect.Create(hitPosition, explosionRadius, explosionDamage,
                        leavesFire ? fireDamagePerSecond : 0f, explosionExcludedEnemy, lifeStealRatio,
                        createsFragmentsOnExplosionKill ? SpawnExplosionKillFragments : null,
                        StageManager.Instance != null ? StageManager.Instance.colorPalette : null);
                }
                if (bullet_HP <= 0)
                {
                    ReleaseFragmentsAtLastHit();
                    gameObject.SetActive(false);
                }
            }
        }
    }

    private void SpawnFragments(Enemy hitEnemy, Vector3 hitPosition)
    {
        // 무작위화 강화는 고정 발수 보정이 아니라 현재 파편 수의 ±50% 범위를 사용한다.
        // 반올림 뒤에도 최소 한 발은 보장한다.
        float countMultiplier = fragmentCountVariance > 0 ? Random.Range(0.5f, 1.5f) : 1f;
        int spawnCount = Mathf.Max(1, Mathf.RoundToInt(fragmentCount * countMultiplier));
        SpawnFragments(hitEnemy, hitPosition, spawnCount, true);
    }

    private void ReleaseFragmentsAtLastHit()
    {
        if (!createsFragments || lastHitEnemy == null) return;
        createsFragments = false;
        if (Random.value < fragmentChance) SpawnFragments(lastHitEnemy, lastHitPosition);
    }

    private void SpawnExplosionKillFragments(Enemy defeatedEnemy, Vector3 defeatedPosition)
    {
        SpawnFragments(defeatedEnemy, defeatedPosition, Random.Range(1, 4), false);
    }

    private void SpawnFragments(Enemy hitEnemy, Vector3 hitPosition, int spawnCount, bool allowExplosionChain)
    {
        if (StageManager.Instance == null) return;
        var targets = new List<Enemy>();
        if (aimsFragments && EnemyManager.Instance != null)
            foreach (GameObject candidate in EnemyManager.Instance.activeEnemies)
            {
                if (candidate == null || !candidate.activeInHierarchy) continue;
                Enemy enemy = candidate.GetComponent<Enemy>();
                if (enemy == null || enemy == hitEnemy || enemy.hp <= 0f) continue;
                if ((candidate.transform.position - hitPosition).sqrMagnitude <= fragmentRange * fragmentRange)
                    targets.Add(enemy);
            }
        targets.Sort((a, b) => (a.transform.position - hitPosition).sqrMagnitude.CompareTo((b.transform.position - hitPosition).sqrMagnitude));

        // 주 탄환을 활성 상태로 유지한 채 파편을 할당해 같은 풀 객체의 재사용을 막는다.
        for (int i = 0; i < Mathf.Max(1, spawnCount); i++)
        {
            GameObject fragment = StageManager.Instance.GetBullet();
            if (fragment == null) continue;
            Bullet component = fragment.GetComponent<Bullet>();
            if (component == null) continue;
            // 기본 파편은 전 방향으로 확산하고 조준 강화가 있을 때만 적을 향한다.
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 direction = targets.Count > 0
                ? targets[i % targets.Count].transform.position - hitPosition
                : new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0f);
            if (direction.sqrMagnitude < 0.0001f) direction = transform.up;
            fragment.transform.position = hitPosition;
            fragment.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
            component.InitializeFragment(this, hitEnemy, allowExplosionChain);
            fragment.SetActive(true);
        }
    }

    private void InitializeFragment(Bullet parent, Enemy excludedEnemy, bool allowExplosionChain)
    {
        // 파편은 주 탄환의 스냅샷에서 생성하며 재분열과 최초 피격 대상의 재타격을 금지한다.
        attackData = null;
        speed = parent.speed;
        damage = parent.damage * parent.fragmentDamageRatio;
        bullet_HP = parent.fragmentPierce;
        // 파편 크기는 합체·대형화 이전의 원본 포탄 크기를 기준으로 고정한다.
        spriteScale = parent.fragmentBaseScale * 0.5f;
        fragmentBaseScale = parent.fragmentBaseScale;
        attackSpriteSnapshot = parent.attackSpriteSnapshot;
        fragmentRange = parent.fragmentRange;
        createsFragments = false;
        aimsFragments = false;
        fragmentCount = 0;
        fragmentCountVariance = 0;
        fragmentPierce = 1;
        fragmentDamageRatio = 0f;
        fragmentChance = 0f;
        fragmentExplosionChance = 0f;
        explodes = allowExplosionChain && parent.fragmentsExplode && Random.value < parent.fragmentExplosionChance;
        fragmentsExplode = false;
        createsFragmentsOnExplosionKill = allowExplosionChain && parent.createsFragmentsOnExplosionKill;
        leavesFire = false;
        explosionDamage = parent.explosionDamage * parent.fragmentDamageRatio;
        explosionRadius = parent.explosionRadius * 0.5f;
        fireDamagePerSecond = 0f;
        lifeStealRatio = parent.lifeStealRatio;
        criticalChance = 0f;
        criticalDamageMultiplier = 1f;
        criticalSpeedMultiplier = 1f;
        explosionDamageRatio = parent.explosionDamageRatio;
        criticalResolved = true;
        criticalHit = false;
        explosionExcludedEnemy = excludedEnemy;
        lifetime = 10f;
        hitEnemies.Clear();
        hitEnemies.Add(excludedEnemy);
        initialized = true;
        ApplyAttackSprite();
    }

    private void ResolveCriticalDamage()
    {
        if (criticalResolved) return;
        criticalResolved = true;
        float flightTime = Mathf.Max(0f, 10f - lifetime);
        float chance = Mathf.Clamp(criticalChance * criticalSpeedMultiplier, 0f, AttackRuntimeData.MaxCriticalChance);
        float multiplier = Mathf.Clamp(criticalDamageMultiplier * (1f + flightTime * 0.1f), 1f, 4f);
        criticalHit = Random.value < chance;
        if (criticalHit) damage *= multiplier;
        // 폭발과 화염도 첫 적중에서 확정된 동일한 치명타 피해를 기준으로 한다.
        explosionDamage = damage * explosionDamageRatio;
        fireDamagePerSecond = damage * 0.2f;
    }

    private void ApplyLifeStealOnKill(float appliedDamage, bool killedEnemy)
    {
        if (appliedDamage <= 0f || !killedEnemy || lifeStealRatio <= 0f || StageManager.Instance == null) return;
        StageManager.Instance.HealWallFromMissingHealth(lifeStealRatio);
    }
}
