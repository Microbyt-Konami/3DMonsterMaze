using System;
using UnityEngine;

[Flags]
public enum CellNeighbor
{
    North = 0x1,
    East = 0x2,
    South = 0x4,
    West = 0x8
}

[Flags]
public enum CellWall
{
    None = 0,
    Right = 0x1,
    Bottom = 0x2,
    Default = Right | Bottom
}

[Serializable]
public class Cell
{
    public int SetId { get; set; }
    public CellWall Walls { get; set; }
}