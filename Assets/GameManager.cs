using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }
    //public BaseAttackSO attackSO;

    // =========================
    // 스테이지 진입 전 홈에서 세팅되는 값
    // =========================
    [Header("플레이어 초기 상태")]
    public float baseAttack = 10f;
    public float baseAttackRange = 1f;
    public float baseAttackInterval = 1f;

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
    public int gold = 0;
    public int donut = 0;
    public int gem = 0;
    public int chocolate = 0;
    public int skillShard = 0;
    public int equipmentShard = 0;

    // 플레이어 레벨
    public int playerLevel = 1;

    // 스킬 레벨
    public Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    // 스킬 목록
    public List<string> skillList = new List<string> { "Drone", "Sphere", "Turret" };

    [Header("빌더 SO")]
    public AttackBuilder attackBuilder;
    private AttackRuntimeData AttackData;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SceneManager.LoadScene("Home");
    }

    // =========================
    // 스테이지 시작 전 초기화
    // =========================
    public void InitializeStage()
    {
        Debug.Log("스테이지 시작: 홈에서 세팅된 값 적용");

        // 홈에서 세팅한 공격력/범위
        Debug.Log($"기본 공격력: {baseAttack}, 공격 범위: {baseAttackRange}");

        // 사용 가능한 스킬/캐릭터 확인
        foreach (var skill in skillUnlocked)
            Debug.Log($"스킬: {skill.Key}, 해금 여부: {skill.Value}");
        foreach (var character in specialCharacterUnlocked)
            Debug.Log($"캐릭터: {character.Key}, 해금 여부: {character.Value}");

        // 장비/공격 효과 확인
        Debug.Log("장비 효과: " + string.Join(", ", equipmentEffects));
        Debug.Log("공격 효과: " + string.Join(", ", attackEffects));
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
