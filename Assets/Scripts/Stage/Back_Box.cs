using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Back_Box : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        // if (StageManager.Instance.DestroyAll) collision.gameObject.SetActive(false);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            // 적을 조용히 제거하면 처치 수가 누락되므로 반드시 사망 처리를 거친다.
            enemy.Die();
            return;
        }
        collision.gameObject.SetActive(false);
    }
}
