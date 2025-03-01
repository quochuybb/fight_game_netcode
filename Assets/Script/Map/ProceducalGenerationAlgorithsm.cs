using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProceducalGenerationAlgorithsm 
{
    public static HashSet<Vector2Int> RandomWalk(Vector2Int startPos, int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();
        path.Add(startPos);
        var previous = startPos;
        for (int i = 0; i < walkLength; i++)
        {
            var newPos = previous + Direction2D.GetRandomCardinalDirection();
            path.Add(newPos);
            previous = newPos;
        }
        return path;
    }

    public static List<Vector2Int> RandomWalkCorridor(Vector2Int startPos, int corridorLength)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();
        var direction = Direction2D.GetRandomCardinalDirection();
        var currPos = startPos;
        corridor.Add(currPos);
        for (int i = 0; i < corridorLength; i++)
        {
            currPos += direction;
            corridor.Add(currPos);
        }
        return corridor;
    }
}

public static class Direction2D
{
    public static List<Vector2Int> CardinaDirectionList = new List<Vector2Int>
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
    };
    
    public static List<Vector2Int> diagonalDirectionList = new List<Vector2Int>
    {
        new Vector2Int(1,1), 
        new Vector2Int(1,-1), 
        new Vector2Int(-1, -1), 
        new Vector2Int(-1, 1)
    };
    public static List<Vector2Int> eightDirectionList = new List<Vector2Int>
    {
        new Vector2Int(0,1), //UP
        new Vector2Int(1,1), //UP-RIGHT
        new Vector2Int(1,0), //RIGHT
        new Vector2Int(1,-1), //RIGHT-DOWN
        new Vector2Int(0, -1), // DOWN
        new Vector2Int(-1, -1), // DOWN-LEFT
        new Vector2Int(-1, 0), //LEFT
        new Vector2Int(-1, 1) //LEFT-UP
    };

    public static Vector2Int GetRandomCardinalDirection()
    {
        return CardinaDirectionList[UnityEngine.Random.Range(0, CardinaDirectionList.Count)];
    }
}
