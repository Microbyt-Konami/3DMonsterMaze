using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MazeGenerator : MonoBehaviour
{
    public int rows = 10, columns = 10;
    public bool log;

    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject floorRock1Prefab;
    [SerializeField] private GameObject floorRock2Prefab;
    [SerializeField] private GameObject wallOreBluePrefab;
    [SerializeField] private GameObject wallOreGreenPrefab;
    [SerializeField] private GameObject wallOreRedPrefab;
    [SerializeField] private Material[] wallMaterials;
    [SerializeField] private Transform mazeParent;
    [SerializeField] private bool hideRoot;
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private NavMeshAgent navMeshAgent;

    [field: SerializeField, Header("Debug")] public bool MazeGenerated { get; private set; }
    [field: SerializeField] public NativeArray<CellWall> CellWalls;

    private EllerJob _ellerJob;
    private ConnectCellsToWallsJob _wallsJob;
    private FindAWayMazeJob _findAWayMazeJob;

    private NativeArray<CellConnect> _cells;
    private NativeArray<CellWall> _walls;
    public NativeArray<int> _wayResult;

    private JobHandle _mazeGeneratorJobHandle;
    private JobHandle _wallsJobHandle;
    private JobHandle _findAWayMazeJobHandle;

    [SerializeField] private int nWallsMaterialsChanged;

    public void GenerateMaze()
    {
        MazeGenerated = false;
        nWallsMaterialsChanged = 0;
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
        //_findAWayMazeJob = new FindAWayMazeJob
        //{
        //    Rows = rows,
        //    Columns = columns,
        //    Log = log,
        //    Walls = _walls,
        //    ColIni = -1,
        //    RowIni = 0,
        //    ColEnd = -1,
        //    RowEnd = rows - 1,
        //    WayResult = new NativeList<int>(rows * 2 * columns, Allocator.TempJob),
        //    MaxWayItem = rows * 2 * columns
        //};

        _mazeGeneratorJobHandle = _ellerJob.Schedule();
        _wallsJobHandle = _wallsJob.Schedule(_mazeGeneratorJobHandle);
        //_findAWayMazeJobHandle = _findAWayMazeJob.Schedule(_wallsJobHandle);

        StartCoroutine(WaitGenerateMazeCoRoutine());
    }

    private void Start()
    {
        navMeshSurface = FindAnyObjectByType<NavMeshSurface>();
    }

    private void OnDestroy()
    {
        DisposeMemoryTemporal();
    }

    private IEnumerator WaitGenerateMazeCoRoutine()
    {
        //var handle = _findAWayMazeJobHandle;
        var handle = _wallsJobHandle;

        yield return new WaitUntil(() => handle.IsCompleted);

        handle.Complete();
        //_findAWayMazeJob.Execute();

        CellWalls = new NativeArray<CellWall>(_walls, Allocator.Persistent);
        //_wayResult = _findAWayMazeJob.WayResult.AsArray();
        _walls.Dispose();
        _cells.Dispose();

        yield return CreateMazeCellsCoRoutine();

        navMeshSurface.BuildNavMesh();

        var path = navMeshAgent.path;
        Color c = Color.magenta;

        navMeshAgent.destination = GameObject.Find("cell_9_9").transform.position;

        Debug.Log($"Path Length: {path.corners.Length}");

        if (path.corners.Length >= 2)
        {
            switch (path.status)
            {
                case NavMeshPathStatus.PathComplete:
                    c = Color.white;
                    break;
                case NavMeshPathStatus.PathInvalid:
                    c = Color.red;
                    break;
                case NavMeshPathStatus.PathPartial:
                    c = Color.yellow;
                    break;
            }

            Vector3 previousCorner = path.corners[0];

            int i = 1;
            while (i < path.corners.Length)
            {
                Vector3 currentCorner = path.corners[i];
                Debug.DrawLine(previousCorner, currentCorner, c);
                previousCorner = currentCorner;
                i++;
            }
        }


        //if (_wayResult.IsCreated)
        //    _wayResult.Dispose();

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

        cell.name = $"cell_{row}_{col}";
        cellScript.row = row;
        cellScript.column = col;
        //if (_wayResult.Contains(row * columns + col))
        //    cellScript.thisWay.SetActive(true);

        if (wall.HasFlag(CellWall.East))
        {
            CreateWall(cellScript, CellWall.East, cellScript.walls, "wallEast", cellScript.wallEastPoint.position,
                cellScript.wallEastPoint.rotation);
        }

        if (wall.HasFlag(CellWall.West))
        {
            CreateWall(cellScript, CellWall.West, cellScript.walls, "wallWest", cellScript.wallWestPoint.position,
                cellScript.wallWestPoint.rotation);
        }

        if (wall.HasFlag(CellWall.North))
        {
            CreateWall(cellScript, CellWall.North, cellScript.walls, "wallNorth", cellScript.wallNorthPoint.position,
                cellScript.wallNorthPoint.rotation);
        }

        if (wall.HasFlag(CellWall.South))
        {
            CreateWall(cellScript, CellWall.South, cellScript.walls, "wallSouth", cellScript.wallSouthPoint.position,
                cellScript.wallSouthPoint.rotation);
        }

        AddFloorRocks(cellScript.floor);
        if (hideRoot)
            cellScript.root.SetActive(false);
    }

    // Replaced the TODO comment with the implementation
    private Wall CreateWall(Cell cell, CellWall cellWall, Transform wallParent, string wallName,
        Vector3 wallPosition, Quaternion wallRotation)
    {
        var wallGO = Instantiate(wallPrefab, wallPosition, wallRotation, wallParent);
        var wall = wallGO.GetComponent<Wall>();

        wallGO.name = wallName;
        AddWallsOre(wall);
        if (Random.value < 0.05f)
            ChangeWallMaterial(cell, wall);

        return wall;
    }

    private void AddFloorRocks(Floor floor)
    {
        CreateSpawnerItem(floor.gameObject, floor.spawnerRock1, floorRock1Prefab);
        CreateSpawnerItem(floor.gameObject, floor.spawnerRock2, floorRock2Prefab);
    }

    private void AddWallsOre(Wall wall)
    {
        var wallGO = wall.gameObject;

        CreateSpawnerItem(wallGO, wall.spawnerOreBlue, wallOreBluePrefab);
        CreateSpawnerItem(wallGO, wall.spawnerOreGreen, wallOreGreenPrefab);
        CreateSpawnerItem(wallGO, wall.spawnerOreRed, wallOreRedPrefab);
    }

    private void ChangeWallMaterial(Cell cell, Wall wall)
    {
        if (cell.row == 0 || cell.row == rows - 1 || cell.column == 0 || cell.column == columns - 1)
            return;

        var materialIndex = Random.Range(0, wallMaterials.Length);
        var material = wallMaterials[materialIndex];

        wall.SetWallMaterial(material);
        Debug.Log($"Change wall Material {wall.transform.parent.parent.gameObject}", wall.transform.parent.parent.gameObject);
        nWallsMaterialsChanged++;
    }

    private void CreateSpawnerItem(GameObject objeto, SpawnerItemsCell spawnerItem, GameObject itemPrefab)
    {
        var count = Random.Range(spawnerItem.minItems, spawnerItem.maxItems);

        for (int i = 0; i < count; i++)
        {
            var position = new Vector3(Random.Range(spawnerItem.minX, spawnerItem.maxX),
                Random.Range(spawnerItem.minY, spawnerItem.maxY), Random.Range(spawnerItem.minZ, spawnerItem.maxZ));
            var item = Instantiate(itemPrefab, spawnerItem.transform.TransformPoint(position), Quaternion.identity,
                spawnerItem.transform);

            item.name = $"{itemPrefab.name}_{i}";
        }
    }

    private void DisposeMemoryTemporal()
    {
        if (_cells.IsCreated)
            _cells.Dispose();
        if (_walls.IsCreated)
            _walls.Dispose();
        if (_wayResult.IsCreated)
            _wayResult.Dispose();
        if (CellWalls.IsCreated)
            CellWalls.Dispose();
    }
}