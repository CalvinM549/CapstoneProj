using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapOverlayScreen : UIScreen
{
    [SerializeField] private Transform mapContainer;

    [SerializeField] private GameObject nodeImagePrefab;
    [SerializeField] private float cellSize;

    private Vector2Int currentTile;
    private Dictionary<Vector2Int, Image> tileDict;


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
        var map = RunManager.Instance.currentRun.map;
        InitializeMapView(map);
    }
    
    private void HandlePlayerRoomChanged(Vector2Int newTile)
    {
        var runTiles = RunManager.Instance.currentRun.map;
        tileDict[currentTile].color = Color.gray6;
        tileDict[newTile].color = Color.white;

        currentTile = newTile;
    }

    public void InitializeMapView(SectorMap activeMap)
    {
        foreach (Transform child in mapContainer)
            Destroy(child.gameObject);

        tileDict = new();

        var tiles = activeMap.nodes;
        currentTile = activeMap.currentNode.coordinates;
        
        foreach (var tile in tiles)
        {
            var colour = tile.cleared ? Color.gray6 : Color.gray3;
            if(tile.coordinates == currentTile) colour = Color.white;
            SpawnTile(tile.coordinates, colour);
        }
    }

    private void SpawnTile(Vector2Int gridPos, Color color)
    {
        Vector3 spawnPos = new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0f);

        GameObject newTile = Instantiate(nodeImagePrefab, mapContainer);
        var image = newTile.GetComponent<Image>();
        image.color = color;
        newTile.transform.localPosition = spawnPos;

        tileDict[gridPos] = image;
    }
}
