// Bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float speed;
    private int bullet_HP;        // 현재 내구도

    private void OnEnable()
    {
        // 총알 이동 속도 세팅
        speed = StageManager.Instance != null ? StageManager.Instance.context.bulletshot : 5f;

        // 매번 풀에서 꺼낼 때 내구도 초기화
        bullet_HP = StageManager.Instance != null ? StageManager.Instance.context.maxBulletHP : 1;
    }

    private void Update()
    {
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
                Enemy enemy = collision.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(StageManager.Instance.context.attack);
                }

                // 총알 내구도 감소
                bullet_HP--;
            }
        }
    }
}
