using Unity.Collections;
using Unity.Jobs;

public struct InOutMazeJob : IJob
{
    public int Rows, Columns;
    public NativeArray<CellWall> Walls;
    public int ColIn, ColOut;

    public void Execute()
    {
    }
}