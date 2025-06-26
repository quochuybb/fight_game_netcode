using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractMapGenerator : MonoBehaviour
{
    [SerializeField] public TilemapVisualizer visualizer = null;
    [SerializeField] public Vector2Int startPos = Vector2Int.zero;

    public void GernerateMap()
    {
        visualizer.ClearTile();
        RunProceduralGeneration();
    }
    public abstract void RunProceduralGeneration();
}
