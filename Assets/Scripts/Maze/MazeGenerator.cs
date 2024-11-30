using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;
using static EntryExitMazeJob;

using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class MazeGenerator : MonoBehaviour
{
    public int rows = 10, columns = 10;
    public bool debug;

    [SerializeField] private GameObject mazePrefab;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject floorRock1Prefab;
    [SerializeField] private GameObject floorRock2Prefab;
    [SerializeField] private GameObject wallOreBluePrefab;
    [SerializeField] private GameObject wallOreGreenPrefab;
    [SerializeField] private GameObject wallOreRedPrefab;
    [SerializeField] private Material[] wallMaterials;
    [SerializeField] private Transform containerParent;
    [SerializeField] private bool hideRoot;
    [SerializeField] private NavMeshAgent navMeshAgent;

    [field: SerializeField, Header("Debug")] public bool MazeGenerated { get; private set; }
    //[field: SerializeField] public NativeArray<CellWall> CellWalls;
    [field: SerializeField] public GameObject MazeGO { get; private set; }
    [SerializeField] private NavMeshSurface navMeshSurface;
    [field: SerializeField] public GameObject CellEntryGO { get; private set; }
    [field: SerializeField] public GameObject CellExitGO { get; private set; }
    [SerializeField] private int nWallsMaterialsChanged;


    private EllerJob _ellerJob;
    private ConnectCellsToWallsJob _wallsJob;
    private EntryExitMazeJob _entryExitMazeJob;

    private NativeArray<CellConnect> _cells;
    private NativeArray<CellWall> _walls;
    private NativeList<EnTryExitCols> _entryExitCols;
    public NativeArray<int> _wayResult;

    private JobHandle _mazeGeneratorJobHandle;
    private JobHandle _wallsJobHandle;
    private JobHandle _entryExitMazeJobHandle;

    private int colEntry, colExit;

    public Coroutine GenerateMaze()
    {
        MazeGenerated = false;
        nWallsMaterialsChanged = 0;
        _cells = new NativeArray<CellConnect>(rows * columns, !debug ? Allocator.TempJob : Allocator.Persistent);
        _walls = new NativeArray<CellWall>(rows * columns, !debug ? Allocator.TempJob : Allocator.Persistent);
        _entryExitCols = new NativeList<EnTryExitCols>(columns, !debug ? Allocator.TempJob : Allocator.Persistent);

        _ellerJob = new EllerJob
        {
            Seed = (uint)Random.Range(int.MinValue, int.MaxValue),
            Debug = debug,
            Rows = rows,
            Columns = columns,
            Cells = _cells,
        };
        _wallsJob = new ConnectCellsToWallsJob
        {
            Rows = rows,
            Columns = columns,
            Debug = debug,
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
        _entryExitMazeJob = new EntryExitMazeJob
        {
            Rows = rows,
            Columns = columns,
            Debug = debug,
            Cells = _cells,
            EntryExitCols = _entryExitCols
        };

        if (debug)
        {
            _ellerJob.Execute();
            _wallsJob.Execute();
            //_findAWayMazeJob.Execute(_wallsJob);
            _entryExitMazeJob.Execute();
        }
        else
        {
            _mazeGeneratorJobHandle = _ellerJob.Schedule();
            _wallsJobHandle = _wallsJob.Schedule(_mazeGeneratorJobHandle);
            //_findAWayMazeJobHandle = _findAWayMazeJob.Schedule(_wallsJobHandle);
            _entryExitMazeJobHandle = _entryExitMazeJob.Schedule(_wallsJobHandle);
        }

        return StartCoroutine(WaitGenerateMazeCoRoutine());
    }

    private void OnDestroy()
    {
        DisposeMemoryTemporal();
    }

    private IEnumerator WaitGenerateMazeCoRoutine()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        if (!debug)
        {
            var handle = _entryExitMazeJobHandle;

            yield return new WaitUntil(() => handle.IsCompleted);

            handle.Complete();
        }

        _cells.Dispose();

        if (_entryExitCols.Length > 0)
        {
            var entryExit = _entryExitMazeJob.EntryExitCols[Random.Range(0, _entryExitMazeJob.EntryExitCols.Length)];

            colEntry = Random.Range(entryExit.colEntryIni, entryExit.colEntryEnd);
            colExit = Random.Range(entryExit.colExitIni, entryExit.colExitEnd);
        }
        else
            colEntry = colExit = -1;

        _entryExitCols.Dispose();

        yield return CreateMazeCellsCoRoutine();

        navMeshSurface.BuildNavMesh();

        CellEntryGO = colEntry >= 0 ? FindCellGO(0, colEntry) : null;
        CellExitGO = colExit >= 0 ? FindCellGO(rows - 1, colExit) : null;

        _walls.Dispose();

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
        stopwatch.Stop();
        Debug.LogWarning($"Maze generation time: {stopwatch.ElapsedMilliseconds} ms");
        MazeGenerated = true;
    }

    private IEnumerator CreateMazeCellsCoRoutine()
    {
        MazeGO = Instantiate(mazePrefab, Vector3.zero, Quaternion.identity, containerParent);
        navMeshSurface = MazeGO.GetComponent<NavMeshSurface>();

        for (int i = 0, idx = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
                CreateCell(i, j, _walls[idx++]);
        }

        yield break;
    }

    private void CreateCell(int row, int col, CellWall wall)
    {
        var position = new Vector3(4 * row, 0, 4 * col);
        var cell = Instantiate(cellPrefab, position, Quaternion.identity, MazeGO.transform);
        var cellScript = cell.GetComponent<Cell>();

        cell.name = GetCellName(row, col);
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
            if (!(row == rows - 1 && col == colExit))
                CreateWall(cellScript, CellWall.South, cellScript.walls, "wallSouth", cellScript.wallSouthPoint.position,
                    cellScript.wallSouthPoint.rotation);
        }

        AddFloorRocks(cellScript.floor);
        if (hideRoot)
            cellScript.root.SetActive(false);
    }

    private string GetCellName(int row, int col)
    {
        return $"cell_{row}_{col}";
    }

    private GameObject FindCellGO(int row, int col) => GameObject.Find(GetCellName(row, col));

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
    }
}