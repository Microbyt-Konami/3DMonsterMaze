using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

//[BurstCompile]
public struct EntryExitMazeJob : IJob
{
    [ReadOnly] public int Rows, Columns;
    [ReadOnly] public bool Debug;
    [ReadOnly] public NativeArray<CellConnect> Cells;

    public NativeList<EnTryExitCols> EntryExitCols;

    public void Execute()
    {
        var nodes = new NativeList<Node>(Rows, Allocator.Temp);
        var node = new Node { colIni = 0, row = 0, col = 0, colIniRow0 = 0 };
        CellConnect cell;

        nodes.Add(node);

        while (nodes.Length > 0)
        {
            node = nodes[nodes.Length - 1];
            nodes.RemoveAt(nodes.Length - 1);

            if (node.col >= Columns)
                continue;

            if (node.row == Rows - 1)
            {
                while (node.col < Columns - 1 && ((Cells[node.col] & CellConnect.Right) != 0))
                    node.col++;

                var colEndRow0 = node.colIniRow0;

                while (colEndRow0 < Columns - 1 && ((Cells[colEndRow0] & CellConnect.Right) != 0))
                    colEndRow0++;

                EntryExitCols.Add(new EnTryExitCols
                {
                    colEntryIni = node.colIniRow0,
                    colEntryEnd = colEndRow0,
                    colExitIni = node.colIni,
                    colExitEnd = node.col,
                });

                if (node.col < Columns - 1)
                {
                    node.row = 0;
                    node.col++;
                    nodes.Add(node);
                }
            }
            else
            {
                cell = Cells[node.row * Columns + node.col];

                if ((cell & CellConnect.Bottom) != 0)
                {
                    node.row++;
                    if (node.row <= Rows - 1)
                    {
                        while (node.col > 0 && ((Cells[node.row * Columns + node.col - 1] & CellConnect.Right) != 0))
                            node.col--;
                        node.colIni = node.col;
                        nodes.Add(node);
                    }
                }
                else if ((cell & CellConnect.Right) != 0)
                {
                    node.col++;
                    nodes.Add(node);
                }
                else
                {
                    if (node.row == 0)
                    {
                        var hasWallR = (Cells[node.col] & CellConnect.Right) == 0;

                        node.col++;
                        node.colIni = node.col;
                        if (hasWallR)
                            node.colIniRow0 = node.col;
                        nodes.Add(node);
                    }
                }
            }
        }

        if (nodes.IsCreated)
            nodes.Dispose();
    }

    public struct EnTryExitCols
    {
        public int colEntryIni, colEntryEnd, colExitIni, colExitEnd;
    }

    public struct Node
    {
        public int row, colIni, col;
        public int colIniRow0;
    }
}
