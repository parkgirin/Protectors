using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageContext : MonoBehaviour
{
    // 스테이지 내 플레이어 초기 스탯
    public float attack;
    public int attackCount = 1; // 발사 수
    public float attackRange;
    public float attackInterval;
    public float bulletshot = 10f;   // 총알 속도
    public int maxBulletHP = 1;   // 총알 관통력

    //public AttackLevelData[] attackLevels;
}
