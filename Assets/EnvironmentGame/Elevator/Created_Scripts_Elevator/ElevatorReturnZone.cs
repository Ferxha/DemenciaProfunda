using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ElevatorReturnZone : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField] private DeepGameManager gameManager;

    private void Awake()
    {
        BoxCollider zoneCollider =
            GetComponent<BoxCollider>();

        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError(
                "DeepGameManager no está asignado en ReturnZone.",
                this
            );

            return;
        }

        gameManager.SetPlayerInsideElevator(true);

        Debug.Log(
            "El jugador entró al elevador.",
            this
        );
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError(
                "DeepGameManager no está asignado en ReturnZone.",
                this
            );

            return;
        }

        gameManager.SetPlayerInsideElevator(false);

        Debug.Log(
            "El jugador salió del elevador y entró a la sala.",
            this
        );
    }
}