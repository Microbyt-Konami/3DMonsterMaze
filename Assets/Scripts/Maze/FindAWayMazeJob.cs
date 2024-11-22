using System;
using System.Drawing.Text;
using Unity.Collections;
using Unity.Jobs;

using Debug = UnityEngine.Debug;

public struct WayCollection : IDisposable
{
    public void Dispose()
    {
        
    }
}

public struct WayItem
{
    public int cell;
}


public struct FindAWayMazeJob : IJob
{
    [ReadOnly] public int Rows, Columns;

    [ReadOnly] public NativeArray<CellWall> Walls;

    // -1 cualquier fila c columna
    [ReadOnly] public int ColIni, ColFin;
    [ReadOnly] public int RowIni, RowFin;

    public int ColResult, RowResult;
    [WriteOnly] public NativeList<int> WayResult;
    public int WayCountResult;
    public WayItem WayItemCurrent;

    //private int _cellWayItemCurrent;
    //private CellWall _celWallsWayItemCurrent, wallCurrentWayItemCurrent;
    //private bool _isWayWayItemCurrent;
    //private NativeStream _wayItemStream;
    //private NativeStream.Reader _wayItemReader;
    //private NativeStream.Writer _wayItemWriter;

    public void Execute()
    {

    }


}