using Unity.Collections;
using Unity.Jobs;

public struct EntryExitMazeJob : IJob
{
    [ReadOnly] public int Rows, Columns;
    [ReadOnly] public bool Log;
    [ReadOnly] public NativeArray<CellConnect> Cells;

    public NativeList<EnTryExitCols> EntryExitCols;

    public void Execute()
    {
        var nodes = new NativeList<Node>(Rows, Allocator.Temp);
        var node = new Node { col0 = 0, row = 0, col = 0 };
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
                var nodeEnd = node;

                while (nodeEnd.col < Columns - 1 && Cells[nodeEnd.col].HasFlag(CellConnect.Right))
                    nodeEnd.col++;

                nodes.RemoveRange(1, nodes.Length - 1);
                node = nodes[0];
                while (node.col < Columns - 1 && Cells[node.col].HasFlag(CellConnect.Right))
                    node.col++;

                EntryExitCols.Add(new EnTryExitCols
                {
                    colEntryIni = nodeEnd.col0,
                    colEntryEnd = nodeEnd.col,
                    colExitIni = node.col,
                    colExitEnd = node.col,
                });

                if (node.col < Columns - 1)
                {
                    node.col++;
                    nodes[0] = node;
                }
                else
                    nodes.RemoveAt(0);
            }
            else
            {
                cell = Cells[node.row * Columns + node.col];

                if (cell.HasFlag(CellConnect.Bottom))
                {
                    node.row++;
                    if (node.row < Rows - 1)
                    {
                        while (node.col > 0 && Cells[node.row * Columns + node.col - 1].HasFlag(CellConnect.Right))
                            node.col--;
                        node.col0 = node.col;
                        nodes.Add(node);
                    }
                }
                else if (cell.HasFlag(CellConnect.Right))
                {
                    node.col++;
                    nodes.Add(node);
                }
                else
                {
                    if (node.row == 0)
                    {
                        node.col++;
                        node.col0 = node.col;
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
        public int row, col0, col;
    }
}
