using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public GameManager GameManager;

    [Header("References")]
    public UIManager uiManager;
    public GameObject player;

    [Header("Controllers")]
    public StageContext context;
    //public StageSpawnController spawnController;
    //public StageProgressController progressController;
    //public StageRewardController rewardController;

    
    public float WallHP = 100f; // 벽 체력

    // 스테이지 종료 후 외부로 전달될 보상
    public int experience = 0;
    public int maxExperience = 0;
    public int gold = 0;
    public int maxgold = 0;
    public int gem = 0;
    public int maxgem = 0;

    public int skillShard = 0;
    public int maxskillShard = 0;
    public int equipmentShard = 0;
    public int maxequipmentShard = 0;

    public int donut = 0;
    public int maxdonut = 0;
    public int chocolate = 0;
    public int maxchocolate = 0;

    // 적 전체 제거용 bool
    public bool DestroyAll = false;

    // 웨이브 수
    public int currentWave = 1;
    public int maxWaves = 20;
    private int lastWave = 0;

    [Header("Stage Objects")]
    //public GameObject player;
    public GameObject enemyManager;
    public GameObject uiCanvas;

    [Header("적 등장 제한")]
    public int totalEnemiesThisStage; // 스테이지 전체 등장 가능 적 수
    private int spawnedEnemies = 0;         // 지금까지 스폰된 적 수

    [Header("적 생성 설정")]
    public int enemiesPerSpawn = 5;     // 한 번에 스폰할 적 수
    public float spawnInterval = 2f;    // 스폰 요청 간격 (초)
    public float totalSpawnDuration = 180f;    // 적이 전부 스폰하는 데 걸리는 시간

    // 처치한 적 수
    public int enemiesDefeated = 0;
    public float WaveExperience = 0;

    public Targeting targeting;

    // bullet pooling
    public GameObject bulletPrefab;
    public List<GameObject> bulletPool = new List<GameObject>();

    // 보스 처치 후 참이 되는 bool
    public bool isBossDefeated = false;

    public bool GameOvered = false;

    // UI
    public GameObject PausePanel;
    public GameObject RewardPanel;
    public GameObject UpgradePanel;

    /*public enum RewardType
    {
        Experience,
        Gold,
        Gem,
        SkillShard,
        EquipmentShard,
        Donut,
        Chocolate
    }*/

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

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }

    // =========================
    // 스테이지 초기화
    // =========================
    private void InitializeStage()
    {
        // =========================
        // 1. 기본 스탯 불러오기
        // =========================
        context.attack = GameManager.baseAttack;
        context.attackCount = 1;
        context.attackRange = GameManager.baseAttackRange;
        context.attackInterval = GameManager.baseAttackInterval;
        context.bulletshot = 10f;

        Debug.Log($"[Stage Init] 공격력 {context.attack}, 범위 {context.attackRange}, 공격 간격 {context.attackInterval}");

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
        WaveExperience = 0;
        experience = 0;
        spawnedEnemies = 0; // 지금까지 스폰된 적 수 리셋
        enemiesDefeated = 0; // 처치한 적 수 리셋
        isBossDefeated = false;
        GameOvered = false;
        WallHP = 100f; // 벽 체력 초기화
        PausePanel.SetActive(false);
        RewardPanel.SetActive(false);

        // =========================
        // 3. 총알 풀 초기화
        // =========================
        bulletPool.Clear();
        AddBullets(3); // 초기 총알 3개 생성

        // =========================
        // 4. 스폰 시스템 시작
        // =========================
        spawnInterval = totalSpawnDuration / (totalEnemiesThisStage / enemiesPerSpawn); // 스테이지 전체 적 수에 맞춰 스폰 간격 조정
        currentWave = 1; // 웨이브 첫 시작
        StartCoroutine(SpawnRequestRoutine()); // 일정 시간마다 스폰 요청

        Time.timeScale = 1f; // 게임 시간 재개
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

    // Wall Hit
    public void OnWallHit(float Damage)
    {
        WallHP -= Damage;
        Debug.Log($"Wall HP: {WallHP}");
        if (WallHP <= 0)
        {
            Debug.Log("Wall 파괴! 스테이지 실패...");
            GameOver();
            // 스테이지 실패 처리 추가 예정
        }
    }

    // =========================
    // 적 처치
    // =========================
    public void OnEnemyDefeated()
    {
        enemiesDefeated++;

        int enemiesPerWave = totalEnemiesThisStage / maxWaves;
        int newWave = enemiesDefeated / enemiesPerWave;

        if (newWave > lastWave)
        {
            lastWave = newWave;
            OnWaveTriggered(newWave);
        }

        WaveExperience = CalculateWaveProgress();
        uiManager.UpdateWaveProgressUI(WaveExperience, maxWaves);
    }

    private void OnWaveTriggered(int wave)
    {
        Debug.Log($"웨이브 {wave} 시작!");

        Time.timeScale = 0f;
        uiManager.UpdateUpgradeList();
    }

    // 웨이브 경험치 계산
    public float CalculateWaveProgress()
    {
        int enemiesPerWave = Mathf.CeilToInt((float)totalEnemiesThisStage / maxWaves);

        int calculatedWave = enemiesDefeated / enemiesPerWave;
        int defeatedInCurrentWave = enemiesDefeated % enemiesPerWave;

        float waveRatio = (float)defeatedInCurrentWave / enemiesPerWave;

        return calculatedWave + waveRatio;
    }

    // 일시정지 및 재시작
    public void Pause()
    {
        //Reward_Calculation();
        if(GameOvered) return;
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f; // 게임 재시작
            PausePanel.SetActive(false);
        }
        else
        {
            Time.timeScale = 0f; // 게임 일시정지
            PausePanel.SetActive(true);
        }
        
    }

    public void GameOver()
    {
        GameOvered = true;
        Time.timeScale = 0f; // 게임 일시정지
        // 보상 계산
        //Reward_Calculation();
        RewardPanel.SetActive(true);
        //OnStageClear();
        CalculateScore();
    }

    // 보상 계산
    /*    private void Reward_Calculation()
        {
            // 경험치 및 골드 보상 계산(임시, 이후 웨이브 별 보상 테이블로 대체 예정)
            float ratio = (float)enemiesDefeated / totalEnemiesThisStage;

            experience = (int)(ratio * maxExperience);
            gold = (int)(ratio * maxgold);
            gem = (int)(ratio * maxgem);
            skillShard = (int)(ratio * maxskillShard);
            equipmentShard = (int)(ratio * maxequipmentShard);
            donut = (int)(ratio * maxdonut);
            chocolate = (int)(ratio * maxchocolate);

            List<int> rewards = new List<int>
            {
                experience,
                gold,
                gem,
                skillShard,
                equipmentShard,
                donut,
                chocolate
            };
            // 보상 UI 에 전달
            UIManager.UpdateRewardUI(rewards);
        }
    */

    // 점수 계산(프로토타입용)
    public void CalculateScore()
    {
        int score = 100000;
        float wallHealthRatio = WallHP / 100f + 1.0f;
        float enemyDefeatRatio = (float)enemiesDefeated / totalEnemiesThisStage;
        score = (int)(score * wallHealthRatio * enemyDefeatRatio);

        uiManager.UpdateScoreUI(score);
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
