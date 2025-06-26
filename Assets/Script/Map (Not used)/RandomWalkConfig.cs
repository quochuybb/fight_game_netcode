using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomWalkConfig", menuName = "Configs/RandomWalkConfig")]
public class RandomWalkConfig : ScriptableObject
{
    public int iterations = 10, walkLenght = 10;
    public bool startRandomWalk = true;
}
