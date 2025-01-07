using UnityEngine;

public class Wall : MonoBehaviour
{
    private WallOccluder _wallOccluder;

    [SerializeField] private MeshRenderer _wallRenderer;
    public SpawnerItemsCell spawnerOreBlue;
    public SpawnerItemsCell spawnerOreGreen;
    public SpawnerItemsCell spawnerOreRed;

    public CellWall cellWall;

    private void Awake()
    {
        _wallOccluder = _wallRenderer.GetComponent<WallOccluder>();
    }

    public Cell GetCell() => GetComponentInParent<Cell>();

    public void SetWallMaterial(Material wallMaterial)
    {
        _wallRenderer.material = wallMaterial;
    }

    public void SetCellsToOcclude(Cell[] cells)
    {
        _wallOccluder.cellsToOccluder = cells;
    }
}
