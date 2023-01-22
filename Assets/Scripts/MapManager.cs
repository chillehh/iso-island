using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    public Dictionary<Vector3Int, TileType> dataFromTiles;
    public List<Vector3Int> playerSpawns;
    public GameObject playerPrefab;
    private PlayerData player;
    private Grid grid;

    private void Awake() {
        if (instance == null) { instance = this; }
    }

    private void Start() {
        grid = GetComponent<Grid>();
    }

    public void spawnPlayer(Vector3Int pos) {
        Debug.Log("spawning..");
        player = Instantiate(playerPrefab).GetComponent<PlayerData>();
        MovementManager.instance.positionPlayerOnLine(dataFromTiles[pos]);
    }

    public List<TileType> getTilesInRange(Vector3Int start, int range) {
        List<TileType> returnPositions = new();
        TileType baseTile = dataFromTiles[start];
        List<TileType> previousStepPositions = new();
        int stepCount = 0;
        previousStepPositions.Add(baseTile);

        while (stepCount < range) {
            List<TileType> neighbourTiles = new();
            foreach (TileType tile in previousStepPositions) {
                neighbourTiles.AddRange(tile.neighbours);
            }
            returnPositions.AddRange(neighbourTiles);
            previousStepPositions = neighbourTiles.Distinct().ToList();
            stepCount++;
        }
        return returnPositions.Distinct().ToList();
    }

    public Vector3Int getTilePosition(Vector3 mousePos)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        return grid.WorldToCell(new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0));
    }
}
