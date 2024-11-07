using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int rows = 10, columns = 10;
    public bool log;
    public bool MazeGenerated { get; private set; }

    private EllerJob _ellerJob;
    private ConnectCellsToWallsJob _wallsJob;
    private JobHandle _mazeGeneratorJobHandle;
    private JobHandle _wallsJobHandle;

    public void GenerateMaze()
    {
        MazeGenerated = false;

        _ellerJob = new EllerJob
        {
            Seed = (uint)Random.Range(int.MinValue, int.MaxValue),
            Log = log,
            Rows = rows,
            Columns = columns,
            Cells = new NativeArray<CellConnect>(rows * columns, Allocator.TempJob),
        };
        _wallsJob = new ConnectCellsToWallsJob
        {
            Rows = rows,
            Columns = columns,
            Cells = _ellerJob.Cells,
            Walls = new NativeArray<CellWall>(rows * columns, Allocator.TempJob),
        };

        _mazeGeneratorJobHandle = _ellerJob.Schedule();
        _wallsJobHandle = _wallsJob.Schedule(_mazeGeneratorJobHandle);

        StartCoroutine(WaitGenerateMazeCoRoutine());
    }

    private IEnumerator WaitGenerateMazeCoRoutine()
    {
        yield return new WaitUntil(() => _wallsJobHandle.IsCompleted);

        _wallsJobHandle.Complete();
        MazeGenerated = true;
    }
}