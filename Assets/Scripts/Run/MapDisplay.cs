using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapDisplay : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private Transform container;

    [SerializeField] private GameObject nodeImagePrefab;
    [SerializeField] private float cellSize;

    private Vector2Int currentTile;
    private Dictionary<Vector2Int, Image> tileDict;
    private Dictionary<Vector2Int, bool> completionDict;
    private bool isActive;

    private void Awake()
    {
        isActive = false;
        cg.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        RunManager.Instance.onPlayerRoomChanged -= HandlePlayerRoomChanged;
    }

    private void Update()
    {
#if UNITY_EDITOR

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HandleMapOpen();
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            HandleMapClose();
        }
#endif
    }

    private void HandleMapOpen()
    {
        if (isActive) return;
        isActive = true;
        cg.gameObject.SetActive(true);
    }

    private void HandleMapClose()
    {
        if (!isActive) return;
        isActive = false;
        cg.gameObject.SetActive(false);
    }

    private void HandlePlayerRoomChanged(Vector2Int newTile)
    {
        tileDict[currentTile].color = completionDict[currentTile] ? Color.gray6 : Color.gray3;
        tileDict[newTile].color = Color.white;

        currentTile = newTile;
    }

    public void HandleRoomCleared(Vector2Int tile) => completionDict[tile] = true;

    public void InitializeMapView(List<Vector2Int> tiles)
    {
        RunManager.Instance.onPlayerRoomChanged += HandlePlayerRoomChanged;

        foreach (Transform child in container)
            Destroy(child.gameObject);

        tileDict = new();
        completionDict = new();

        for (int i = 0; i < tiles.Count; i++)
        {
            var color = Color.gray3;
            SpawnTile(tiles[i], color);
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
