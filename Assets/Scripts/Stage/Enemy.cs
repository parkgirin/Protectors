using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int HP_Set = 10;
    public float hp;
    public TextMeshPro TMPro_hp;
    public float speed = 1f;
    public bool attacking = false;
    public float attackDelay = 1f;

    private float baseSpeed;
    private bool isDying;
    private float spawnHealthMultiplier = 1f;
    private float attackDamage = 1f;

    private StageManager stageManager;
    public GameObject dieAnime;

    private void OnEnable()
    {
        // 가능한 한 미리 찾아둠 (Inspector 대신 코드로 연결)
        stageManager = FindObjectOfType<StageManager>();
        if (stageManager == null)
            Debug.LogWarning("StageManager를 찾을 수 없습니다. 스테이지 관련 기능이 동작하지 않을 수 있습니다.");
        hp = Mathf.Max(1f, HP_Set * spawnHealthMultiplier);
        isDying = false;
        if (TMPro_hp != null) TMPro_hp.text = hp.ToString("0.#");

        // 초기 위치 설정
        float x = Random.Range(-2.3f, 2.3f);
        this.transform.position = new Vector3(x, 4f, 0);

        if (baseSpeed <= 0f) baseSpeed = speed;
        speed = baseSpeed * Random.Range(0.8f, 1.2f);
        attacking = false;
        attackDelay = 1f;
    }

    void Update()
    {
        if (hp <= 0)
        {
            DieAnime();
        }
        else if (!attacking)
        {
            transform.Translate(Vector2.down * speed * Time.deltaTime);
        }
        else
        {
            // 시간 지연 후 공격 실행
            attackDelay -= Time.deltaTime;
            if (attackDelay <= 0f)
            {
                Attack();
                attackDelay = 1f; // 공격 후 딜레이 초기화
            }
        }
    }

    public void Die()
    {
        if (isDying) return;
        isDying = true;
        AudioManager.Instance?.PlaySfx(GameSfx.EnemyDefeated);
        if (StageManager.Instance != null)
            StageManager.Instance.OnEnemyDefeated();

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(gameObject);
            EnemyManager.Instance.OnEnemyDestroyed(gameObject);
        }
    }
    public void DieAnime(bool criticalHit = false)
    {
        if (isDying) return;
        if (dieAnime != null)
        {
            GameObject anime = Instantiate(dieAnime);
            anime.transform.position = this.transform.position;
            // Carry the selected enemy color into its animated defeat sprite.
            PaletteColorApplicator sourceColor = GetComponentInChildren<PaletteColorApplicator>();
            PaletteColorApplicator effectColor = anime.GetComponentInChildren<PaletteColorApplicator>();
            if (sourceColor != null && effectColor != null)
            {
                effectColor.ColorRole = sourceColor.ColorRole;
                effectColor.Palette = sourceColor.Palette;
            }
            if (criticalHit) CriticalImpactEffect.Play(anime);
        }
        else
        {
            Debug.LogWarning("Die Anime Prefab is not assigned.");
        }
        Die();
    }

    public float TakeDamage(float damage, bool criticalHit = false)
    {
        // 풀링된 적이 이미 사망한 뒤 여러 탄환의 충돌이 들어와도 피해를 중복 처리하지 않는다.
        if (isDying || !gameObject.activeInHierarchy || damage <= 0f) return 0f;
        float appliedDamage = Mathf.Min(Mathf.Max(0f, hp), damage);
        hp = Mathf.Max(0f, hp - damage);
        if (TMPro_hp != null) TMPro_hp.text = hp.ToString("0.#");

        if (hp <= 0)
        {
            DieAnime(criticalHit);
        }
        else if (criticalHit)
        {
            CriticalImpactEffect.Play(gameObject);
        }
        return appliedDamage;
    }

    public void Attack()
    {
        StageManager manager = StageManager.Instance != null ? StageManager.Instance : stageManager;
        if (manager != null) manager.OnWallHit(attackDamage);
    }

    public void ConfigureForWave(float healthMultiplier, float attackMultiplier)
    {
        spawnHealthMultiplier = Mathf.Max(1f, healthMultiplier);
        attackDamage = Mathf.Max(1f, attackMultiplier);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            Debug.Log("Enemy collided with Wall");
            attacking = true;
        }
    }
}
