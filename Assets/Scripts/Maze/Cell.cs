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

public class Cell : MonoBehaviour
{
    //public bool 
}