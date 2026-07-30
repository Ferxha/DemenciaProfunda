using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HueShiftCycle : MonoBehaviour
{
    [Header("Global Volume")]
    [SerializeField] private Volume globalVolume;

    [Header("Duraciones")]
    [SerializeField] private float zeroToPositiveDuration = 3f;
    [SerializeField] private float positiveToNegativeDuration = 4f;

    private ColorAdjustments colorAdjustments;
    private Coroutine hueCoroutine;

    private void Awake()
    {
        if (globalVolume == null)
        {
            Debug.LogError(
                "LevelThreeHueEffect: Global Volume no está asignado.",
                this
            );

            enabled = false;
            return;
        }

        VolumeProfile runtimeProfile = globalVolume.profile;

        if (runtimeProfile == null)
        {
            Debug.LogError(
                "LevelThreeHueEffect: El Global Volume no tiene Profile.",
                this
            );

            enabled = false;
            return;
        }

        if (!runtimeProfile.TryGet(out colorAdjustments))
        {
            Debug.LogError(
                "LevelThreeHueEffect: El Profile no tiene Color Adjustments.",
                this
            );

            enabled = false;
            return;
        }

        colorAdjustments.active = true;
        colorAdjustments.hueShift.overrideState = true;
        colorAdjustments.hueShift.value = 0f;

        Debug.Log(
            "LevelThreeHueEffect listo.",
            this
        );
    }

    [ContextMenu("Probar efecto")]
    public void StartKeysSearchEffect()
    {
        if (colorAdjustments == null)
        {
            Debug.LogError(
                "No se puede iniciar el efecto porque Color Adjustments es null.",
                this
            );

            return;
        }

        if (hueCoroutine != null)
        {
            StopCoroutine(hueCoroutine);
        }

        colorAdjustments.hueShift.value = 0f;

        hueCoroutine = StartCoroutine(
            HueRoutine()
        );

        Debug.Log(
            "Iniciando efecto automático de Hue Shift.",
            this
        );
    }

    public void StopKeysSearchEffect()
    {
        if (hueCoroutine != null)
        {
            StopCoroutine(hueCoroutine);
            hueCoroutine = null;
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.hueShift.value = 0f;
        }
    }

    private IEnumerator HueRoutine()
    {
        yield return StartCoroutine(
            ChangeHue(
                0f,
                180f,
                zeroToPositiveDuration
            )
        );

        yield return StartCoroutine(
            ChangeHue(
                180f,
                -180f,
                positiveToNegativeDuration
            )
        );

        hueCoroutine = null;
    }

    private IEnumerator ChangeHue(
        float startValue,
        float endValue,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / duration
            );

            colorAdjustments.hueShift.value =
                Mathf.Lerp(
                    startValue,
                    endValue,
                    progress
                );

            yield return null;
        }

        colorAdjustments.hueShift.value = endValue;
    }

    private void OnDisable()
    {
        StopKeysSearchEffect();
    }
}
