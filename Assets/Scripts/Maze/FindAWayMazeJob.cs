using System;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Debug = UnityEngine.Debug;

public struct WayItem : IEquatable<WayItem>
{
    public int Row, Col;
    public CellWall CellWalls;
    public CellWall Wall;
    public int Length;

    public bool Equals(WayItem other) => Row == other.Row && Col == other.Col;
}

//[BurstCompile()]
public struct FindAWayMazeJob : IJob
{
    [ReadOnly] public int Rows, Columns;
    [ReadOnly] public bool Log;

    [ReadOnly] public NativeArray<CellWall> Walls;

    // -1 cualquier fila c columna
    [ReadOnly] public int ColIni, ColEnd;
    [ReadOnly] public int RowIni, RowEnd;
    [ReadOnly] public int MaxWayItem;

    [WriteOnly] public int CelIni, CelEnd;
    public NativeList<int> WayResult;
    [WriteOnly] public int WayCountResult;
    [WriteOnly] public bool Found;

    public void Execute()
    {
        Found = false;

        if (MaxWayItem == 0)
            return;

        var stack = new NativeList<WayItem>(MaxWayItem, Allocator.Temp);

        var item = new WayItem();
        int row0 = RowIni > 0 ? RowIni : 0, col0 = ColIni > 0 ? ColIni : 0;

        CelIni = row0 * Columns + col0;

        CreateItem(row0, col0, ref item);

        stack.Add(item);

        while (stack.Length > 0)
        {
            var idx = stack.Length - 1;

            item = stack[idx];
            stack.RemoveAt(idx);

            if ((RowEnd <= 0 || RowEnd == item.Row) && (ColEnd <= 0 || ColEnd == item.Col))
            {
                Found = true;
                WayCountResult = item.Length;
                CelEnd = item.Row * Columns + item.Col;

                if (Log)
                    Debug.Log($"Found way: {WayCountResult} items. Cell: {CelIni} - {CelEnd}");

                break;
            }
            else
            {
                if (NextItem(ref item))
                {
                    if (!PushItem(ref stack, ref item))
                        break;
                }
                else if (PopWall(ref item))
                {
                    if (!PushItem(ref stack, ref item))
                        break;
                }
                else if (AnotherWay(ref item))
                {
                    if (!PushItem(ref stack, ref item))
                        break;
                }
            }
        }

        CreateWay(ref stack);

        if (stack.IsCreated)
            stack.Dispose();
    }

    private void CreateWay(ref NativeList<WayItem> stack)
    {
        if (stack.Length > WayResult.Length)
            return;

        for (int i = 0; i < stack.Length; i++)
            WayResult[i] = stack[i].Row * Columns + stack[i].Col;
    }

    private bool AnotherWay(ref WayItem item)
    {
        if (ColIni < 0 && item.Col < Columns)
        {
            CreateItem(item.Row, item.Col + 1, ref item);

            return true;

        }
        else if (RowIni < 0 && item.Row < Rows)
        {
            CreateItem(item.Row + 1, item.Col, ref item);

            return true;
        }

        return false;
    }

    private bool NextItem(ref WayItem item)
    {
        int row = item.Row, col = item.Col;
        var cellWalls = GetCellWall(row, col);

        //if (cellWalls.HasFlag(item.Wall))
        if ((cellWalls & item.Wall) != 0)
            return false;

        (row, col) = NextCell(item);

        if (row < 0 || col < 0)
            return false;

        CreateItem(row, col, ref item, item.Wall);

        return true;
    }

    /// <summary>
    /// -1 no se puede ir en esa dirección
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private readonly (int row, int col) NextCell(WayItem item)
    {
        int row = item.Row;
        int col = item.Col;

        switch (item.Wall)
        {
            case CellWall.North:
                row--;
                break;
            case CellWall.South:
                row = row < Rows - 1 ? item.Row + 1 : -1;
                break;
            case CellWall.East:
                col = col < Columns - 1 ? item.Col + 1 : -1;
                break;
            case CellWall.West:
                col--;
                break;
            default:
                row = col = -1;
                break;
        };

        return (row, col);
    }

    private bool PushItem(ref NativeList<WayItem> stack, ref WayItem item)
    {
        if ((WayResult.Capacity > 0 && item.Length >= WayResult.Capacity) || stack.Length >= MaxWayItem)
            return false;

        if (stack.Contains(item))
            return false;

        item.Length++;
        stack.Add(item);

        return true;
    }

    private bool PopWall(ref WayItem item)
    {
        CellWall wall = item.Wall;
        CellWall cellWalls;

        if (item.Length > 1)
        {
            CellWall oppositeWall = GetCellWallOposite(wall);
            cellWalls = item.CellWalls & ~oppositeWall;
        }
        else
            cellWalls = item.CellWalls;

        (cellWalls, wall) = PopWall(cellWalls);

        item.CellWalls = cellWalls;
        item.Wall = wall;

        return cellWalls != CellWall.None;
    }

    private readonly CellWall GetCellWallOposite(CellWall wall)
        => wall switch
        {
            CellWall.North => CellWall.South,
            CellWall.South => CellWall.North,
            CellWall.East => CellWall.West,
            CellWall.West => CellWall.East,
            _ => CellWall.None
        };

    private readonly (CellWall cell, CellWall wall) PopWall(CellWall cellWalls)
    {
        CellWall wall;

        //if (cellWalls.HasFlag(CellWall.North))
        if ((cellWalls & CellWall.North) != 0)
            wall = CellWall.North;
        //else if (cellWalls.HasFlag(CellWall.South))
        else if ((cellWalls & CellWall.South) != 0)
            wall = CellWall.South;
        //else if (cellWalls.HasFlag(CellWall.East))
        else if ((cellWalls & CellWall.East) != 0)
            wall = CellWall.East;
        else
            return (CellWall.None, CellWall.None);

        cellWalls &= ~wall;

        return (cellWalls, wall);
    }

    private void CreateItem(int row, int col, ref WayItem item, CellWall wallIni = CellWall.None)
    {
        CellWall wall;

        item.Row = row;
        item.Col = col;

        CellWall cellWalls = GetCellWall(row, col);

        item.CellWalls = (~cellWalls) & CellWall.AllWalls;
        if (wallIni == CellWall.None) //if (wallIni =)
        {
            (cellWalls, wall) = PopWall(item.CellWalls);
            item.CellWalls = cellWalls;
            item.Wall = wall;
        }
        else
            item.Wall = wallIni;
        item.Length = 1;
    }

    private readonly CellWall GetCellWall(int row, int col)
    {
        int cell = GetCell(row, col);
        CellWall cellWalls = Walls[cell];

        return cellWalls;
    }

    private readonly int GetCell(int row, int col)
    {
        return row * Columns + col;
    }
}