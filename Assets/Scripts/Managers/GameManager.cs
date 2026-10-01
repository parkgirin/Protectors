using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum PlayMode
    {
        Stage,
        EndlessLab
    }

    private const string HighScoreKey = "HighScore";
    private const string LastScoreKey = "LastScore";
    private const string EndlessHighScoreKey = "EndlessHighScore";
    private const string EndlessBestWaveKey = "EndlessBestWave";
    private const string Stage01ClearedKey = "Stage01Cleared";
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }
    //public BaseAttackSO attackSO;


    [Header("사용 가능한 캐릭터/스킬")]
    public Dictionary<string, bool> skillUnlocked = new Dictionary<string, bool>();
    public Dictionary<string, bool> specialCharacterUnlocked = new Dictionary<string, bool>();

    [Header("장비/공격 효과")]
    public List<string> equipmentEffects = new List<string>();
    public List<string> attackEffects = new List<string>();

    // =========================
    // 홈에서 사용하는 경험치/재화
    // =========================
    [Header("경험치/재화")]
    public int experience = 0;
    [SerializeField] private CurrencyAsset goldCurrency;
    [SerializeField] private CurrencyAsset donutCurrency;
    [SerializeField] private CurrencyAsset gemCurrency;
    [SerializeField] private CurrencyAsset chocolateCurrency;
    [SerializeField] private CurrencyAsset skillShardCurrency;
    [SerializeField] private CurrencyAsset equipmentShardCurrency;

    public int gold { get => GetAmount(goldCurrency); set => SetAmount(goldCurrency, value); }
    public int donut { get => GetAmount(donutCurrency); set => SetAmount(donutCurrency, value); }
    public int gem { get => GetAmount(gemCurrency); set => SetAmount(gemCurrency, value); }
    public int chocolate { get => GetAmount(chocolateCurrency); set => SetAmount(chocolateCurrency, value); }
    public int skillShard { get => GetAmount(skillShardCurrency); set => SetAmount(skillShardCurrency, value); }
    public int equipmentShard { get => GetAmount(equipmentShardCurrency); set => SetAmount(equipmentShardCurrency, value); }

    // 플레이어 레벨
    public int playerLevel = 1;
    public int highScore { get; private set; }
    public int lastScore { get; private set; }
    public int endlessHighScore { get; private set; }
    public int endlessBestWave { get; private set; }
    public bool HasClearedStage01 => PlayerPrefs.GetInt(Stage01ClearedKey, 0) == 1;
    public PlayMode CurrentPlayMode { get; private set; } = PlayMode.Stage;

    // 스킬 레벨
    public Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    // 스킬 목록
    public List<string> skillList = new List<string> { "Drone", "Sphere", "Turret" };

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        lastScore = PlayerPrefs.GetInt(LastScoreKey, 0);
        endlessHighScore = PlayerPrefs.GetInt(EndlessHighScoreKey, 0);
        endlessBestWave = PlayerPrefs.GetInt(EndlessBestWaveKey, 0);

        // Scores were only recorded on a successful stage clear in earlier builds.
        // Preserve that progress when migrating to the explicit Stage 01 clear flag.
        if (highScore > 0 && !HasClearedStage01)
        {
            PlayerPrefs.SetInt(Stage01ClearedKey, 1);
            PlayerPrefs.Save();
        }
    }

    // =========================
    // 스테이지 시작 전 초기화
    // =========================
    public void InitializeStage()
    {
        Debug.Log("스테이지 시작: 홈에서 세팅된 값 적용");

        // 홈에서 세팅한 공격력/범위
        //Debug.Log($"기본 공격력: {baseAttack}, 공격 범위: {baseAttackRange}");

        // 사용 가능한 스킬/캐릭터 확인
        foreach (var skill in skillUnlocked)
            Debug.Log($"스킬: {skill.Key}, 해금 여부: {skill.Value}");
        foreach (var character in specialCharacterUnlocked)
            Debug.Log($"캐릭터: {character.Key}, 해금 여부: {character.Value}");

        // 장비/공격 효과 확인
        Debug.Log("장비 효과: " + string.Join(", ", equipmentEffects));
        Debug.Log("공격 효과: " + string.Join(", ", attackEffects));
    }

    public void SetPlayMode(PlayMode playMode)
    {
        CurrentPlayMode = playMode;
    }

    // =========================
    // 스테이지 완료 후 보상 증가
    // =========================
    public void AddExperience(int amount) => experience += amount;
    public void AddGold(int amount) => gold += amount;
    public void AddDonut(int amount) => donut += amount;
    public void AddGem(int amount) => gem += amount;
    public void AddChocolate(int amount) => chocolate += amount;
    public void AddSkillShard(int amount) => skillShard += amount;
    public void AddEquipmentShard(int amount) => equipmentShard += amount;

    private static int GetAmount(CurrencyAsset currency) => currency != null ? currency.Amount : 0;

    private static void SetAmount(CurrencyAsset currency, int amount)
    {
        if (currency != null) currency.Amount = amount;
    }

    public int RegisterScore(int score)
    {
        lastScore = Mathf.Max(0, score);
        PlayerPrefs.SetInt(LastScoreKey, lastScore);

        if (lastScore > highScore)
        {
            highScore = lastScore;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
        }

        PlayerPrefs.Save();
        return highScore;
    }

    public void RegisterStage01Clear()
    {
        PlayerPrefs.SetInt(Stage01ClearedKey, 1);
        PlayerPrefs.Save();
    }

    public void RegisterEndlessResult(int score, int completedWaves)
    {
        if (score > endlessHighScore)
        {
            endlessHighScore = score;
            PlayerPrefs.SetInt(EndlessHighScoreKey, endlessHighScore);
        }

        if (completedWaves > endlessBestWave)
        {
            endlessBestWave = completedWaves;
            PlayerPrefs.SetInt(EndlessBestWaveKey, endlessBestWave);
        }

        PlayerPrefs.Save();
    }

    // =========================
    // 홈에서 스킬/캐릭터 해금
    // =========================
    public void UnlockSkill(string skillName)
    {
        if (!skillUnlocked.ContainsKey(skillName))
            skillUnlocked.Add(skillName, true);
        else
            skillUnlocked[skillName] = true;
    }

    public void UnlockSpecialCharacter(string characterName)
    {
        if (!specialCharacterUnlocked.ContainsKey(characterName))
            specialCharacterUnlocked.Add(characterName, true);
        else
            specialCharacterUnlocked[characterName] = true;

        AudioManager.Instance?.SetCharacterLayer(characterName, true);
    }

    // 홈에서 장비/공격 효과 적용
    public void AddEquipmentEffect(string effect)
    {
        if (!equipmentEffects.Contains(effect))
            equipmentEffects.Add(effect);
    }

    public void AddAttackEffect(string effect)
    {
        if (!attackEffects.Contains(effect))
            attackEffects.Add(effect);
    }
}
