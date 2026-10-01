using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 이전 씬도 새 공격 구성 컨트롤러를 사용하도록 보장하는 호환용 컴포넌트다.
public class Player_Shot : MonoBehaviour
{
    public float attackTimer = 0f;           // 발사 주기 체크
    public float targetCheckTimer = 0f;      // 타겟 추적 주기 체크
    public GameObject currentTarget;         // 최근에 찾은 적 (Enemy GameObject)
    private void Awake()
    {
        if (GetComponent<AttackController>() == null)
            gameObject.AddComponent<AttackController>();
    }

    private bool showRange;

    private void OnDrawGizmos()
    {
        if (!showRange)
            return;

        if (StageManager.Instance == null ||
            StageManager.Instance.context == null ||
            StageManager.Instance.context.RuntimeAttack == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            StageManager.Instance.context.RuntimeAttack.range
        );
    }
}
