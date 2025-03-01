using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomWalkMapGenerator : AbstractMapGenerator
{
    [SerializeField] private RandomWalkConfig randomWalkConfig;

    public override void RunProceduralGeneration()
    {
        HashSet<Vector2Int> floorPos = RunRandomWalk(randomWalkConfig);
        visualizer.ClearTile();
        visualizer.PaintFloorTile(floorPos);
        WallGenerator.CreateWalls(floorPos, visualizer);
    }

    protected HashSet<Vector2Int> RunRandomWalk(RandomWalkConfig parameters)
    {
        var currentPos = startPos;
        HashSet<Vector2Int> floorPos = new HashSet<Vector2Int>();
        for (int i = 0; i < parameters.iterations; i++)
        {
            var path = ProceducalGenerationAlgorithsm.RandomWalk(currentPos, parameters.walkLenght);
            floorPos.UnionWith(path);
            if (parameters.startRandomWalk)
            {
                currentPos = floorPos.ElementAt(Random.Range(0, floorPos.Count));
            }
        }
        return floorPos;
    }
}
