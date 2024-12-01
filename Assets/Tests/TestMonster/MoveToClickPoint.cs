using UnityEngine;
using UnityEngine.InputSystem;


public class MoveToClickPoint : MonoBehaviour
{
    void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            var mousePosition = mouse.position.ReadValue();

            if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition), out var hit, 100))
                transform.position = hit.point;
        }
    }
}
