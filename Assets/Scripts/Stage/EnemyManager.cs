using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public List<GameObject> enemyPrefab = new List<GameObject>(); // 스폰할 적 프리팹
    public int maxEnemies = 150;   // 전체 풀 수
    public int maxActiveEnemies = 100; // 화면 내 최대 활성 적 수

    public List<GameObject> activeEnemies = new List<GameObject>();
    public Queue<GameObject> pendingEnemies = new Queue<GameObject>(); // 대기중인 적

    // 풀링용 리스트
    public List<GameObject> enemyPool = new List<GameObject>();
    private readonly HashSet<GameObject> queuedEnemies = new HashSet<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[EnemyManager] Instance 등록 완료");
        }

        InitializePool();
    }

    private void Update()
    {
        FillActiveSlots();
    }

    // Pending enemies are already part of the stage total. Move them onto the
    // screen only when the Inspector-authored active-enemy limit allows it.
    private void FillActiveSlots()
    {
        while (pendingEnemies.Count > 0 && activeEnemies.Count < maxActiveEnemies)
        {
            //Debug.Log($"[OnEnemyDestroyed] 대기 적 활성화, 남은 적 {pendingEnemies.Count - 1}");
            GameObject next = pendingEnemies.Dequeue();
            queuedEnemies.Remove(next);
            ActivateEnemy(next);
        }
    }

    void InitializePool()
    {
        if (enemyPrefab == null || enemyPrefab.Count == 0)
        {
            Debug.LogError("[EnemyManager] enemyPrefab is not assigned.");
            return;
        }
        for (int i = 0; i < maxEnemies; i++)
        {
            // EnemyManager의 자식으로 생성
            if(enemyPrefab.Count > 1)
            {
                int randomIndex = Random.Range(0, enemyPrefab.Count);
                GameObject enemy = Instantiate(enemyPrefab[randomIndex], this.transform);
                enemy.SetActive(false);
                enemyPool.Add(enemy);

            }
            else
            {
                GameObject enemy = Instantiate(enemyPrefab[0], this.transform);
                enemy.SetActive(false);
                enemyPool.Add(enemy);
            }
        }
    }

    // --------------------------------
    // SpawnEnemies: 풀에서 적 가져와 활성화 / 대기열 관리
    // --------------------------------
    public int SpawnEnemies(int count)
    {
        int spawned = 0;
        for (int i = 0; i < count; i++)
        {
            GameObject enemy = GetNextEnemyFromPool();
            if (enemy == null)
            {
                Debug.LogWarning("[SpawnEnemies] 풀에 남은 Enemy 없음!");
                break;
            }

            if (activeEnemies.Count < maxActiveEnemies)
            {
                ActivateEnemy(enemy);
            }
            else
            {
                pendingEnemies.Enqueue(enemy);
                queuedEnemies.Add(enemy);
            }
            spawned++;
        }
        return spawned;
    }

    public int PendingEnemyCount => pendingEnemies.Count;

    public void ActivateEnemy(GameObject enemy)
    {
        //Debug.Log("[ActivateEnemy] 적 활성화");
        enemy.SetActive(true);
        enemy.transform.parent = this.transform; // 안전하게 부모 지정
        RegisterEnemy(enemy);
    }

    GameObject GetNextEnemyFromPool()
    {
        foreach (var enemy in enemyPool)
        {
            if (!enemy.activeSelf && !queuedEnemies.Contains(enemy))
                return enemy;
        }
        Debug.LogWarning("[GetNextEnemyFromPool] 풀에 남은 Enemy 없음!");
        return null;
    }

    public void RegisterEnemy(GameObject enemy)
    {
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);
    }

    // --------------------------------
    // 적 제거 시 대기열 처리
    // --------------------------------
    public void OnEnemyDestroyed(GameObject enemy)
    {
        UnregisterEnemy(enemy);
        queuedEnemies.Remove(enemy);

        // 죽은 적 비활성화
        enemy.SetActive(false);
        FillActiveSlots();
    }

    public GameObject GetClosestEnemy(Vector3 fromPosition, float maxTargetDistance)
    {
        GameObject closestEnemy = null;
        float closestDistanceSqr = Mathf.Infinity;

        // 활성화된 적들만 검사
        foreach (var enemy in activeEnemies)
        {
            if (enemy == null || !enemy.activeSelf) continue;

            float sqrDistance = (enemy.transform.position - fromPosition).sqrMagnitude;

            // 거리 제한 적용
            if (sqrDistance > maxTargetDistance * maxTargetDistance)
                continue;

            if (sqrDistance < closestDistanceSqr)
            {
                closestDistanceSqr = sqrDistance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy; // 없으면 null 반환
    }
}
