using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeManager : MonoBehaviour
{
    public void SetupStage()
    {
        GameManager.Instance.UnlockSkill("Fireball");
        GameManager.Instance.UnlockSpecialCharacter("Mars");

        GameManager.Instance.AddEquipmentEffect("CriticalBoost");
        GameManager.Instance.AddAttackEffect("Burn");

        // 씬 전환 전에 스테이지 초기화
        GameManager.Instance.InitializeStage();

        // 스테이지 씬으로 전환
        SceneManager.LoadScene("Stage");
    }
}
