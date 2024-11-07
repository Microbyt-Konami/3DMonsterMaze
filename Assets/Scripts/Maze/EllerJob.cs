using System.Text;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public struct EllerJob : IJob
{
    public uint seed;
    public int rows, columns;
    public NativeArray<CellConnect> cells;
    public bool log;

    private Random random;
    private int idxRowCurrentIni;
    private int idxRowCurrentEnd;

    public void Execute()
    {
        random = new Random(seed);
        InitRow1();
        for (idxRowCurrentIni = 0; idxRowCurrentIni < cells.Length; idxRowCurrentIni += columns)
        {
            idxRowCurrentEnd = idxRowCurrentIni + columns - 1;
            JoinRowHCellsRandom();
            JoinRowVCellsRandom();
        }
    }

    private void InitRow1()
    {
        // Lo primero que se tiene que hacer cada columna de la primera fila tiene su conjunto propio
        for (int i = 0; i < columns; i++)
            cells[i] |= CellConnect.None;
    }

    private void JoinRowHCellsRandom()
    {
        // En la fila actual decidimos aleatoriamente por cada par de celda adjacentes si se unen o no
        for (int i = idxRowCurrentIni; i <= idxRowCurrentEnd; i++)
            if (random.NextBool())
                cells[i] |= CellConnect.Right;

        if (log)
            LogSetCells("En la fila actual decidimos aleatoriamente por cada par de celda adjacentes si se unen o no");
    }

    private void JoinRowVCellsRandom()
    {
        // Ahora determinamos aleatoriamente las conexiones verticales, al menos una por conjunto.
        for (int i = idxRowCurrentIni; i <= idxRowCurrentEnd; i++)
        {
            bool joinDone = false;
            int idxFin = GetCellEndSet(i);
            float range = 0.5f;

            if (i == idxFin)
                cells[i] |= CellConnect.Bottom;
            else
            {
            }
        }
    }

    private int GetCellEndSet(int idxRowCurrent)
    {
        for (var i = idxRowCurrent + 1; i <= idxRowCurrentEnd; i++)
            if (cells[i].HasFlag(CellConnect.Right))
                return i;

        return idxRowCurrentEnd;
    }

    private void LogSetCells(string text)
    {
        // text.Append(text);
        // text.Append('\n');
        var sb = new StringBuilder();

        sb.AppendLine(text.ToString());

        int idx = 0;
        int nrows = (idxRowCurrentEnd + 1) / columns;

        for (int j = 0; j < columns; j++)
            sb.Append("--");
        sb.AppendLine();
        for (int i = 0; i < nrows; i++)
        {
            sb.Append("|");
            bool wasWallRight = false;

            for (int j = 0; j < columns; j++)
            {
                wasWallRight = !cells[idx + j].HasFlag(CellConnect.Right);

                sb.Append($"{(wasWallRight ? " |" : "  ")}");
            }

            sb.AppendLine((!wasWallRight) ? "|" : "");

            if (i < rows - 1)
            {
                sb.Append("|");
                for (int j = 0; j < columns; j++)
                    sb.Append(!cells[idx++].HasFlag(CellConnect.Bottom) ? "--" : "  ");
                sb.AppendLine("|");
            }
        }

        for (int j = 0; j < columns; j++)
            sb.Append("--");
        sb.AppendLine();

        Debug.Log(sb.ToString());
    }
}