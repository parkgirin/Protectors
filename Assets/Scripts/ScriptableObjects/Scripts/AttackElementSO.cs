using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AttackElement", menuName = "ScriptableObjects/Attack/Element", order = 1)]
public class AttackElementSO : ScriptableObject
{
    [Header("加己 扁夯 沥焊")]
    public string elementName = "NewElement";

    [Header("加己 加己 府胶飘")]
    public List<Property> properties = new List<Property>();
}
