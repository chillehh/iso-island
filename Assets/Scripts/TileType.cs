using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum typeOfTile { WATER, GRASS, DIRT, STONE, TREE_S, TREE_L }
public class TileType
{
    public typeOfTile type;
    public TileBase tile;
    public Vector3Int position;
    public List<TileType> neighbours;

    // For path finding
    public int G;
    public int H;
    public int F { get { return G + H; } }
    public TileType previous;
    public bool isBlocked;
}
