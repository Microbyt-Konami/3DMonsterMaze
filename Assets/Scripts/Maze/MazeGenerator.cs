using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeGenerator : MonoBehaviour
{
    public int rows = 10, columns = 10;
    public bool log;

    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private Transform mazeParent;
    [SerializeField] private bool hideRoot;
    [field: SerializeField] public bool MazeGenerated { get; private set; }
    [field: SerializeField] public NativeArray<CellWall> CellWalls;

    private EllerJob _ellerJob;
    private ConnectCellsToWallsJob _wallsJob;
    private FindAWayMazeJob _findAWayMazeJob;

    private NativeArray<CellConnect> _cells;
    private NativeArray<CellWall> _walls;

    private JobHandle _mazeGeneratorJobHandle;
    private JobHandle _wallsJobHandle;
    private JobHandle _findAWayMazeJobHandle;

    public void GenerateMaze()
    {
        MazeGenerated = false;
        _cells = new NativeArray<CellConnect>(rows * columns, Allocator.TempJob);
        _walls = new NativeArray<CellWall>(rows * columns, Allocator.TempJob);

        _ellerJob = new EllerJob
        {
            Seed = (uint)Random.Range(int.MinValue, int.MaxValue),
            Log = log,
            Rows = rows,
            Columns = columns,
            Cells = _cells,
        };
        _wallsJob = new ConnectCellsToWallsJob
        {
            Rows = rows,
            Columns = columns,
            Log = log,
            Cells = _ellerJob.Cells,
            Walls = _walls,
        };
        _findAWayMazeJob = new FindAWayMazeJob
        {
            Rows = rows,
            Columns = columns,
            Walls = _wallsJob.Walls,
        };

        _mazeGeneratorJobHandle = _ellerJob.Schedule();
        _wallsJobHandle = _wallsJob.Schedule(_mazeGeneratorJobHandle);
        _findAWayMazeJobHandle = _findAWayMazeJob.Schedule(_wallsJobHandle);

        StartCoroutine(WaitGenerateMazeCoRoutine());
    }

    private void OnDestroy()
    {
        DisposeMemoryTemporal();
    }

    private IEnumerator WaitGenerateMazeCoRoutine()
    {
        var handle = _findAWayMazeJobHandle;

        yield return new WaitUntil(() => handle.IsCompleted);

        handle.Complete();

        CellWalls = new NativeArray<CellWall>(_walls, Allocator.Persistent);
        _walls.Dispose();
        _cells.Dispose();

        yield return CreateMazeCellsCoRoutine();

        // var position = new Vector3(10, 0, 10);
        // var cell = Instantiate(cellPrefab, position, Quaternion.identity);
        // var cellScript = cell.GetComponent<Cell>();
        // var wallEast = Instantiate(wallPrefab,
        //     cellScript.wallEastPoint.position /*position + new Vector3(2.05f, 1.95f, 4f)*/,
        //     cellScript.wallEastPoint.rotation /* Quaternion.Euler(0, 0, 90)*/,
        //     cellScript.walls);
        // var wallWest = Instantiate(wallPrefab,
        //     cellScript.wallWestPoint.position /*position + new Vector3(-2.05f, 1.95f, 4f)*/,
        //     cellScript.wallWestPoint.rotation /*Quaternion.Euler(0, 0, 90)*/,
        //     cellScript.walls);
        // var wallNorth = Instantiate(wallPrefab,
        //     cellScript.wallNorthPoint.position /*position + new Vector3(4f, 1.95f, 2.05f)*/,
        //     cellScript.wallNorthPoint.rotation /*Quaternion.Euler(0, 0, 0)*/,
        //     cellScript.walls);
        // var wallSouth = Instantiate(wallPrefab,
        //     cellScript.wallSouthPoint.position /*position + new Vector3(4f, 1.95f, -2.05f)*/,
        //     cellScript.wallSouthPoint.rotation /*Quaternion.Euler(0, 0, 0)*/,
        //     cellScript.walls);
        //
        // cell.name = "cellTest";
        // wallEast.name = "wallEast";
        // wallWest.name = "wallWest";
        // wallNorth.name = "wallNorth";
        // wallSouth.name = "wallSouth";
        MazeGenerated = true;
    }

    private IEnumerator CreateMazeCellsCoRoutine()
    {
        for (int i = 0, idx = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
                CreateCell(i, j, CellWalls[idx++]);
        }

        yield break;
    }

    private void CreateCell(int row, int col, CellWall wall)
    {
        var position = new Vector3(4 * row, 0, 4 * col);
        var cell = Instantiate(cellPrefab, position, Quaternion.identity, mazeParent);
        var cellScript = cell.GetComponent<Cell>();

        if (col == columns - 1)
        {
            var wallEast = Instantiate(wallPrefab,
                cellScript.wallEastPoint.position /*position + new Vector3(2.05f, 1.95f, 4f)*/,
                cellScript.wallEastPoint.rotation /* Quaternion.Euler(0, 0, 90)*/,
                cellScript.walls);
            wallEast.name = "wallEast";
        }

        if (col == 0)
        {
            var wallWest = Instantiate(wallPrefab,
                cellScript.wallWestPoint.position /*position + new Vector3(-2.05f, 1.95f, 4f)*/,
                cellScript.wallWestPoint.rotation /*Quaternion.Euler(0, 0, 90)*/,
                cellScript.walls);
            wallWest.name = "wallWest";
        }

        if (row == 0)
        {
            var wallNorth = Instantiate(wallPrefab,
                cellScript.wallNorthPoint.position /*position + new Vector3(4f, 1.95f, 2.05f)*/,
                cellScript.wallNorthPoint.rotation /*Quaternion.Euler(0, 0, 0)*/,
                cellScript.walls);
            wallNorth.name = "wallNorth";
        }

        if (row == rows - 1)
        {
            var wallSouth = Instantiate(wallPrefab,
                cellScript.wallSouthPoint.position /*position + new Vector3(4f, 1.95f, -2.05f)*/,
                cellScript.wallSouthPoint.rotation /*Quaternion.Euler(0, 0, 0)*/,
                cellScript.walls);
            wallSouth.name = "wallSouth";
        }

        cell.name = $"cell_{row}_{col}";
        if (hideRoot)
            cellScript.root.SetActive(false);
    }

    private void DisposeMemoryTemporal()
    {
        _cells.Dispose();
        _walls.Dispose();
        CellWalls.Dispose();
    }
}