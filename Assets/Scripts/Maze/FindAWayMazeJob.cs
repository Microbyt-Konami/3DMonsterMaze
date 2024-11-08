using Unity.Collections;
using Unity.Jobs;

public struct FindAWayMazeJob : IJob
{
    public int Rows, Columns;
    public NativeArray<CellWall> Walls;
    // -1 cualquier fila c columna
    public int ColIni, ColFin;
    public int RowIni, RowFin;

    public void Execute()
    {
    }
}