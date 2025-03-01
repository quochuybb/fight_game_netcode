using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Character Stats", menuName = "Stats/Character Stats")]
public class CharacterStats : ScriptableObject
{
    public float maxHealth;
    public float speed;
    public float damage;
    public Color color;
    
    public CharacterStats Clone()
    {
        CharacterStats clone = CreateInstance<CharacterStats>();
        clone.maxHealth = this.maxHealth; 
        clone.speed = this.speed;
        clone.damage = this.damage;
        return clone;
    }
}
