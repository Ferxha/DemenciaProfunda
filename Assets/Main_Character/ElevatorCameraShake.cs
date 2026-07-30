using System.Collections;
using UnityEngine;

public class ElevatorCameraShake : MonoBehaviour
{
    [Header("Configuración predeterminada")]
    [SerializeField] private float defaultDuration = 2.5f;

    [Header("Intensidad")]
    [SerializeField] private float positionStrength = 0.018f;
    [SerializeField] private float rotationStrength = 0.7f;
    [SerializeField] private float frequency = 22f;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private Coroutine shakeCoroutine;

    private void Awake()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    public void PlayShake()
    {
        PlayShake(defaultDuration);
    }

    public void PlayShake(float duration)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(
            ShakeRoutine(duration)
        );
    }

    private IEnumerator ShakeRoutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / duration
            );

            float fade = 1f - progress;

            float noiseX =
                Mathf.PerlinNoise(
                    Time.time * frequency,
                    0f
                ) * 2f - 1f;

            float noiseY =
                Mathf.PerlinNoise(
                    0f,
                    Time.time * frequency
                ) * 2f - 1f;

            float noiseZ =
                Mathf.PerlinNoise(
                    Time.time * frequency,
                    Time.time * frequency
                ) * 2f - 1f;

            Vector3 positionOffset = new Vector3(
                noiseX,
                noiseY,
                noiseZ * 0.35f
            ) * positionStrength * fade;

            Quaternion rotationOffset =
                Quaternion.Euler(
                    noiseY * rotationStrength * fade,
                    noiseX * rotationStrength * fade,
                    noiseZ * rotationStrength * fade
                );

            transform.localPosition =
                initialLocalPosition + positionOffset;

            transform.localRotation =
                initialLocalRotation * rotationOffset;

            yield return null;
        }

        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;

        shakeCoroutine = null;
    }

    private void OnDisable()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
    }
}