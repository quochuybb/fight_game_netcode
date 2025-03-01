using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackConfig : ScriptableObject
{
    public float size;
    public float damage;
    public float speed;
    public float delay;
    public LayerMask layerMask;
}
