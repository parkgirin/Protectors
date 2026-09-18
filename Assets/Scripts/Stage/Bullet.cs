// Bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float speed;
    private int bullet_HP;        // 현재 내구도
    private AttackRuntimeData attackData;
    private SpriteRenderer spriteRenderer;
    private Sprite defaultSprite;
    private Vector3 defaultLocalScale;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) defaultSprite = spriteRenderer.sprite;
        defaultLocalScale = transform.localScale;
    }

    public void Initialize(AttackRuntimeData attack)
    {
        attackData = attack;
        speed = attack.EffectiveBulletSpeed;
        bullet_HP = attack.bulletHP;
        ApplyAttackSprite();
    }

    private void ApplyAttackSprite()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        Sprite attackSprite = attackData != null && attackData.source != null
            ? attackData.source.attackSprite
            : null;
        spriteRenderer.sprite = attackSprite != null ? attackSprite : defaultSprite;
        float scale = attackData != null ? attackData.EffectiveAttackSpriteScale : 1f;
        transform.localScale = defaultLocalScale * scale;
    }

    private void OnEnable()
    {
        // 총알 이동 속도 세팅
        if (attackData == null && StageManager.Instance != null)
            attackData = StageManager.Instance.context.RuntimeAttack;
        if (attackData != null) Initialize(attackData);
        else { speed = 5f; bullet_HP = 1; }
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
                    if (attackData != null) enemy.TakeDamage(attackData.EffectiveDamage);
                }

                // 총알 내구도 감소
                bullet_HP--;
            }
        }
    }
}
