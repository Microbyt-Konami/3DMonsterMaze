using System;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[Flags]
public enum CellConnect
{
    None = 0,
    Right = 0x1,
    Bottom = 0x2,
    Default = Right | Bottom
}

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
        int idxLastRow = (rows - 1) * columns;
        random = new Random(seed);
        InitRow1();
        for (idxRowCurrentIni = 0; idxRowCurrentIni < idxLastRow; idxRowCurrentIni += columns)
        {
            idxRowCurrentEnd = idxRowCurrentIni + columns - 1;
            JoinRowHCellsRandom();
            JoinRowVCellsRandom();
        }

        JoinLastRowCells();
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
            int idxFin = GetCellEndSet(i);
            float range = 0.5f;

            if (i == idxFin)
                cells[i] |= CellConnect.Bottom;
            else
            {
                bool joinDone = false;
                do
                {
                    for (var j = i; j <= idxFin; j++)
                    {
                        if (random.NextFloat() >= range)
                        {
                            cells[j] |= CellConnect.Bottom;
                            joinDone = true;
                        }
                    }

                    range = range > 0.1f ? range - 0.1f : 0;
                } while (!joinDone);

                i = idxFin;
            }
        }

        if (log)
            LogSetCells("Ahora determinamos aleatoriamente las conexiones verticales, al menos una por conjunto.");
    }

    void JoinLastRowCells()
    {
        // Reiniciar los conjuntos conservando en el mismo conjunto aquellas celdas que compartan conjunto superior
        int idxRowPrev = idxRowCurrentIni - columns;
        int idxRowPrevPrev = idxRowPrev - columns;

        if (idxRowPrev >= 0 && idxRowPrevPrev >= 0)
        {
            for (; idxRowPrev < idxRowCurrentIni; idxRowPrev++, idxRowPrevPrev++)
                if (cells[idxRowPrevPrev].HasFlag(CellConnect.Bottom))
                    cells[idxRowPrev] |= CellConnect.Bottom;
        }

        for (var i = idxRowCurrentIni; i < idxRowCurrentEnd; i++)
            cells[i] |= CellConnect.Right;

        if (log)
            LogSetCells(
                "Reiniciar los conjuntos conservando en el mismo conjunto aquellas celdas que compartan conjunto superior.");
    }

    private int GetCellEndSet(int idxRowCurrent)
    {
        if (idxRowCurrent == idxRowCurrentEnd)
            return idxRowCurrentEnd;

        for (var i = idxRowCurrent + 1; i < idxRowCurrentEnd; i++)
            if (!cells[i].HasFlag(CellConnect.Right))
                return i - 1;

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
        sb.AppendLine("-");
        for (int i = 0; i < nrows; i++)
        {
            sb.Append("|");
            //bool wasWallRight = false;

            for (int j = 0; j < columns - 1; j++)
            {
                var wasWallRight = !cells[idx + j].HasFlag(CellConnect.Right);

                sb.Append($"{(wasWallRight ? " |" : "  ")}");
            }

            sb.AppendLine(" |");

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
        sb.AppendLine("-");

        Debug.Log(sb.ToString());
    }
}