using System;
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
    West = 0x8
}

public struct ConnectCellsToWallsJob : IJob
{
    public int Rows, Columns;
    public bool Log;
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
                if (j == 0 || !Cells[idx - 1].HasFlag(CellConnect.Right))
                    walls |= CellWall.West;
                if (!cellConnect.HasFlag(CellConnect.Bottom) || i == Rows - 1)
                    walls |= CellWall.South;
                if (i == 0 || !Cells[idx - Columns].HasFlag(CellConnect.Bottom))
                    walls |= CellWall.North;

                Walls[idx++] = walls;
            }
        }
        
        if(Log)
            LogWallsCells("Mostra las paredes");
    }

    private void LogWallsCells(string text)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine(text.ToString());

        int idx = 0;

        for (int j = 0; j < Columns; j++)
            sb.Append("--");
        sb.AppendLine("-");
        for (int i = 0; i < Rows; i++)
        {
            sb.Append("|");
            //bool wasWallRight = false;

            for (int j = 0; j < Columns - 1; j++)
            {
                if(Walls[idx + j].HasFlag(CellWall.North))
                    sb.Append('N');
                else
                    sb.Append(' ');
                var wasWallRight = !Walls[idx + j].HasFlag(CellWall.East);

                sb.Append($"{(wasWallRight ? " |" : "  ")}");
            }

            sb.AppendLine(" |");

            if (i < Rows - 1)
            {
                sb.Append("|");
                for (int j = 0; j < Columns; j++)
                    sb.Append(!Walls[idx++].HasFlag(CellWall.South) ? "--" : "  ");
                sb.AppendLine("|");
            }
        }

        for (int j = 0; j < Columns; j++)
            sb.Append("--");
        sb.AppendLine("-");

        Debug.Log(sb.ToString());
    }
}