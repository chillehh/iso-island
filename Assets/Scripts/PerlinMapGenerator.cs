using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PerlinMapGenerator : MonoBehaviour
{
    public static PerlinMapGenerator instance;

    // Tile values 0 - 1
    public float water = 0.01f;
    public float sand = 0.4f;
    public float dirt = 0.6f;
    public float grass = 0.7f;
    public float tree_s = 0.8f;
    public float tree_l = 0.9f;

    public float scale = 0.1f;

    public List<Tile> mapTiles = new();
    [SerializeField] private Tile playerTile;

    public int sizeMin = 20;
    public int sizeMax = 40;
    public string seed;
    private string lastSeed;

    private int size;

    public Tilemap tileMap;
    public Tilemap playerMap;

    private TileType[,] tiles;
    private List<Vector3Int> possiblePlayerSpawns;
    private List<Vector3Int> playerSpawns;

    public Dictionary<Vector3Int, TileType> dataFromTiles;
    private List<Vector3Int> map;

    // Inputs
    [SerializeField] private InputAction spacebar;

    private void OnEnable()
    {
        spacebar.Enable();
        spacebar.performed += spacePressed;
    }

    private void OnDisable()
    {
        spacebar.performed -= spacePressed;
        spacebar.Disable();
    }

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    public void generateIsland(string newSeed)
    {
        seed = newSeed;
        generate();
    }

    private void spacePressed(InputAction.CallbackContext ctx)
    {
        if (SceneManager.GetActiveScene().name != "MainGame")
        {
            if (lastSeed == seed)
            {
                seed = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
            }
            generate();
            lastSeed = seed;
        }
    }

    void Start()
    {
        generate();
        lastSeed = seed;
    }

    private typeOfTile getTypeFromNoise(float noiseValue)
    {
        if (noiseValue <= water)
        {
            return typeOfTile.WATER;
        } else if (noiseValue <= sand)
        {
            return typeOfTile.STONE;
        } else if (noiseValue <= dirt)
        {
            return typeOfTile.DIRT;
        } else if (noiseValue <= grass)
        {
            return typeOfTile.GRASS;
        } else if (noiseValue <= tree_s)
        {
            return typeOfTile.TREE_S;
        } else
        {
            return typeOfTile.TREE_L;
        }
    }

    private Tile getTileFromType(typeOfTile type)
    {
        switch(type)
        {
            case typeOfTile.WATER:
                return mapTiles[0];
            case typeOfTile.STONE:
                return mapTiles[1];
            case typeOfTile.DIRT:
                return mapTiles[2];
            case typeOfTile.GRASS:
                return mapTiles[3];
            case typeOfTile.TREE_S:
                return mapTiles[4];
            case typeOfTile.TREE_L:
                return mapTiles[5];
            default:
                return mapTiles[0];
        }
    }

    private float[,] generateFalloff()
    {
        float[,] falloffMap = new float[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float xv = x / (float)size * 2 - 1;
                float yv = y / (float)size * 2 - 1;
                float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
                falloffMap[x, y] = Mathf.Pow(v, 3f) / (Mathf.Pow(v, 3f) + Mathf.Pow(2.2f - 2.2f * v, 3f));
            }
        }
        return falloffMap;
    }

    private float[,] generatePerlin()
    {
        float[,] noiseMap = new float[size, size];
        float xOffset = UnityEngine.Random.Range(-10000f, 10000f);
        float yOffset = UnityEngine.Random.Range(-10000f, 10000f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x * scale + xOffset, y * scale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }
        return noiseMap;
    }

    private void generate()
    {
        UnityEngine.Random.InitState(seed.GetHashCode());
        tileMap.ClearAllTiles();
        size = UnityEngine.Random.Range(sizeMin, sizeMax);
        float[,] noiseMap = generatePerlin();
        float[,] falloffMap = generateFalloff();
        tiles = new TileType[size, size];
        dataFromTiles = new();
        map = new();
        possiblePlayerSpawns = new();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                TileType tile = new TileType();
                float noiseValue = noiseMap[x, y];
                noiseValue -= falloffMap[x, y];
                Vector3Int pos = new Vector3Int(x, y, 0);
                tile.type = getTypeFromNoise(noiseValue);
                tile.tile = getTileFromType(tile.type);
                tile.position = pos;
                tile.neighbours = new();
                dataFromTiles.Add(tile.position, tile); // Add to dictionary for retrieving values later
                map.Add(tile.position);
                tiles[x, y] = tile;
                tileMap.SetTile(pos, tile.tile);

                // Add tile to possiblePlayerSpawns if sand
                if (tile.type == typeOfTile.STONE)
                {
                    possiblePlayerSpawns.Add(tile.position);
                }
            }
        }
        getNeighbours();
        getPlayerSpawns(4);
        MapManager.instance.dataFromTiles = dataFromTiles;
        MapManager.instance.playerSpawns = playerSpawns;
        MapManager.instance.spawnPlayer(map[UnityEngine.Random.Range(0, map.Count)]);
    }

    private void getNeighbours()
    {
        foreach(TileType tile in tiles)
        {
            // Ignore water tiles, only get neighbours for land tiles
            if (tile.type == typeOfTile.WATER) { continue; }

            // Left, right, up down.
            Vector3Int left = new Vector3Int(tile.position.x + 1, tile.position.y, 0);
            Vector3Int right = new Vector3Int(tile.position.x - 1, tile.position.y, 0);
            Vector3Int up = new Vector3Int(tile.position.x, tile.position.y + 1, 0);
            Vector3Int down = new Vector3Int(tile.position.x, tile.position.y - 1, 0);

            if (tileMap.HasTile(left) && dataFromTiles.ContainsKey(left) && dataFromTiles[left].type != typeOfTile.WATER)
            {
                tile.neighbours.Add(dataFromTiles[left]);
            }

            if (tileMap.HasTile(right) && dataFromTiles.ContainsKey(right) && dataFromTiles[right].type != typeOfTile.WATER)
            {
                tile.neighbours.Add(dataFromTiles[right]);
            }

            if (tileMap.HasTile(up) && dataFromTiles.ContainsKey(up) && dataFromTiles[up].type != typeOfTile.WATER)
            {
                tile.neighbours.Add(dataFromTiles[up]);
            }

            if (tileMap.HasTile(down) && dataFromTiles.ContainsKey(down) && dataFromTiles[down].type != typeOfTile.WATER)
            {
                tile.neighbours.Add(dataFromTiles[down]);
            }
        }
    }

    private void getPlayerSpawns(int numPlayers)
    {
        playerMap.ClearAllTiles();
        playerSpawns = new();
        List<Vector3Int> playerLocations = new();

        // Find the closest map tiles to each edge of the map generated
        playerLocations.Add(findClosestTile(new Vector3Int(0, 0, 0), possiblePlayerSpawns));
        playerLocations.Add(findClosestTile(new Vector3Int(size, size, 0), possiblePlayerSpawns));
        playerLocations.Add(findClosestTile(new Vector3Int(size, 0, 0), possiblePlayerSpawns));
        playerLocations.Add(findClosestTile(new Vector3Int(0, size, 0), possiblePlayerSpawns));

        // Spawn Players based on possible locations at random
        for (int j = 0; j < numPlayers; j++)
        {
            var index = UnityEngine.Random.Range(0, playerLocations.Count);
            playerMap.SetTile(playerLocations[index], playerTile);
            playerSpawns.Add(playerLocations[index]);
            playerLocations.Remove(playerLocations[index]);
        }
    }

    private Vector3Int findClosestTile(Vector3Int cornerPos, List<Vector3Int> list)
    {
        Vector3Int closest = Vector3Int.zero;
        float smallestDist = 999999;
        for (int i = 0; i < list.Count; i++)
        {
            float dist = Vector3Int.Distance(cornerPos, list[i]);
            // Make sure there is at least 1 neighbour tile
            TileType tile = dataFromTiles.ContainsKey(list[i]) ? dataFromTiles[list[i]] : null;
            if (dist < smallestDist && tile != null && tile.neighbours.Count > 0)
            {
                smallestDist = dist;
                closest = list[i];
            }
        }
        return closest;
    }

    private bool distanceLargerThan(Vector3Int pos1, Vector3Int pos2, int dist)
    {
        var dPos = new Vector3Int(pos1.x - pos2.x, pos1.y - pos2.y, 0);
        return dPos.x > dist || dPos.x < -dist || dPos.y > dist || dPos.y < -dist;
    }

    private Vector3Int furthestFrom(Vector3Int position, List<Vector3Int> list)
    {
        float furthestDist = 0;
        Vector3Int furthestPosition = Vector3Int.zero;
        for(int i = 0; i < list.Count; i++)
        {
            float dist = Vector3Int.Distance(position, list[i]);
            if (dist > furthestDist)
            {
                furthestDist = dist;
                furthestPosition = list[i];
            }
        }
        Debug.Log(furthestPosition + " " + furthestDist);
        return furthestPosition;
    }
}
