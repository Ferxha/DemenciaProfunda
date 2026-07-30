using UnityEngine;

public class ElevatorReturnZone : MonoBehaviour
{
    [SerializeField] private DeepGameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        gameManager.SetPlayerInsideElevator(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        gameManager.SetPlayerInsideElevator(false);
    }
}