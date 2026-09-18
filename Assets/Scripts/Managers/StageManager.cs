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
    [SerializeField] private AttackDataSO startingAttack;
    [SerializeField] private List<AttackDataSO> startingAttacks = new List<AttackDataSO>();
    [SerializeField] private List<AttackDataSO> unlockableAttacks = new List<AttackDataSO>();
    [SerializeField] private AttackBuilder attackBuilder;

    private AttackRuntimeData attackData;

    public IReadOnlyList<AttackDataSO> UnlockableAttacks => unlockableAttacks;


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
    public int maxWaves;
    private int lastWave = 0;

    [Header("Stage Objects")]
    //public GameObject player;
    public GameObject enemyManager;
    public GameObject uiCanvas;

    [Header("적 등장 제한")]
    public int totalEnemiesThisStage; // 스테이지 전체 등장 가능 적 수
    private int spawnedEnemies = 0;         // 지금까지 스폰된 적 수

    [Header("적 생성 설정")]
    public int enemiesPerSpawn;     // 한 번에 스폰할 적 수
    public float spawnInterval;     // 스폰 요청 간격 (초)

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
    private bool stageCompleted;
    private bool rewardsGranted;

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
        // GameManager 확인
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager 없음 → Stage 단독 실행. Home으로 이동합니다.");
            SceneManager.LoadScene("Home");
            return;
        }

        GameManager = GameManager.Instance;

        // 스테이지 초기화
        InitializeStage();
    }

    // 스테이지 초기화
    private void InitializeStage()
    {
        // UI Manager 참조 확보
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        // 필수 참조 확인
        if (context == null)
        {
            Debug.LogError("StageContext가 연결되지 않았습니다.");
            return;
        }

        if (startingAttacks.Count == 0 && startingAttack != null)
            startingAttacks.Add(startingAttack);

        if (startingAttacks.Count == 0)
        {
            Debug.LogError("At least one starting attack must be assigned.");
            return;
        }

        if (attackBuilder == null)
        {
            Debug.LogError("attackBuilder가 연결되지 않았습니다.");
            return;
        }

        // Every stage constructs fresh runtime copies. SO assets are never mutated.
        List<AttackRuntimeData> runtimeAttacks = new List<AttackRuntimeData>();
        foreach (AttackDataSO data in startingAttacks)
            if (data != null) runtimeAttacks.Add(attackBuilder.Build(data));
        if (runtimeAttacks.Count == 0)
        {
            Debug.LogError("No valid starting attack data was found.");
            return;
        }
        context.InitializeAttacks(runtimeAttacks);
        attackData = context.RuntimeAttack;

        Debug.Log(
            $"[Stage Init] 공격 수 {context.RuntimeAttacks.Count}, 공격력 {attackData.damage}, " +
            $"범위 {attackData.range}, " +
            $"공격 간격 {attackData.attackInterval}, " +
            $"탄환 수 {attackData.bulletCount}, " +
            $"탄환 속도 {attackData.bulletSpeed}, " +
            $"탄환 HP {attackData.bulletHP}"
        );

        // GameManager 데이터 확인
        foreach (var skill in GameManager.skillUnlocked)
            Debug.Log($"해금된 스킬: {skill.Key} → {skill.Value}");

        foreach (var character in GameManager.specialCharacterUnlocked)
            Debug.Log($"해금된 캐릭터: {character.Key} → {character.Value}");

        Debug.Log("장비 효과: " + string.Join(", ", GameManager.equipmentEffects));
        Debug.Log("공격 효과: " + string.Join(", ", GameManager.attackEffects));

        // 스테이지 상태 초기화
        if (totalEnemiesThisStage <= 0 || maxWaves <= 0 || enemiesPerSpawn <= 0 || spawnInterval <= 0f)
        {
            Debug.LogError("Stage spawn settings must be configured in the Inspector.");
            return;
        }
        currentWave = 0;
        lastWave = 0;
        WaveExperience = 0;
        experience = 0;
        spawnedEnemies = 0;
        enemiesDefeated = 0;
        isBossDefeated = false;
        GameOvered = false;
        stageCompleted = false;
        rewardsGranted = false;
        WallHP = 100f;

        if (PausePanel != null) PausePanel.SetActive(false);
        if (RewardPanel != null) RewardPanel.SetActive(false);
        if (UpgradePanel != null) UpgradePanel.SetActive(false);

        // 총알 풀 초기화
        bulletPool.Clear();
        if (bulletPrefab == null)
        {
            Debug.LogError("bulletPrefab is not assigned.");
            return;
        }
        AddBullets(3);

        // 스테이지 오브젝트 활성화
        if (player != null)
            player.SetActive(true);

        if (enemyManager != null)
            enemyManager.SetActive(true);

        if (uiCanvas != null)
            uiCanvas.SetActive(true);

        Debug.Log("Stage 시작: 모든 오브젝트 활성화됨!");

        // Spawn values are authored per stage in the Inspector.
        currentWave = 1;

        StartCoroutine(SpawnRequestRoutine());

        Time.timeScale = 1f;
    }

    private IEnumerator SpawnRequestRoutine()
    {
        while (spawnedEnemies < totalEnemiesThisStage && !isBossDefeated && !GameOvered)
        {
            int spawned = SpawnEnemies(enemiesPerSpawn);
            // A full pool is temporary: wait until a killed enemy returns to the pool.
            if (spawned == 0)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }
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

    public int SpawnEnemies(int requestedCount)
    {
        if (EnemyManager.Instance == null)
        {
            Debug.LogError("EnemyManager 인스턴스 없음! 스폰 불가");
            return 0;
        }

        // 남은 적 수 계산
        int remaining = totalEnemiesThisStage - spawnedEnemies;
        if (remaining <= 0)
        {
            Debug.Log("스테이지 전체 적 스폰 완료");
            return 0;
        }

        // 요청 수와 남은 수 중 작은 값만 스폰
        int spawnCount = Mathf.Min(requestedCount, remaining);
        int actuallySpawned = EnemyManager.Instance.SpawnEnemies(spawnCount);
        spawnedEnemies += actuallySpawned;
        Debug.Log($"적 {actuallySpawned}마리 스폰, 현재까지 총 {spawnedEnemies}/{totalEnemiesThisStage}");
        return actuallySpawned;
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

        int enemiesPerWave = Mathf.Max(1, Mathf.CeilToInt((float)totalEnemiesThisStage / maxWaves));
        int newWave = Mathf.Min(maxWaves, enemiesDefeated / enemiesPerWave);

        if (newWave > lastWave)
        {
            lastWave = newWave;
            OnWaveTriggered(newWave);
        }

        WaveExperience = CalculateWaveProgress();
        if (uiManager != null) uiManager.UpdateWaveProgressUI(WaveExperience, maxWaves);

        if (spawnedEnemies >= totalEnemiesThisStage && enemiesDefeated >= totalEnemiesThisStage)
            CompleteStage();
    }

    private void OnWaveTriggered(int wave)
    {
        Debug.Log($"웨이브 {wave} 시작!");

        if (wave <= 0 || wave >= maxWaves) return;
        Time.timeScale = 0f;
        if (uiManager == null || !uiManager.UpdateUpgradeList())
            Time.timeScale = 1f;
    }

    // 웨이브 경험치 계산
    public float CalculateWaveProgress()
    {
        int enemiesPerWave = Mathf.Max(1, Mathf.CeilToInt((float)totalEnemiesThisStage / maxWaves));

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
            if (PausePanel != null) PausePanel.SetActive(false);
        }
        else
        {
            Time.timeScale = 0f; // 게임 일시정지
            if (PausePanel != null) PausePanel.SetActive(true);
        }
        
    }

    public void GameOver()
    {
        if (GameOvered || stageCompleted) return;
        GameOvered = true;
        Time.timeScale = 0f; // 게임 일시정지
        // 보상 계산
        //Reward_Calculation();
        if (RewardPanel != null) RewardPanel.SetActive(true);
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

        if (uiManager != null) uiManager.UpdateScoreUI(score);
    }
    // =========================
    // 스테이지 종료
    // =========================
    public void OnStageClear()
    {
        // The same return button is used by the clear and game-over panels.
        // A failed run must never award clear rewards.
        if (!stageCompleted && !GameOvered)
            CompleteStage();
        SceneManager.LoadScene("Home");
    }

    private void CompleteStage()
    {
        if (stageCompleted) return;
        stageCompleted = true;
        Time.timeScale = 0f;
        CalculateRewards();
        if (RewardPanel != null) RewardPanel.SetActive(true);
        CalculateScore();
    }

    private void CalculateRewards()
    {
        if (rewardsGranted || GameManager.Instance == null) return;
        rewardsGranted = true;
        experience = maxExperience;
        gold = maxgold;
        gem = maxgem;
        skillShard = maxskillShard;
        equipmentShard = maxequipmentShard;
        donut = maxdonut;
        chocolate = maxchocolate;
        GameManager.Instance.AddExperience(experience);
        GameManager.Instance.AddGold(gold);
        GameManager.Instance.AddGem(gem);
        GameManager.Instance.AddSkillShard(skillShard);
        GameManager.Instance.AddEquipmentShard(equipmentShard);
        GameManager.Instance.AddDonut(donut);
        GameManager.Instance.AddChocolate(chocolate);

        Debug.Log("Stage clear!");
        Debug.Log($"획득 경험치: {experience}, 골드: {gold}");
        Debug.Log($"획득 스킬조각: {skillShard}, 장비조각: {equipmentShard}");
        Debug.Log($"획득 도넛: {donut}, 초콜릿: {chocolate}");
    }
}
