using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public GameManager GameManager;
    public static StageManager Instance;

    // 스테이지 내 플레이어 초기 스탯
    public float attack;
    public int attackCount = 1; // 발사 수
    public float attackRange;
    public float attackInterval;
    public float bulletshot = 10f;   // 총알 속도
    public int maxBulletHP = 1;   // 총알 관통력

    // 스테이지 종료 후 외부로 전달될 보상
    public int experience = 0;
    public int gold = 0;
    public int gem = 0;

    public int skillShard = 0;
    public int equipmentShard = 0;

    public int donut = 0;
    public int chocolate = 0;

    // 적 전체 제거용 bool
    public bool DestroyAll = false;

    // 웨이브 수
    public int currentWave = 1;
    public int maxWaves = 20;
    public TextMeshProUGUI waveText;

    [Header("Stage Objects")]
    public GameObject player;
    public GameObject enemyManager;
    public GameObject uiCanvas;

    [Header("적 등장 제한")]
    public int totalEnemiesThisStage; // 스테이지 전체 등장 가능 적 수
    private int spawnedEnemies = 0;         // 지금까지 스폰된 적 수

    [Header("적 생성 설정")]
    public int enemiesPerSpawn = 5;     // 한 번에 스폰할 적 수
    public float spawnInterval = 2f;    // 스폰 요청 간격 (초)
    public float totalSpawnDuration = 180f;    // 적이 전부 스폰하는 데 걸리는 시간

    public Targeting targeting;

    // bullet pooling
    public GameObject bulletPrefab;
    public List<GameObject> bulletPool = new List<GameObject>();

    // 보스 처치 후 참이 되는 bool
    public bool isBossDefeated = false;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        // 스테이지 진입 후 오브젝트 활성화
        if (player != null) player.SetActive(true);
        if (enemyManager != null) enemyManager.SetActive(true);
        if (uiCanvas != null) uiCanvas.SetActive(true);

        Debug.Log("Stage 시작: 모든 오브젝트 활성화됨!");

        // GameManager 확인
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager 없음 → Stage 단독 실행. Home으로 이동합니다.");
            SceneManager.LoadScene("Home");
            return;
        }
        GameManager = GameManager.Instance;

        // GameManager 값으로 스테이지 초기화
        InitializeStage();
    }

    // =========================
    // 스테이지 초기화
    // =========================
    private void InitializeStage()
    {
        // =========================
        // 1. 기본 스탯 불러오기
        // =========================
        attack = GameManager.baseAttack;
        attackCount = 1;
        attackRange = GameManager.baseAttackRange;
        attackInterval = GameManager.baseAttackInterval;
        bulletshot = 10f;

        Debug.Log($"[Stage Init] 공격력 {attack}, 범위 {attackRange}, 공격 간격 {attackInterval}");

        // 해금된 스킬/캐릭터 출력
        foreach (var skill in GameManager.skillUnlocked)
            Debug.Log($"해금된 스킬: {skill.Key} → {skill.Value}");
        foreach (var character in GameManager.specialCharacterUnlocked)
            Debug.Log($"해금된 캐릭터: {character.Key} → {character.Value}");

        // 장비/공격 효과 확인
        Debug.Log("장비 효과: " + string.Join(", ", GameManager.equipmentEffects));
        Debug.Log("공격 효과: " + string.Join(", ", GameManager.attackEffects));

        // =========================
        // 2. 스테이지 상태 초기화
        // =========================
        currentWave = 0;
        experience = 0;
        spawnedEnemies = 0; // 지금까지 스폰된 적 수 리셋

        // =========================
        // 3. 총알 풀 초기화
        // =========================
        bulletPool.Clear();
        AddBullets(3); // 초기 총알 3개 생성

        // =========================
        // 4. 스폰 시스템 시작
        // =========================
        spawnInterval = totalSpawnDuration / (totalEnemiesThisStage / enemiesPerSpawn); // 스테이지 전체 적 수에 맞춰 스폰 간격 조정
        StartNewWave(); // 웨이브 첫 시작
        StartCoroutine(SpawnRequestRoutine()); // 일정 시간마다 스폰 요청
    }

    private IEnumerator SpawnRequestRoutine()
    {
        while (spawnedEnemies < totalEnemiesThisStage && !isBossDefeated)
        {
            SpawnEnemies(enemiesPerSpawn);
            yield return new WaitForSeconds(spawnInterval);
        }

        Debug.Log("[StageManager] 모든 적 스폰 완료");
    }

    // 풀에서 사용 가능한 총알 가져오기
    public GameObject GetBullet()
    {
        foreach (var bullet in bulletPool)
        {
            if (!bullet.activeSelf)
                return bullet;
        }

        // 남은 총알 없으면 새로 생성 후 추가
        GameObject newBullet = Instantiate(bulletPrefab);
        newBullet.SetActive(false);
        bulletPool.Add(newBullet);
        return newBullet;
    }

    // 풀에 총알 추가
    public void AddBullets(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Add(bullet);
        }
    }

    public void SpawnEnemies(int requestedCount)
    {
        if (EnemyManager.Instance == null)
        {
            Debug.LogError("EnemyManager 인스턴스 없음! 스폰 불가");
            return;
        }

        // 남은 적 수 계산
        int remaining = totalEnemiesThisStage - spawnedEnemies;
        if (remaining <= 0)
        {
            Debug.Log("스테이지 전체 적 스폰 완료");
            return;
        }

        // 요청 수와 남은 수 중 작은 값만 스폰
        int spawnCount = Mathf.Min(requestedCount, remaining);
        spawnedEnemies += spawnCount;

        EnemyManager.Instance.SpawnEnemies(spawnCount);
        Debug.Log($"적 {spawnCount}마리 스폰 요청, 현재까지 총 {spawnedEnemies}/{totalEnemiesThisStage}");
    }

    // 새로운 웨이브 함수
    public void StartNewWave()
    {
        currentWave++;
        // 업그레이드 선택 로직 추가 예정

        if (DestroyAll) return;
    }

    // =========================
    // 적 처치
    // =========================
    public void OnEnemyDefeated()
    {
        Debug.Log($"적 처치!");
    }

    // =========================
    // 스테이지 종료
    // =========================
    public void OnStageClear()
    {
        StageClear();
        SceneManager.LoadScene("Home");
    }

    private void StageClear()
    {
        GameManager.Instance.AddExperience(experience);
        GameManager.Instance.AddGold(gold);
        //GameManager.Instance.AddGem(gem);
        GameManager.Instance.AddSkillShard(skillShard);
        GameManager.Instance.AddEquipmentShard(equipmentShard);
        GameManager.Instance.AddDonut(donut);
        GameManager.Instance.AddChocolate(chocolate);

        Debug.Log("스테이지 클리어!");
        Debug.Log($"획득 경험치: {experience}, 골드: {gold}");
        Debug.Log($"획득 스킬조각: {skillShard}, 장비조각: {equipmentShard}");
        Debug.Log($"획득 도넛: {donut}, 초콜릿: {chocolate}");
    }
}
