using UnityEngine;

public class Wall : MonoBehaviour
{
    [SerializeField] private MeshRenderer _wallRenderer;
    public SpawnerItemsCell spawnerOreBlue;
    public SpawnerItemsCell spawnerOreGreen;
    public SpawnerItemsCell spawnerOreRed;

    public void SetWallMaterial(Material wallMaterial)
    {
        _wallRenderer.material = wallMaterial;
    }
}
