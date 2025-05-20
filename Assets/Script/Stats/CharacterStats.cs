using System;using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Character Stats", menuName = "Stats/Character Stats")]
public class CharacterStats : ScriptableObject
{
    public float alive;
    public float healthPoint;
    public float gut;
    public float speedMove;
    public float armor;
    public float poison;
    public float burn;

    public CharacterStatsNetwork Mapping()
    {
        CharacterStatsNetwork statsNetwork = new CharacterStatsNetwork();
        statsNetwork.alive = alive;
        statsNetwork.healthPoint = this.healthPoint;
        statsNetwork.gut = this.gut;
        statsNetwork.speedMove = this.speedMove;
        statsNetwork.armor = this.armor;
        statsNetwork.poison = this.poison;
        statsNetwork.burn = this.burn;
        return statsNetwork;
    }
    
}
