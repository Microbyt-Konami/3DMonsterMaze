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

[Serializable]
public class Cell
{
    public int SetId { get; set; }
    public CellConnect Connects { get; set; }

    public bool isWallRight => !Connects.HasFlag(CellConnect.Right);
    public bool isWallBottom => !Connects.HasFlag(CellConnect.Bottom);
}