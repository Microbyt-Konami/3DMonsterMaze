using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[Flags]
public enum CellWall
{
    None = 0x0,
    North = 0x1,
    East = 0x2,
    South = 0x4,
    West = 0x8,
    AllWalls = North | East | South | West
}

//[BurstCompile]
public struct ConnectCellsToWallsJob : IJob
{
    [ReadOnly] public int Rows, Columns;
    [ReadOnly] public bool Debug;
    [ReadOnly] public NativeArray<CellConnect> Cells;
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

                //if (!cellConnect.HasFlag(CellConnect.Right) || j == Columns - 1)
                if ((cellConnect & CellConnect.Right) == 0 || j == Columns - 1)
                    walls |= CellWall.East;
                //if (j == 0 || !Cells[idx - 1].HasFlag(CellConnect.Right))
                if (j == 0 || (Cells[idx - 1] & CellConnect.Right) == 0)
                    walls |= CellWall.West;
                //if (!cellConnect.HasFlag(CellConnect.Bottom) || i == Rows - 1)
                if ((cellConnect & CellConnect.Bottom) == 0 || i == Rows - 1)
                    walls |= CellWall.South;
                //if (i == 0 || !Cells[idx - Columns].HasFlag(CellConnect.Bottom))
                if (i == 0 || (Cells[idx - Columns] & CellConnect.Bottom) == 0)
                    walls |= CellWall.North;

                Walls[idx++] = walls;
            }
        }

        //if (Log)
        //    LogWallsCells("Mostra las paredes");
    }

    /*
    private void LogWallsCells(string text)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine(text.ToString());

        int idx = 0;

        for (int j = 0; j < Columns; j++)
            sb.Append("---");
        sb.AppendLine();
        for (int i = 0; i < Rows; i++)
        {
            if (Walls[idx].HasFlag(CellWall.West))
                sb.Append("|");
            //bool wasWallRight = false;

            for (int j = 0; j < Columns; j++)
            {
                if (Walls[idx + j].HasFlag(CellWall.North))
                    sb.Append('N');
                else
                    sb.Append(' ');
                if (Walls[idx + j].HasFlag(CellWall.West))
                    sb.Append('W');
                else
                    sb.Append(' ');

                if (Walls[idx + j].HasFlag(CellWall.East))
                    sb.Append('|');
                else
                    sb.Append(' ');
            }

            sb.AppendLine();
            if (Walls[idx].HasFlag(CellWall.West))
                sb.Append("|");
            for (int j = 0; j < Columns; j++)
            {
                if (Walls[idx + j].HasFlag(CellWall.South))
                    sb.Append("--");
                else
                    sb.Append("  ");
                if (Walls[idx + j].HasFlag(CellWall.East))
                    sb.Append('|');
                else
                    sb.Append(' ');
            }

            sb.AppendLine();
        }

        for (int j = 0; j < Columns; j++)
            sb.Append("---");
        sb.AppendLine();

        Debug.Log(sb.ToString());
    }
    */
}