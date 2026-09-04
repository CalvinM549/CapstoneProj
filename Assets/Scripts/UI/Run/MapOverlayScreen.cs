using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapOverlayScreen : UIScreen
{
    [SerializeField] private Transform container;

    [SerializeField] private GameObject nodeImagePrefab;
    [SerializeField] private float cellSize;

    private Vector2Int currentTile;
    private Dictionary<Vector2Int, Image> tileDict;
    private Dictionary<Vector2Int, bool> completionDict;


    private void OnEnable()
    {
        RunManager.Instance.onPlayerRoomChanged += HandlePlayerRoomChanged;
    }

    private void OnDisable()
    {
        RunManager.Instance.onPlayerRoomChanged -= HandlePlayerRoomChanged;
    }

    protected override void OnBeforeOpen(object payload)
    {
        var tiles = RunManager.Instance.currentRun.map.tiles;
        InitializeMapView(tiles);
    }
    
    private void HandlePlayerRoomChanged(Vector2Int newTile)
    {
        var runTiles = RunManager.Instance.currentRun.map.tiles;
        tileDict[currentTile].color = runTiles[currentTile].cleared ? Color.gray6 : Color.gray3;
        tileDict[newTile].color = Color.white;

        currentTile = newTile;
    }

    public void InitializeMapView(Dictionary<Vector2Int, RoomNode> tiles)
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);

        tileDict = new();
        completionDict = new();

        foreach (var tile in tiles)
        {
            var color = Color.gray3;
            SpawnTile(tile.Key, color);
        }
    }

    private void SpawnTile(Vector2Int gridPos, Color color)
    {
        Vector3 spawnPos = new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0f);

        GameObject newTile = Instantiate(nodeImagePrefab, container);
        var image = newTile.GetComponent<Image>();
        image.color = color;
        newTile.transform.localPosition = spawnPos;

        completionDict[gridPos] = false;
        tileDict[gridPos] = image;
    }
}
