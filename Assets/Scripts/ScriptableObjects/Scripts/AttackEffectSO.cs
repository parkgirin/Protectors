using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AttackEffect", menuName = "ScriptableObjects/Attack/Effect", order = 1)]
public class AttackEffectSO : ScriptableObject
{
    [Header("이펙트 기본 정보")]
    public string effectName = "NewEffect";

    [Header("이펙트 속성 리스트")]
    public List<Property> properties = new List<Property>();
}
