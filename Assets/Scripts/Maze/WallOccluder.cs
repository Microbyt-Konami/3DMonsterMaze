using UnityEngine;

public class WallOccluder : MonoBehaviour
{
    public Cell[] cellsToOccluder;

    //public bool occluder;
    public int nLock;

    private void OnBecameVisible()
    {
        Occlude(true);
    }

    //private void OnBecameInvisible()
    //{
    //    Occlude(false);
    //}

    public void Occlude(bool argIsOccluder)
    {
        foreach (var cell in cellsToOccluder)
            cell.Occlude(argIsOccluder);
    }
}
