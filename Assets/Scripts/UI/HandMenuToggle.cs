using UnityEngine;
using UnityEngine.InputSystem;

public class HandMenuToggle : MonoBehaviour
{
    public GameObject menu;
    public InputActionReference toggleAction;

    private void OnEnable()
    {
        toggleAction.action.Enable();
        toggleAction.action.performed += ToggleMenu;
    }

    private void OnDisable()
    {
        toggleAction.action.performed -= ToggleMenu;
        toggleAction.action.Disable();
    }

    private void ToggleMenu(InputAction.CallbackContext context)
    {
        menu.SetActive(!menu.activeSelf);
    }
}
