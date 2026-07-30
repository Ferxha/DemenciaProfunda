using UnityEngine;

public class RandomLightsMovement : MonoBehaviour
{
    [Header("Luces a mover")]
    [SerializeField] private Transform[] lightsToMove;

    [Header("Área de movimiento")]
    [SerializeField] private float areaWidth = 10f;
    [SerializeField] private float areaHeight = 6f;

    [Header("Movimiento")]
    [SerializeField] private float minSpeed = 1f;
    [SerializeField] private float maxSpeed = 3f;

    [SerializeField] private float minWaitTime = 0.2f;
    [SerializeField] private float maxWaitTime = 1f;

    [SerializeField] private float arrivalDistance = 0.05f;

    [Header("Configuración")]
    [SerializeField] private bool useLocalPosition = true;

    private Vector3[] targetPositions;
    private float[] movementSpeeds;
    private float[] waitTimers;
    private float[] fixedZPositions;

    private void Start()
    {
        if (lightsToMove == null || lightsToMove.Length == 0)
        {
            Debug.LogWarning(
                "No hay luces asignadas en RandomLightsMovement.",
                this
            );

            enabled = false;
            return;
        }

        int lightCount = lightsToMove.Length;

        targetPositions = new Vector3[lightCount];
        movementSpeeds = new float[lightCount];
        waitTimers = new float[lightCount];
        fixedZPositions = new float[lightCount];

        for (int i = 0; i < lightCount; i++)
        {
            if (lightsToMove[i] == null)
            {
                continue;
            }

            fixedZPositions[i] = useLocalPosition
                ? lightsToMove[i].localPosition.z
                : lightsToMove[i].position.z;

            AssignNewTarget(i);
        }
    }

    private void Update()
    {
        for (int i = 0; i < lightsToMove.Length; i++)
        {
            Transform currentLight = lightsToMove[i];

            if (currentLight == null)
            {
                continue;
            }

            if (waitTimers[i] > 0f)
            {
                waitTimers[i] -= Time.deltaTime;
                continue;
            }

            Vector3 currentPosition = useLocalPosition
                ? currentLight.localPosition
                : currentLight.position;

            Vector3 newPosition = Vector3.MoveTowards(
                currentPosition,
                targetPositions[i],
                movementSpeeds[i] * Time.deltaTime
            );

            // Mantiene fija la posición en Z.
            newPosition.z = fixedZPositions[i];

            if (useLocalPosition)
            {
                currentLight.localPosition = newPosition;
            }
            else
            {
                currentLight.position = newPosition;
            }

            if (Vector3.Distance(
                    newPosition,
                    targetPositions[i]
                ) <= arrivalDistance)
            {
                waitTimers[i] = Random.Range(
                    minWaitTime,
                    maxWaitTime
                );

                AssignNewTarget(i);
            }
        }
    }

    private void AssignNewTarget(int index)
    {
        float randomX = Random.Range(
            -areaWidth * 0.5f,
            areaWidth * 0.5f
        );

        float randomY = Random.Range(
            -areaHeight * 0.5f,
            areaHeight * 0.5f
        );

        if (useLocalPosition)
        {
            targetPositions[index] = new Vector3(
                randomX,
                randomY,
                fixedZPositions[index]
            );
        }
        else
        {
            Vector3 areaCenter = transform.position;

            targetPositions[index] = new Vector3(
                areaCenter.x + randomX,
                areaCenter.y + randomY,
                fixedZPositions[index]
            );
        }

        movementSpeeds[index] = Random.Range(
            minSpeed,
            maxSpeed
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = useLocalPosition
            ? transform.localToWorldMatrix
            : Matrix4x4.identity;

        Vector3 center = useLocalPosition
            ? Vector3.zero
            : transform.position;

        Gizmos.DrawWireCube(
            center,
            new Vector3(
                areaWidth,
                areaHeight,
                0.01f
            )
        );
    }
}
