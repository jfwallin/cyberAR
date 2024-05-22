using UnityEngine;
using MagicLeapTools;

public class ButtonInteraction : MonoBehaviour
{
    public Color hoverColor = Color.yellow;
    public Color pressColor = Color.red;
    private Color originalColor;
    private Material buttonMaterial;
    private InputReceiver input;

    void Start()
    {
        // Clone the material to avoid changing the shared material
        buttonMaterial = GetComponent<Renderer>().material;
        originalColor = buttonMaterial.color;
        input = GetComponent<InputReceiver>();
        input.OnTargetEnter.AddListener((x) => buttonMaterial.color = hoverColor);
        input.OnTargetExit.AddListener((x) => buttonMaterial.color = originalColor);
        input.OnFire0Down.AddListener((x) => buttonMaterial.color = pressColor);
        input.OnFire0Up.AddListener((x) => buttonMaterial.color = originalColor);
    }
    private void OnDestroy()
    {
        input.OnTargetEnter.RemoveAllListeners();
        input.OnTargetExit.RemoveAllListeners();
        input.OnFire0Down.RemoveAllListeners();
        input.OnFire0Up.RemoveAllListeners();
    }
}
