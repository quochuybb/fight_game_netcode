using System;using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Character Stats", menuName = "Stats/Character Stats")]
public class CharacterStats : ScriptableObject
{
    public float healthPoint;
    public float damagePercentage;
    public float speedMove;

    public CharacterStatsNetwork Mapping()
    {
        CharacterStatsNetwork statsNetwork = new CharacterStatsNetwork();
        statsNetwork.healthPoint = this.healthPoint;
        statsNetwork.damagePercentage = this.damagePercentage;
        statsNetwork.speedMove = this.speedMove;
        return statsNetwork;
    }
    
}
