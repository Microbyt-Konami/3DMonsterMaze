using System;
using Unity.Collections;
using Unity.Jobs;

[Flags]
public enum CellWall
{
    None = 0x0,
    North = 0x1,
    East = 0x2,
    South = 0x4,
    West = 0x8
}

public struct ConnectCellsToWallsJob : IJob
{
    public int Rows, Columns;
    public NativeArray<CellConnect> Cells;
    public NativeArray<CellWall> Walls;

    public void Execute()
    {
        int idx = 0;

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                var cellConnect = Cells[idx];
                var walls = CellWall.None;

                if (!cellConnect.HasFlag(CellConnect.Right) || j == Columns - 1)
                    walls |= CellWall.East;
                if (i == 0 || !Cells[idx - 1].HasFlag(CellConnect.Right))
                    walls |= CellWall.West;
                if (!cellConnect.HasFlag(CellConnect.Bottom) || i == Rows - 1)
                    walls |= CellWall.South;
                if (j == 0 || !Cells[idx - Columns].HasFlag(CellConnect.Bottom))
                    walls |= CellWall.North;

                Walls[idx++] = walls;
            }
        }
    }
}