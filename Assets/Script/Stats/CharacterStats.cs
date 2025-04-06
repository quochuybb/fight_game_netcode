using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Character Stats", menuName = "Stats/Character Stats")]
public class CharacterStats : ScriptableObject
{
    public float healthPoint;
    public float damagePercentage;
    public float speedMove;
    public Color color;

    public CharacterStatsNetwork MappingToStruct()
    {
        CharacterStatsNetwork serializable = new CharacterStatsNetwork();
        serializable.healthPoint = this.healthPoint;
        serializable.damagePercentage = this.damagePercentage;
        serializable.speedMove = this.speedMove;
        return serializable;
    }
}
