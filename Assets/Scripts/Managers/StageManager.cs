using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    private const string FastForwardUnlockedKey = "Stage.FastForwardUnlocked";
    private static readonly float[] FastForwardSpeeds = { 1f, 2f, 3f };

    public static StageManager Instance;

    public GameManager GameManager;

    [Header("References")]
    public UIManager uiManager;
    public GameObject player;
    public GameColorPalette colorPalette;

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

    [Header("웨이브별 적 성장")]
    [Min(1f)] public float enemyHealthMultiplierPerWave = 1.12f;
    [Min(1f)] public float enemyAttackMultiplierPerWave = 1.08f;
    [Min(1f)] public float enemyCountMultiplierPerWave = 1.1f;

    public float CurrentEnemyHealthMultiplier => GetWaveMultiplier(enemyHealthMultiplierPerWave);
    public float CurrentEnemyAttackMultiplier => GetWaveMultiplier(enemyAttackMultiplierPerWave);
    public float CurrentEnemyCountMultiplier => GetWaveMultiplier(enemyCountMultiplierPerWave);
    private bool IsEndlessMode => GameManager != null &&
        GameManager.CurrentPlayMode == global::GameManager.PlayMode.EndlessLab;

    // 처치한 적 수
    public int enemiesDefeated = 0;
    public float WaveExperience = 0;

    public Targeting targeting;

    // 탄환 오브젝트 풀
    public GameObject bulletPrefab;
    public List<GameObject> bulletPool = new List<GameObject>();
    [SerializeField, Min(0)] private int initialAreaEffectPoolSize = 8;
    private readonly List<ProjectileAreaEffect> areaEffectPool = new List<ProjectileAreaEffect>();

    // 보스 처치 후 참이 되는 bool
    public bool isBossDefeated = false;

    public bool GameOvered = false;
    private bool stageCompleted;

    // 사용자 인터페이스
    public GameObject PausePanel;
    public GameObject RewardPanel;
    public GameObject UpgradePanel;
    public Button fastButton;
    private int fastForwardSpeedIndex;
    private Graphic fastButtonIcon;
    private PaletteColorApplicator fastButtonPalette;
    private PaletteColorApplicator fastButtonIconPalette;

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
        AddAreaEffects(initialAreaEffectPoolSize);
        ConfigureFastForwardButton();
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
        fastForwardSpeedIndex = 0;
        Time.timeScale = FastForwardSpeeds[fastForwardSpeedIndex];
        RefreshFastForwardButton();
        // UI Manager 참조 확보
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        Camera stageCamera = Camera.main;
        FixedAspectViewport fixedViewport = GetComponent<FixedAspectViewport>();
        if (fixedViewport == null) fixedViewport = gameObject.AddComponent<FixedAspectViewport>();
        fixedViewport.Configure(stageCamera);

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

        // 스테이지마다 새 런타임 복사본을 만들며 원본 SO 에셋은 변경하지 않는다.
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

        // 임시 테스트 UI 및 실제 캐릭터 상태가 이전 스테이지의 테스트 상태를 물려받지 않게 한다.
        AudioManager.Instance?.SetJupiterLayer(false);
        AudioManager.Instance?.SetSaturnLayer(false);
        foreach (var character in GameManager.specialCharacterUnlocked)
        {
            Debug.Log($"해금된 캐릭터: {character.Key} → {character.Value}");
            AudioManager.Instance?.SetCharacterLayer(character.Key, character.Value);
        }

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
        WallHP = 100f;
        if (uiManager != null) uiManager.UpdateBarrierHealthUI(WallHP, 100f);

        if (PausePanel != null) PausePanel.SetActive(false);
        if (RewardPanel != null) RewardPanel.SetActive(false);
        if (UpgradePanel != null) UpgradePanel.SetActive(false);

        EnsurePopupBackdrops();

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

        // 생성 관련 수치는 스테이지별로 인스펙터에서 설정한다.
        currentWave = 1;

        // 전투가 시작되기 전에 첫 강화 한 번을 선택하게 한다.
        Time.timeScale = 0f;
        if (uiManager == null || !uiManager.UpdateUpgradeList())
            ResumeGameplay();
        StartCoroutine(SpawnRequestRoutine());
    }

    private IEnumerator SpawnRequestRoutine()
    {
        while (Time.timeScale <= 0f && !GameOvered)
            yield return null;
        while ((IsEndlessMode || spawnedEnemies < totalEnemiesThisStage) && !isBossDefeated && !GameOvered)
        {
            int waveSpawnCount = Mathf.Max(1, Mathf.RoundToInt(enemiesPerSpawn * CurrentEnemyCountMultiplier));
            int spawned = SpawnEnemies(waveSpawnCount);
            // 풀이 가득 찬 경우 처치된 적이 풀로 돌아올 때까지 기다린다.
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

    public ProjectileAreaEffect GetAreaEffect()
    {
        foreach (ProjectileAreaEffect effect in areaEffectPool)
            if (effect != null && !effect.gameObject.activeSelf)
                return effect;

        return CreateAreaEffect();
    }

    public void ReleaseAreaEffect(ProjectileAreaEffect effect)
    {
        if (effect == null || !areaEffectPool.Contains(effect)) return;
        effect.gameObject.SetActive(false);
    }

    private void AddAreaEffects(int count)
    {
        for (int i = 0; i < count; i++) CreateAreaEffect();
    }

    private ProjectileAreaEffect CreateAreaEffect()
    {
        GameObject effectObject = new GameObject("포탄 폭발");
        effectObject.transform.SetParent(transform, false);
        ProjectileAreaEffect effect = effectObject.AddComponent<ProjectileAreaEffect>();
        effectObject.SetActive(false);
        areaEffectPool.Add(effect);
        return effect;
    }

    public int SpawnEnemies(int requestedCount)
    {
        if (EnemyManager.Instance == null)
        {
            Debug.LogError("EnemyManager 인스턴스 없음! 스폰 불가");
            return 0;
        }

        // 일반 스테이지는 총 등장 수를 제한하고, 실험실 무한모드는 매 요청마다 계속 생성한다.
        int remaining = totalEnemiesThisStage - spawnedEnemies;
        if (!IsEndlessMode && remaining <= 0)
        {
            Debug.Log("스테이지 전체 적 스폰 완료");
            return 0;
        }

        int spawnCount = IsEndlessMode ? requestedCount : Mathf.Min(requestedCount, remaining);
        int actuallySpawned = EnemyManager.Instance.SpawnEnemies(
            spawnCount, CurrentEnemyHealthMultiplier, CurrentEnemyAttackMultiplier);
        spawnedEnemies += actuallySpawned;
        Debug.Log($"적 {actuallySpawned}마리 스폰, 현재까지 총 {spawnedEnemies}/{totalEnemiesThisStage}");
        return actuallySpawned;
    }

    // 방벽 피격 처리
    public void OnWallHit(float Damage)
    {
        WallHP = Mathf.Max(0f, WallHP - Damage);
        if (uiManager != null) uiManager.UpdateBarrierHealthUI(WallHP, 100f);
        Debug.Log($"Wall HP: {WallHP}");
        if (WallHP <= 0)
        {
            Debug.Log("Wall 파괴! 스테이지 실패...");
            GameOver();
            // 스테이지 실패 처리 추가 예정
        }
    }

    public void HealWall(float amount)
    {
        if (amount <= 0f || GameOvered || stageCompleted) return;
        WallHP = Mathf.Min(100f, WallHP + amount);
        if (uiManager != null) uiManager.UpdateBarrierHealthUI(WallHP, 100f);
    }

    public void HealWallFromMissingHealth(float ratio)
    {
        if (ratio <= 0f || GameOvered || stageCompleted) return;
        // 흡혈은 현재 공격력 대신 잃은 배리어 체력을 기준으로 회복한다.
        HealWall(Mathf.Max(0f, 100f - WallHP) * ratio);
    }

    // =========================
    // 적 처치
    // =========================
    public void OnEnemyDefeated()
    {
        // 종료 후 도착한 충돌 콜백은 결과를 바꾸거나 강화 창을 다시 열면 안 된다.
        if (GameOvered || stageCompleted) return;
        enemiesDefeated++;

        int enemiesPerWave = Mathf.Max(1, Mathf.CeilToInt((float)totalEnemiesThisStage / maxWaves));
        int newWave = IsEndlessMode
            ? enemiesDefeated / enemiesPerWave
            : Mathf.Min(maxWaves, enemiesDefeated / enemiesPerWave);

        if (newWave > lastWave)
        {
            lastWave = newWave;
            // 무한모드에서는 웨이브가 계속 진행되지만 강화 횟수는 일반 스테이지와 같게 제한한다.
            if (IsEndlessMode || newWave < maxWaves)
            {
                currentWave = newWave + 1;
                OnWaveTriggered(currentWave);
            }
        }

        WaveExperience = CalculateWaveProgress();
        if (uiManager != null) uiManager.UpdateWaveProgressUI(WaveExperience, maxWaves, IsEndlessMode);

        if (!IsEndlessMode && spawnedEnemies >= totalEnemiesThisStage && enemiesDefeated >= totalEnemiesThisStage)
            CompleteStage();
    }

    private void OnWaveTriggered(int wave)
    {
        Debug.Log($"웨이브 {wave} 시작!");

        if (wave <= 1 || wave > maxWaves) return;
        Time.timeScale = 0f;
        if (uiManager == null || !uiManager.UpdateUpgradeList())
            ResumeGameplay();
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
        // 강화 및 결과 화면이 닫힐 때까지 해당 화면이 일시정지 상태를 유지한다.
        if (GameOvered || stageCompleted || (UpgradePanel != null && UpgradePanel.activeSelf) ||
            (uiManager != null && uiManager.UpgradePanel != null && uiManager.UpgradePanel.activeSelf)) return;
        if (Time.timeScale == 0f)
        {
            ResumeGameplay();
            if (PausePanel != null) PausePanel.SetActive(false);
        }
        else
        {
            Time.timeScale = 0f; // 게임 일시정지
            ShowPopup(PausePanel);
        }
        
    }

    public void GameOver()
    {
        if (GameOvered || stageCompleted) return;
        GameOvered = true;
        AudioManager.Instance?.PlaySfx(GameSfx.GameOver);
        Time.timeScale = 0f; // 게임 일시정지
        // 정지 팝업에서 철수한 경우, 결과 팝업만 남기고 기존 팝업을 닫는다.
        if (PausePanel != null) PausePanel.SetActive(false);
        if (uiManager != null) uiManager.PrepareResultPanel(false);
        ShowPopup(RewardPanel);
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

        int completedWaves = Mathf.Max(0, currentWave - 1);
        int highScore = score;
        if (GameManager != null)
        {
            if (IsEndlessMode) GameManager.RegisterEndlessResult(score, completedWaves);
            else highScore = GameManager.RegisterScore(score);
        }
        if (uiManager != null)
            uiManager.UpdateScoreUI(score, highScore, IsEndlessMode, completedWaves);
    }

    private float GetWaveMultiplier(float multiplierPerWave)
    {
        return Mathf.Pow(Mathf.Max(1f, multiplierPerWave), Mathf.Max(0, currentWave - 1));
    }
    // =========================
    // 스테이지 종료
    // =========================
    public void OnStageClear()
    {
        // 클리어와 게임 오버 화면은 동일한 복귀 버튼을 사용한다.
        if (!stageCompleted && !GameOvered)
            CompleteStage();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Home");
    }

    private void CompleteStage()
    {
        // 같은 물리 프레임에서도 클리어와 실패가 동시에 처리되지 않도록 한다.
        if (stageCompleted || GameOvered) return;
        stageCompleted = true;
        AudioManager.Instance?.PlaySfx(GameSfx.StageClear);
        GameManager?.RegisterStage01Clear();
        PlayerPrefs.SetInt(FastForwardUnlockedKey, 1);
        PlayerPrefs.Save();
        RefreshFastForwardButton();
        Time.timeScale = 0f;
        if (uiManager != null) uiManager.PrepareResultPanel(true);
        ShowPopup(RewardPanel);
        CalculateScore();
    }

    private static void ShowPopup(GameObject popup)
    {
        if (popup == null) return;
        // 딤 배경과 팝업을 HUD 최상단에 함께 배치한다.
        PopupBackdropOverlay.Show(popup);
        popup.SetActive(true);
    }

    private void EnsurePopupBackdrops()
    {
        AddBackdrop(PausePanel);
        AddBackdrop(RewardPanel);
        AddBackdrop(UpgradePanel);
    }

    private static void AddBackdrop(GameObject popup)
    {
        if (popup != null && popup.GetComponent<PopupBackdropOverlay>() == null)
            popup.AddComponent<PopupBackdropOverlay>();
    }

    private void ConfigureFastForwardButton()
    {
        if (fastButton == null) return;
        fastButtonIcon = fastButton.transform.Find("FastIcon")?.GetComponent<Graphic>();
        fastButtonPalette = fastButton.GetComponent<PaletteColorApplicator>();
        fastButtonIconPalette = fastButtonIcon != null ? fastButtonIcon.GetComponent<PaletteColorApplicator>() : null;
        fastButton.onClick.AddListener(CycleFastForwardSpeed);
        PaletteSettings.Changed += RefreshFastForwardButton;
        RefreshFastForwardButton();
    }

    private void OnDestroy() => PaletteSettings.Changed -= RefreshFastForwardButton;

    private void RefreshFastForwardButton()
    {
        if (fastButton == null) return;
        bool unlocked = !GameOvered && PlayerPrefs.GetInt(FastForwardUnlockedKey, 0) == 1;
        fastButton.interactable = unlocked;

        PaletteColor colorRole = GetFastForwardColorRole(unlocked);
        ApplyFastForwardColor(fastButton.targetGraphic, fastButtonPalette, colorRole);
        ApplyFastForwardColor(fastButtonIcon, fastButtonIconPalette, colorRole);
    }

    private void CycleFastForwardSpeed()
    {
        if (GameOvered || PlayerPrefs.GetInt(FastForwardUnlockedKey, 0) != 1) return;
        fastForwardSpeedIndex = (fastForwardSpeedIndex + 1) % FastForwardSpeeds.Length;
        Time.timeScale = FastForwardSpeeds[fastForwardSpeedIndex];
        RefreshFastForwardButton();
    }

    private PaletteColor GetFastForwardColorRole(bool unlocked)
    {
        if (!unlocked) return PaletteColor.StageRedDark;

        switch (fastForwardSpeedIndex)
        {
            case 1: return PaletteColor.StageRed;
            case 2: return PaletteColor.StageRedBright;
            default: return PaletteColor.StageRedDark;
        }
    }

    private void ApplyFastForwardColor(Graphic graphic, PaletteColorApplicator paletteBinding, PaletteColor colorRole)
    {
        if (paletteBinding != null)
        {
            paletteBinding.ColorRole = colorRole;
            return;
        }
        if (graphic == null) return;
        graphic.color = colorPalette != null
            ? colorPalette.Get(colorRole)
            : colorRole == PaletteColor.StageRedBright ? new Color(1f, 0.416f, 0.341f)
            : colorRole == PaletteColor.StageRed ? new Color(0.933f, 0.29f, 0.224f)
            : new Color(0.663f, 0.184f, 0.22f);
    }

    public void ResumeGameplay()
    {
        if (GameOvered || stageCompleted) return;
        Time.timeScale = PlayerPrefs.GetInt(FastForwardUnlockedKey, 0) == 1
            ? FastForwardSpeeds[fastForwardSpeedIndex]
            : 1f;
    }

}
