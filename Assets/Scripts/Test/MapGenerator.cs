using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public MapGenerationService generation;

    public GameDatabase db;

    [SerializeField] private Vector2Int startPosition = Vector2Int.zero;

    //[SerializeField] private int iterations;
    [SerializeField] private int walkLength;
    [SerializeField] private Vector2Int mapBounds;
    
    [SerializeField] private bool allowOverlap = false;

    [SerializeField] private float delay;

    private HashSet<Vector2Int> visited = new();
    private Coroutine walkRoutine;

    [SerializeField] private GameObject displayNode;
    [SerializeField] private LineRenderer pathLine;

    private void Start()
    {
        generation = new(db.rooms, mapBounds, walkLength, 0, 1f, 0.3f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RunGeneration();
        }
    }

    public void RunGeneration()
    {
        //RunRandomWalk();

        ResetWalk();

        visited = generation.RunSimpleRandomWalk();

        walkRoutine = StartCoroutine(AnimateWalk());
    }

    private void RunRandomWalk()
    {


        Vector2Int currentPos = startPosition;
        visited.Add(currentPos);

        //SpawnTile(currentPos);

        int currentStep = 0;

        while (currentStep < walkLength)
        {
            Vector2Int randomDirection = DirectionExtensions.Random().ToGridOffset();
            Vector2Int nextPos = currentPos + randomDirection;

            if (allowOverlap)
            {
                currentPos = nextPos;
                visited.Add(nextPos);
                currentStep++;

                //SpawnTile(currentPos);
            }

            else
            {
                if (!visited.Contains(nextPos))
                {
                    currentPos = nextPos;
                    visited.Add(nextPos);
                    currentStep++;

                    //SpawnTile(currentPos);
                }

                else if (IsTrapped(currentPos))
                {
                    currentPos = GetRandomVisited();
                }
            }
        }

        walkRoutine = StartCoroutine(AnimateWalk());
    }

    private IEnumerator AnimateWalk()
    {
        if (displayNode == null) yield break;

        List<Vector2Int> list = new(visited);
        for (int i = 0; i < list.Count; i++)
        {
            SpawnTile(list[i], Color.Lerp(Color.green, Color.red, ((float)i / (float)(list.Count-1))));
            yield return new WaitForSeconds(delay);
        }
    }

    private void SpawnTile(Vector2Int gridPos, Color colour)
    {
        Vector2 spawnWorldPos = new Vector2(gridPos.x, gridPos.y);
        GameObject newTile = Instantiate(displayNode, spawnWorldPos, Quaternion.identity);
        newTile.GetComponent<SpriteRenderer>().color = colour;
        newTile.transform.localScale = Vector3.one * 0.9f;
        newTile.transform.parent = this.transform;
    }

    private void ResetWalk()
    {
        visited.Clear();
        if (walkRoutine != null)
            StopCoroutine(walkRoutine);

        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    private bool IsTrapped(Vector2Int pos)
    {
        if (!visited.Contains(pos + Direction.North.ToGridOffset()))
            return false;
        if (!visited.Contains(pos + Direction.South.ToGridOffset()))
            return false;
        if (!visited.Contains(pos + Direction.East.ToGridOffset()))
            return false;
        if (!visited.Contains(pos + Direction.West.ToGridOffset()))
            return false;

        return true;
    }

    private Vector2Int GetRandomVisited()
    {
        List<Vector2Int> list = new(visited);
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
}
