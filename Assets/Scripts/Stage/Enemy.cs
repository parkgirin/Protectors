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

    private void OnEnable()
    {
        hp = HP_Set;
        TMPro_hp.text = hp.ToString();

        // 초기 위치 설정
        float x = Random.Range(-2.5f, 2.5f);
        this.transform.position = new Vector3(x, 5, 0);

        float randomFactor = Random.Range(0.8f, 1.2f);
        speed *= randomFactor;
    }

    void Update()
    {
        if (hp <= 0)
        {
            Die();
        }
        else
        {
            transform.Translate(Vector2.down * speed * Time.deltaTime);
        }
    }

    private void Die()
    {
        if (StageManager.Instance != null)
            StageManager.Instance.OnEnemyDefeated();

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(gameObject);
            EnemyManager.Instance.OnEnemyDestroyed(gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        TMPro_hp.text = hp.ToString();

        if (hp <= 0)
        {
            Die();
        }
    }
}
