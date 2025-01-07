using UnityEngine;

public class TestWall1 : MonoBehaviour
{
    [SerializeField] private TestWall1[] wallsOCcuder;
    [SerializeField] private Cell[] cells;

    private bool _occluder;

    public bool Occluder { get => _occluder; set => _occluder = value; }

    private void OnBecameVisible()
    {
        Debug.Log("Visible", gameObject);

        if (_occluder)
            return;

        foreach (var wall in wallsOCcuder)
            wall.Occluder = true;

        foreach (var cell in cells)
            cell.gameObject.SetActive(false);
    }

    private void OnBecameInvisible()
    {
        Debug.Log("Invisible", gameObject);

        if (_occluder)
            return;

        foreach (var wall in wallsOCcuder)
            wall.Occluder = false;

        foreach (var cell in cells)
            cell.gameObject.SetActive(true);
    }
}
