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
    [field: SerializeField] public bool MazeGenerated { get; private set; }

    private EllerJob _ellerJob;
    private ConnectCellsToWallsJob _wallsJob;
    private InOutMazeJob _inOutMazeJob;
    private JobHandle _mazeGeneratorJobHandle;
    private JobHandle _wallsJobHandle;
    private JobHandle _inOutMazeJobHandle;

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
        _inOutMazeJob = new InOutMazeJob
        {
            Rows = rows,
            Columns = columns,
            Walls = _wallsJob.Walls,
        };

        _mazeGeneratorJobHandle = _ellerJob.Schedule();
        _wallsJobHandle = _wallsJob.Schedule(_mazeGeneratorJobHandle);
        _inOutMazeJobHandle = _inOutMazeJob.Schedule(_wallsJobHandle);

        StartCoroutine(WaitGenerateMazeCoRoutine());
    }

    private IEnumerator WaitGenerateMazeCoRoutine()
    {
        var handle = _inOutMazeJobHandle;

        yield return new WaitUntil(() => handle.IsCompleted);

        handle.Complete();
        MazeGenerated = true;
    }
}