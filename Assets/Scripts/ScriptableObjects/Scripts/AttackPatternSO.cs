using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AttackPattern", menuName = "ScriptableObjects/Attack/Pattern", order = 1)]
public class AttackPatternSO : ScriptableObject
{
    [Header("패턴 기본 정보")]
    public string patternName = "NewPattern";

    [Header("패턴 속성 리스트")]
    public List<Property> properties = new List<Property>();
}
