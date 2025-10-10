using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackUpgrad : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeOption
    {
        public string name;
        public string description;

        public int attackCountDelta;      // 수량: 절댓값 증가
        public int CountDelta;            // 관통: 절댓값 증가
        public float attackPercent;       // 공격력: +/- % (예: 0.2 = +20%)
        public float attackRangePercent;  // 사거리: +/- %
        public float attackIntervalPercent; // 공격 간격: +/- %
        public float bulletSpeedPercent;  // 탄속: +/- %
        public float bulletSizePercent;   // 탄 크기: +/- %

        public void Apply()
        {
            var sm = StageManager.Instance;

            // 절댓값 증가
            sm.attackCount += attackCountDelta;
            sm.maxBulletHP += CountDelta;

            // 퍼센트 증가/감소
            sm.attack *= (1 + attackPercent);
            sm.attackRange *= (1 + attackRangePercent);
            sm.attackInterval *= (1 - attackIntervalPercent); // 공격 간격은 감소하면 빨라짐
            sm.bulletshot *= (1 + bulletSpeedPercent);

            foreach (GameObject bullet in sm.bulletPool)
            {
                if (bullet != null)
                    bullet.transform.localScale *= (1 + bulletSizePercent);
            }

            // 최소 발사 간격 제한
            if (sm.attackInterval < 0.1f)
                sm.attackInterval = 0.1f;
        }
    }

}
