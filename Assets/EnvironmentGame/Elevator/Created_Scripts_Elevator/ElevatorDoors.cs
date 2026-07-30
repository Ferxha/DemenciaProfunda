using UnityEngine;

public class ElevatorDoors : MonoBehaviour
{
    [Header("Animators de las puertas")]
    [SerializeField] private Animator leftDoorAnimator;
    [SerializeField] private Animator rightDoorAnimator;

    [Header("Nombres de los estados")]
    [SerializeField] private string leftOpenState = "OpenD";
    [SerializeField] private string leftCloseState = "CloseD";

    [SerializeField] private string rightOpenState = "OpenI";
    [SerializeField] private string rightCloseState = "CloseI";

    private bool doorsOpen;

    private void Awake()
    {
        Debug.Log("ElevatorDoors está activo.", this);
    }

    public void OpenDoors()
    {
        Debug.Log("OPEN DOORS FUE LLAMADO.", this);

        if (leftDoorAnimator == null)
        {
            Debug.LogError(
                "No está asignado el Animator de Door left.",
                this
            );
        }
        else
        {
            Debug.Log(
                $"Reproduciendo {leftOpenState} en Door left.",
                leftDoorAnimator
            );

            leftDoorAnimator.Play(leftOpenState, 0, 0f);
        }

        if (rightDoorAnimator == null)
        {
            Debug.LogError(
                "No está asignado el Animator de Door right.",
                this
            );
        }
        else
        {
            Debug.Log(
                $"Reproduciendo {rightOpenState} en Door right.",
                rightDoorAnimator
            );

            rightDoorAnimator.Play(rightOpenState, 0, 0f);
        }

        doorsOpen = true;
    }

    public void CloseDoors()
    {
        Debug.Log("CLOSE DOORS FUE LLAMADO.", this);

        if (!doorsOpen)
        {
            Debug.LogWarning(
                "Las puertas ya estaban marcadas como cerradas.",
                this
            );
        }

        if (leftDoorAnimator != null)
        {
            leftDoorAnimator.Play(leftCloseState, 0, 0f);
        }

        if (rightDoorAnimator != null)
        {
            rightDoorAnimator.Play(rightCloseState, 0, 0f);
        }

        doorsOpen = false;
    }

    [ContextMenu("Probar apertura")]
    private void TestOpenDoors()
    {
        OpenDoors();
    }

    [ContextMenu("Probar cierre")]
    private void TestCloseDoors()
    {
        CloseDoors();
    }
}