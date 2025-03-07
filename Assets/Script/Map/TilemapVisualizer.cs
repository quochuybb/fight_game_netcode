using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap, wallTilemap;
    [SerializeField] private GameObject stained;
    [SerializeField] private TileBase floorTile, wallTop, wallSideRight, wallSideLeft, wallBottom, wallFull, wallInnerCornerDownLeft,
        wallInnerCornerDownRight, wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft, wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft;

    public void PaintFloorTile(IEnumerable<Vector2Int> floorPos)
    {
        PaintTile(floorPos, tilemap,floorTile);
    }

    private void PaintTile(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase floorTile)
    {
        float posStained = float.MinValue;
        Vector2 posStainedTile = Vector2.zero;
        foreach (var pos in positions)
        {
            float newPosStained = Vector2.Distance(new Vector2(0, 0), pos);
            if (posStained < newPosStained)
            {
                posStained = newPosStained;
                if (isCanPlaceStained(pos,positions))
                {
                    posStainedTile = pos;

                }
            }
            PaintSingleTile(pos, tilemap, floorTile);
        }
        Instantiate(stained, posStainedTile, Quaternion.identity);
    }

    private bool isCanPlaceStained(Vector2Int posPlace,IEnumerable<Vector2Int> positions)
    {

        if (positions.Contains(posPlace + new Vector2Int(0, 1)) 
            && positions.Contains(posPlace + new Vector2Int(1, 0)) 
            && positions.Contains(posPlace + new Vector2Int(-1, 0))
            && positions.Contains(posPlace + new Vector2Int(0, -1))
            && positions.Contains(posPlace + new Vector2Int(1, -1))
            && positions.Contains(posPlace + new Vector2Int(-1, -1))
            && positions.Contains(posPlace + new Vector2Int(1, 1))
            && positions.Contains(posPlace + new Vector2Int(-1, 1)))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    internal void PaintSingleCornerWall(Vector2Int pos, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType,2);
        TileBase tile = null;
        if (WallTypes.wallInnerCornerDownLeft.Contains(typeAsInt))
        {
            tile = wallInnerCornerDownLeft;
        } else if (WallTypes.wallInnerCornerDownRight.Contains(typeAsInt))
        {
            tile = wallInnerCornerDownRight;
        } else if (WallTypes.wallDiagonalCornerDownRight.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerDownRight;
        } else if (WallTypes.wallDiagonalCornerDownLeft.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerDownLeft;
        } 
        else if (WallTypes.wallDiagonalCornerUpRight.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerUpRight;
        } else if (WallTypes.wallDiagonalCornerUpLeft.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerUpLeft;
        } else if (WallTypes.wallFullEightDirections.Contains(typeAsInt))
        {
            tile = wallFull;
        } else if (WallTypes.wallBottmEightDirections.Contains(typeAsInt))
        {
            tile = wallBottom;
        }
        if (tile != null)
        {

            PaintSingleTile(pos, wallTilemap, tile);
            
        }
        
    }

    internal void PaintSingleBasicWall(Vector2Int pos, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType,2);
        TileBase tile = null;
        if (WallTypes.wallTop.Contains(typeAsInt))
        {
            tile = wallTop;
        }
        else if (WallTypes.wallSideRight.Contains(typeAsInt))
        {
            tile = wallSideRight;
        }
        else if (WallTypes.wallSideLeft.Contains(typeAsInt))
        {
            tile = wallSideLeft;
        }
        else if (WallTypes.wallBottm.Contains(typeAsInt))
        {
            tile = wallBottom;
        }else if (WallTypes.wallFull.Contains(typeAsInt))
        {
            tile = wallFull;
        }

        if (tile != null)
        {
            PaintSingleTile(pos, wallTilemap,tile );
        }
    }

    private void PaintSingleTile(Vector2Int position, Tilemap tilemap, TileBase floorTile)
    {
        var tilePos = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePos, floorTile);
    }

    public void ClearTile()
    {
        tilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }
}
