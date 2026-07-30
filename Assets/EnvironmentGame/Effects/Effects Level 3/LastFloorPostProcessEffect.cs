using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LastFloorPostProcessEffect : MonoBehaviour
{
    [Header("Global Volume")]
    [SerializeField] private Volume globalVolume;

    [Header("Chromatic Aberration")]
    [Range(0f, 1f)]
    [SerializeField] private float chromaticIntensity = 1f;

    [Header("Color Filter")]
    [SerializeField]
    private Color lastFloorColor =
        new Color32(239, 255, 211, 255);

    private ChromaticAberration chromaticAberration;
    private ColorAdjustments colorAdjustments;

    private float originalChromaticIntensity;
    private Color originalColorFilter;

    private bool initialized;

    private void Awake()
    {
        InitializeEffect();
    }

    private void InitializeEffect()
    {
        if (globalVolume == null)
        {
            Debug.LogError(
                "No se asignó el Global Volume.",
                this
            );

            return;
        }

        VolumeProfile runtimeProfile =
            globalVolume.profile;

        if (runtimeProfile == null)
        {
            Debug.LogError(
                "El Global Volume no tiene un Volume Profile.",
                this
            );

            return;
        }

        if (!runtimeProfile.TryGet(
                out chromaticAberration))
        {
            Debug.LogError(
                "El Volume Profile no contiene Chromatic Aberration.",
                this
            );

            return;
        }

        if (!runtimeProfile.TryGet(
                out colorAdjustments))
        {
            Debug.LogError(
                "El Volume Profile no contiene Color Adjustments.",
                this
            );

            return;
        }

        originalChromaticIntensity =
            chromaticAberration.intensity.value;

        originalColorFilter =
            colorAdjustments.colorFilter.value;

        initialized = true;
    }

    public void ApplyLastFloorEffect()
    {
        if (!initialized)
        {
            InitializeEffect();
        }

        if (!initialized)
        {
            return;
        }

        chromaticAberration.active = true;
        chromaticAberration.intensity.overrideState = true;
        chromaticAberration.intensity.value =
            chromaticIntensity;

        colorAdjustments.active = true;
        colorAdjustments.colorFilter.overrideState = true;
        colorAdjustments.colorFilter.value =
            lastFloorColor;

        Debug.Log(
            "Efecto del último piso activado.",
            this
        );
    }

    public void ResetEffect()
    {
        if (!initialized)
        {
            return;
        }

        chromaticAberration.intensity.value =
            originalChromaticIntensity;

        colorAdjustments.colorFilter.value =
            originalColorFilter;
    }
}
