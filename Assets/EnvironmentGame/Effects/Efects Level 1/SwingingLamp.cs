using UnityEngine;

public class SwingingLamp : MonoBehaviour
{
    [Header("Movimiento de balanceo")]
    [SerializeField] private float horizontalAngle = 4f;
    [SerializeField] private float verticalAngle = 2f;
    [SerializeField] private float swingSpeed = 1.2f;

    [Header("Movimiento irregular")]
    [SerializeField] private bool useIrregularMovement = true;
    [SerializeField] private float irregularAmount = 0.8f;
    [SerializeField] private float irregularSpeed = 0.5f;

    private Quaternion initialRotation;
    private float randomOffset;

    private void Start()
    {
        initialRotation = transform.localRotation;
        randomOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        float time = Time.time * swingSpeed;

        float rotationX =
            Mathf.Sin(time) * verticalAngle;

        float rotationZ =
            Mathf.Sin(time * 0.8f) * horizontalAngle;

        if (useIrregularMovement)
        {
            float noise =
                Mathf.PerlinNoise(
                    randomOffset,
                    Time.time * irregularSpeed
                );

            noise = (noise - 0.5f) * 2f;

            rotationX += noise * irregularAmount;
            rotationZ += noise * irregularAmount;
        }

        Quaternion swingRotation =
            Quaternion.Euler(
                rotationX,
                0f,
                rotationZ
            );

        transform.localRotation =
            initialRotation * swingRotation;
    }
}
