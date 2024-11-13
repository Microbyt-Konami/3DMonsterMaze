using Unity.Collections;
using Unity.Jobs;

public struct FindAWayMazeJob : IJob
{
    [ReadOnly] public int Rows, Columns;

    [ReadOnly] public NativeArray<CellWall> Walls;

    // -1 cualquier fila c columna
    [ReadOnly] public int ColIni, ColFin;
    [ReadOnly] public int RowIni, RowFin;

    public void Execute()
    {
    }
}