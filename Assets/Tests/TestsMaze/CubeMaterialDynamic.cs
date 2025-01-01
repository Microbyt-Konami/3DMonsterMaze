using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static UnityEditor.MaterialProperty;

public class CubeMaterialDynamic : MonoBehaviour
{
    [SerializeField] private bool useX = true;
    [SerializeField] private bool useY;
    [SerializeField] private bool useZ = true;
    [SerializeField] private Renderer myRenderer;
    [SerializeField] private Material material;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Obtén el tamaño del cubo
        Vector3 size = transform.localScale;
        Debug.Log($"Size: {size}", gameObject);
        Vector2 sizeNew = Vector2.zero;

        SetDim(ref sizeNew, size.x, useX);
        SetDim(ref sizeNew, size.y, useX);
        SetDim(ref sizeNew, size.z, useZ);

        // Ajusta el tiling según el tamaño del cubo
        material = new Material(myRenderer.material);//material = myRend
        myRenderer.material = material;
        //material = myRenderer.material;//material = myRend
        material.mainTextureScale = sizeNew;
        Debug.Log($"mainTextureScale: {material.mainTextureScale}", gameObject);

        void SetDim(ref Vector2 v, float value, bool use)
        {
            if (use)
                if (v.x == 0)
                    v.x = value;
                else
                    v.y = value;
        }
    }
}
