using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathFinder
{
    private Dictionary<Vector3Int, TileType> searchableTiles;

    public List<TileType> findPath(TileType start, TileType end, List<TileType> inRangeTiles)
    {
        searchableTiles = new Dictionary<Vector3Int, TileType>();
        List<TileType> openList = new List<TileType>();
        HashSet<TileType> closedList = new HashSet<TileType>();
        if (inRangeTiles.Count > 0)
        {
            foreach (var item in inRangeTiles)
            {
                searchableTiles.Add(item.position, MapManager.instance.dataFromTiles[item.position]);
            }
        }
        else
        {
            searchableTiles = MapManager.instance.dataFromTiles;
        }

        openList.Add(start);

        while (openList.Count > 0)
        {
            TileType currentTileType = openList.OrderBy(x => x.F).First();
            openList.Remove(currentTileType);
            closedList.Add(currentTileType);
            if (currentTileType == end)
            {
                return getFinishedList(start, end);
            }
            foreach (TileType tile in currentTileType.neighbours)
            {
                if (tile.isBlocked || closedList.Contains(tile) || Mathf.Abs(currentTileType.position.z - tile.position.z) > 1)
                {
                    continue;
                }
                tile.G = GetManhattenDistance(start, tile);
                tile.H = GetManhattenDistance(end, tile);
                tile.previous = currentTileType;
                if (!openList.Contains(tile))
                {
                    openList.Add(tile);
                }
            }
        }
        return new List<TileType>();
    }

    private List<TileType> getFinishedList(TileType start, TileType end)
    {
        List<TileType> finishedList = new List<TileType>();
        TileType currentTile = end;
        while (currentTile != start)
        {
            finishedList.Add(currentTile);
            currentTile = currentTile.previous;
        }
        finishedList.Reverse();
        return finishedList;
    }

    private int GetManhattenDistance(TileType start, TileType tile)
    {
        return Mathf.Abs(start.position.x - tile.position.x) + Mathf.Abs(start.position.y - tile.position.y);
    }
}
