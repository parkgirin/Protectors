using System.Collections;
using System.Collections.Generic;
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

    private StageManager stageManager;
    public GameObject dieAnime;

    private void OnEnable()
    {
        // 가능한 한 미리 찾아둠 (Inspector 대신 코드로 연결)
        stageManager = FindObjectOfType<StageManager>();
        if (stageManager == null)
            Debug.LogWarning("StageManager를 찾을 수 없습니다. 스테이지 관련 기능이 동작하지 않을 수 있습니다.");
        hp = HP_Set;
        isDying = false;
        if (TMPro_hp != null) TMPro_hp.text = hp.ToString();

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
        if (StageManager.Instance != null)
            StageManager.Instance.OnEnemyDefeated();

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(gameObject);
            EnemyManager.Instance.OnEnemyDestroyed(gameObject);
        }
    }
    public void DieAnime()
    {
        if (dieAnime != null)
        {
            GameObject anime = Instantiate(dieAnime);
            anime.transform.position = this.transform.position;
        }
        else
        {
            Debug.LogWarning("Die Anime Prefab is not assigned.");
        }
        Die();
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        if (TMPro_hp != null) TMPro_hp.text = hp.ToString();

        if (hp <= 0)
        {
            DieAnime();
        }
    }

    public void Attack()
    {
        StageManager manager = StageManager.Instance != null ? StageManager.Instance : stageManager;
        if (manager != null) manager.OnWallHit(1f);
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
