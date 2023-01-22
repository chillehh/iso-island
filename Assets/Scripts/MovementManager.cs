using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementManager : MonoBehaviour
{
    public static MovementManager instance;

    MouseInput mouseInput;

    public float speed = 4.0f;
    
    private PathFinder pathFinder;
    private bool isMoving;
    private List<TileType> path;
    private List<TileType> rangeFinderTiles;

    private void OnEnable()
    {
        mouseInput.Enable();
        mouseInput.Mouse.mouseClick.performed += mousePressed;
    }

    private void OnDisable()
    {
        mouseInput.Mouse.mouseClick.performed -= mousePressed;
        mouseInput.Disable();
    }

    private void Awake()
    {
        if (instance == null) { instance = this; }
        mouseInput = new();
    }

    private void Start() {
        pathFinder = new();
        path = new List<TileType>();
        isMoving = false;
        rangeFinderTiles = new List<TileType>();
    }

    private void mousePressed(InputAction.CallbackContext ctx)
    {
        Vector3Int clickPos = MapManager.instance.getTilePosition(new Vector3(mouseInput.Mouse.mousePosition.ReadValue<Vector2>().x, mouseInput.Mouse.mousePosition.ReadValue<Vector2>().y, 0));
        Debug.Log("mouse clicked at: " + clickPos.ToString());
        TileType endPos = MapManager.instance.dataFromTiles[clickPos];
        TileType startPos = MapManager.instance.dataFromTiles[PlayerData.instance.position];
        path = pathFinder.findPath(startPos, endPos, rangeFinderTiles);
    }

    private void LateUpdate() {
        if (path.Count > 0)
        {
            Debug.Log("Moving along path" + path.Count);
            moveAlongPath();
        }
    }

    private void moveAlongPath()
    {
        var step = speed * Time.deltaTime;
        float zIndex = path[0].position.z;
        PlayerData.instance.transform.position = Vector2.MoveTowards(PlayerData.instance.transform.position, new Vector2(path[0].position.x, path[0].position.y), step);
        PlayerData.instance.transform.position = new Vector3(PlayerData.instance.transform.position.x, PlayerData.instance.transform.position.y, zIndex);
        if(Vector3Int.Distance(PlayerData.instance.position, path[0].position) < 1f)
        {
            positionPlayerOnLine(path[0]);
            path.RemoveAt(0);
        }
    }

    public void positionPlayerOnLine(TileType tile)
    {
        PlayerData.instance.transform.position = new Vector3(tile.position.x, tile.position.y+0.0001f, tile.position.z);
        PlayerData.instance.position = tile.position;
    }

}
