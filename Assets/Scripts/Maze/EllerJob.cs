using System;
using System.Text;
using Unity.Burst;
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

[BurstCompile]
public struct EllerJob : IJob
{
    [ReadOnly] public uint Seed;
    [ReadOnly] public int Rows, Columns;
    [ReadOnly] public bool Log;
    public NativeArray<CellConnect> Cells;

    private Random _random;
    private int _idxRowCurrentIni;
    private int _idxRowCurrentEnd;

    public void Execute()
    {
        int idxLastRow = (Rows - 1) * Columns;
        _random = new Random(Seed);
        InitRow1();
        for (_idxRowCurrentIni = 0; _idxRowCurrentIni < idxLastRow; _idxRowCurrentIni += Columns)
        {
            _idxRowCurrentEnd = _idxRowCurrentIni + Columns - 1;
            JoinRowHCellsRandom();
            JoinRowVCellsRandom();
        }

        JoinLastRowCells();
    }

    private void InitRow1()
    {
        // Lo primero que se tiene que hacer cada columna de la primera fila tiene su conjunto propio
        for (int i = 0; i < Columns; i++)
            Cells[i] |= CellConnect.None;
    }

    private void JoinRowHCellsRandom()
    {
        // En la fila actual decidimos aleatoriamente por cada par de celda adjacentes si se unen o no
        for (int i = _idxRowCurrentIni; i <= _idxRowCurrentEnd; i++)
            if (_random.NextBool())
                Cells[i] |= CellConnect.Right;

        //if (Log)
        //    LogSetCells("En la fila actual decidimos aleatoriamente por cada par de celda adjacentes si se unen o no");
    }

    private void JoinRowVCellsRandom()
    {
        // Ahora determinamos aleatoriamente las conexiones verticales, al menos una por conjunto.
        for (int i = _idxRowCurrentIni; i <= _idxRowCurrentEnd; i++)
        {
            int idxFin = GetCellEndSet(i);
            float range = 0.5f;

            if (i == idxFin)
                Cells[i] |= CellConnect.Bottom;
            else
            {
                bool joinDone = false;
                do
                {
                    for (var j = i; j <= idxFin; j++)
                    {
                        if (_random.NextFloat() >= range)
                        {
                            Cells[j] |= CellConnect.Bottom;
                            joinDone = true;
                        }
                    }

                    range = range > 0.1f ? range - 0.1f : 0;
                } while (!joinDone);

                i = idxFin;
            }
        }

        //if (Log)
        //    LogSetCells("Ahora determinamos aleatoriamente las conexiones verticales, al menos una por conjunto.");
    }

    void JoinLastRowCells()
    {
        // Reiniciar los conjuntos conservando en el mismo conjunto aquellas celdas que compartan conjunto superior
        int idxRowPrev = _idxRowCurrentIni - Columns;
        int idxRowPrevPrev = idxRowPrev - Columns;

        if (idxRowPrev >= 0 && idxRowPrevPrev >= 0)
        {
            for (; idxRowPrev < _idxRowCurrentIni; idxRowPrev++, idxRowPrevPrev++)
                //if (Cells[idxRowPrevPrev].HasFlag(CellConnect.Bottom))
                if ((Cells[idxRowPrevPrev] & CellConnect.Bottom) != 0)
                    Cells[idxRowPrev] |= CellConnect.Bottom;
        }

        for (var i = _idxRowCurrentIni; i < _idxRowCurrentEnd; i++)
            Cells[i] |= CellConnect.Right;

        //if (Log)
        //    LogSetCells(
        //        "Reiniciar los conjuntos conservando en el mismo conjunto aquellas celdas que compartan conjunto superior.");
    }

    private int GetCellEndSet(int idxRowCurrent)
    {
        if (idxRowCurrent == _idxRowCurrentEnd)
            return _idxRowCurrentEnd;

        for (var i = idxRowCurrent + 1; i < _idxRowCurrentEnd; i++)
            //if (!Cells[i].HasFlag(CellConnect.Right))
            if ((Cells[i] & CellConnect.Right) == 0)
                return i - 1;

        return _idxRowCurrentEnd;
    }

    /*
    private void LogSetCells(string text)
    {
        var sb = new StringBuilder();

        sb.AppendLine(text.ToString());

        int idx = 0;
        int nrows = (_idxRowCurrentEnd + 1) / Columns;

        for (int j = 0; j < Columns; j++)
            sb.Append("--");
        sb.AppendLine("-");
        for (int i = 0; i < nrows; i++)
        {
            sb.Append("|");
            //bool wasWallRight = false;

            for (int j = 0; j < Columns - 1; j++)
            {
                //var wasWallRight = !Cells[idx + j].HasFlag(CellConnect.Right);
                var wasWallRight = (Cells[idx + j] & CellConnect.Right) == 0;

                sb.Append($"{(wasWallRight ? " |" : "  ")}");
            }

            sb.AppendLine(" |");

            if (i < Rows - 1)
            {
                sb.Append("|");
                for (int j = 0; j < Columns; j++)
                    //sb.Append(!Cells[idx++].HasFlag(CellConnect.Bottom) ? "--" : "  ");
                    sb.Append((Cells[idx++] & CellConnect.Bottom) == 0 ? "--" : "  ");
                sb.AppendLine("|");
            }
        }

        for (int j = 0; j < Columns; j++)
            sb.Append("--");
        sb.AppendLine("-");

        Debug.Log(sb.ToString());
    }
    */
}