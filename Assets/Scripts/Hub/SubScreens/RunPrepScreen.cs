using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RunPrepScreen : HubScreen
{
    public override HubState ScreenType => HubState.RunPrep;
    private int tempSeed; // Generated seed (can be overriden)

    [SerializeField] private Transform mapOrigin;
    
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private float delay;
    private Coroutine walkRoutine;

    private List<GameObject> spawnedTiles = new();


    public override void Initialize()
    {
        base.Initialize();

        HubManager.Instance.onMapGenerated += HandleMapGenerated;
    }

    private void OnDisable()
    {
        if (HubManager.Instance != null)
            HubManager.Instance.onMapGenerated -= HandleMapGenerated;
    }

    public override void OnClose()
    {

    }

    public override void OnOpen(HubManager hub)
    {
        manager = hub;

        //display etc
    }

    public void RegenMap()
    {
        ClearCurrent();

        HubManager.Instance.GenerateNewMap();
    }

    private void ClearCurrent()
    {
        if (walkRoutine != null)
            StopCoroutine(walkRoutine);


        foreach (var tile in spawnedTiles)
            Destroy(tile);

        spawnedTiles.Clear();
    }

    private void HandleMapGenerated(RunMap map)
    {
        ClearCurrent();

        List<Vector2Int> tiles = map.tiles.Keys.ToList();

        walkRoutine = StartCoroutine(AnimateWalk(tiles));
    }

    private IEnumerator AnimateWalk(List<Vector2Int> map)
    {
        if (tilePrefab == null) yield break;

        List<Vector2Int> list = new(map);
        for (int i = 0; i < list.Count; i++)
        {
            SpawnTile(list[i], Color.Lerp(Color.green, Color.red, ((float)i / (float)(list.Count - 1))));
            yield return new WaitForSeconds(delay);
        }
    }

    private void SpawnTile(Vector2Int gridPos, Color colour)
    {
        Vector2 spawnWorldPos = new Vector2(gridPos.x, gridPos.y);
        GameObject newTile = Instantiate(tilePrefab, mapOrigin);
        spawnedTiles.Add(newTile);

        newTile.transform.localPosition = spawnWorldPos;
        newTile.GetComponent<SpriteRenderer>().color = colour;
        newTile.transform.localScale = Vector3.one * 0.9f;
        newTile.transform.parent = this.transform;
    }

    #region Button functions



    #endregion
}
