using UnityEngine;

public class Targeting : MonoBehaviour
{
    public float checkInterval = 0.5f;
    private float timer = 0f;

    public GameObject CurrentTarget;

    public GameObject targetMarker;
    public GameObject targetPrefab;

    // 왕복 이동용 변수
    public float moveRange = 2f;   // 왕복 범위
    public float moveSpeed = 2f;   // 이동 속도
    private Vector3 startPos;

    private void OnEnable()
    {
        if (targetPrefab != null)
        {
            targetMarker = Instantiate(targetPrefab);
            targetMarker.SetActive(false);
        }

        startPos = new Vector3(0, 0, 0);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            UpdateTarget();
            timer = 0f;
        }

        // 타겟이 없다면 임시 왕복 이동 함수
        if (CurrentTarget == null) Move();
    }

    void Move()
    {
        // 왕복 이동
        if (targetMarker != null && targetMarker.activeSelf)
        {
            float x = Mathf.PingPong(Time.time * moveSpeed, moveRange * 2) - moveRange;
            targetMarker.transform.position = startPos + new Vector3(x, 0f, 0f);
        }
    }
    void UpdateTarget()
    {
        if (EnemyManager.Instance == null || StageManager.Instance == null ||
            StageManager.Instance.context == null || StageManager.Instance.context.RuntimeAttack == null)
        {
            CurrentTarget = null;
            if (targetMarker != null) targetMarker.SetActive(false);
            return;
        }

        CurrentTarget = EnemyManager.Instance.GetClosestEnemy(
            transform.position,
            StageManager.Instance.context.RuntimeAttack.EffectiveRange);

        // 실제 타겟팅 로직 대신 임시로 마커 보이게만 처리
        if (targetMarker == null) return;
        if (CurrentTarget != null)
        {
            targetMarker.SetActive(true);
            targetMarker.transform.position = CurrentTarget.transform.position;
        }
        else
        {
            // 임시 테스트용으로 타겟이 없어도 활성화
            targetMarker.SetActive(true);
        }
    }
}
