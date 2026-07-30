using UnityEngine;

public class OutlineTarget : MonoBehaviour
{
    [Header("Objeto con el shader de contorno")]
    [SerializeField] private GameObject outlineObject;

    private void Awake()
    {
        if (outlineObject == null)
        {
            Debug.LogWarning(
                $"No se asignó el objeto de contorno en {gameObject.name}.",
                this
            );

            return;
        }

        // El contorno empieza oculto.
        outlineObject.SetActive(false);
    }

    public void ShowOutline()
    {
        if (outlineObject != null &&
            !outlineObject.activeSelf)
        {
            outlineObject.SetActive(true);
        }
    }

    public void HideOutline()
    {
        if (outlineObject != null &&
            outlineObject.activeSelf)
        {
            outlineObject.SetActive(false);
        }
    }
}
