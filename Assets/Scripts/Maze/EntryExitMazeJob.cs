using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct EntryExitMazeJob : IJob
{
    [ReadOnly] public int Rows, Columns;
    [ReadOnly] public bool Debug;
    [ReadOnly] public NativeArray<CellConnect> Cells;

    public NativeList<EnTryExitCols> EntryExitCols;

    public void Execute()
    {
        var nodes = new NativeArray<int>(Rows, Allocator.Temp);
        var row = 0;
        var colLast = Columns - 1;
        var rowLast = Rows - 1;

        do
        {
            var col = nodes[row];

            if (col >= Columns)
            {
                row--;
                continue;
            }

            var cell = Cells[row * Columns + col];

            if (row < rowLast)
            {
                if ((cell & CellConnect.Bottom) != 0)
                {
                    row++;
                    while (col > 0 && ((Cells[row * Columns + col - 1] & CellConnect.Right) != 0))
                        col--;
                    nodes[row] = col;
                }
                else
                    nodes[row]++;
            }
            else
            {
                var colIniRow0 = nodes[0];
                var colEndRow0 = colIniRow0;
                var colIniRowLast = col;
                var colEndRowLast = col;

                while (colIniRow0 > 0 && ((Cells[colIniRow0 - 1] & CellConnect.Right) != 0))
                    colIniRow0--;

                while (colEndRow0 < colLast && ((Cells[colEndRow0] & CellConnect.Right) != 0))
                    colEndRow0++;

                nodes[0] = colEndRow0 + 1;

                while (colIniRowLast > 0 && ((Cells[rowLast * Columns + colIniRowLast - 1] & CellConnect.Right) != 0))
                    colIniRowLast--;

                while (colEndRowLast < colLast && ((Cells[rowLast * Columns + colEndRowLast] & CellConnect.Right) != 0))
                    colEndRowLast++;

                EntryExitCols.Add(new EnTryExitCols
                {
                    colEntryIni = colIniRow0,
                    colEntryEnd = colEndRow0,
                    colExitIni = colIniRowLast,
                    colExitEnd = colEndRowLast,
                });

                row = 0;
            }
        } while (row >= 0);

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
