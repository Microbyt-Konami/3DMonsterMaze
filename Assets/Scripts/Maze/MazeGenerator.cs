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
    [field: SerializeField] public bool MazeGenerated { get; private set; }
    [field: SerializeField] public NativeArray<CellWall> Walls;

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
        Walls = new NativeArray<CellWall>(_walls, Allocator.Persistent);
        DisposeMemoryTemporal();
        MazeGenerated = true;
    }

    private void DisposeMemoryTemporal()
    {
        _cells.Dispose();
        _walls.Dispose();
    }
}