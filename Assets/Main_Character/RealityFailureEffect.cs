using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class RealityFailureEffect : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private CanvasGroup blackFade;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip failureSound;

    [SerializeField] private float effectDuration = 1.5f;

    private ChromaticAberration chromaticAberration;
    private Vignette vignette;
    private LensDistortion lensDistortion;

    private void Awake()
    {
        if (volume != null &&
            volume.profile != null)
        {
            volume.profile.TryGet(
                out chromaticAberration
            );

            volume.profile.TryGet(
                out vignette
            );

            volume.profile.TryGet(
                out lensDistortion
            );
        }

        if (blackFade != null)
        {
            blackFade.alpha = 0f;
            blackFade.interactable = false;
            blackFade.blocksRaycasts = false;

            Image fadeImage =
                blackFade.GetComponent<Image>();

            if (fadeImage != null)
            {
                fadeImage.raycastTarget = false;
            }
        }
    }

    public IEnumerator PlayFailure()
    {
        if (audioSource != null &&
            failureSound != null)
        {
            audioSource.PlayOneShot(
                failureSound
            );
        }

        float elapsed = 0f;

        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / effectDuration
                );

            float pulse =
                Mathf.Abs(
                    Mathf.Sin(
                        progress *
                        Mathf.PI *
                        8f
                    )
                );

            if (chromaticAberration != null)
            {
                chromaticAberration
                    .intensity.value =
                    Mathf.Lerp(
                        0.2f,
                        1f,
                        pulse
                    );
            }

            if (vignette != null)
            {
                vignette
                    .intensity.value =
                    Mathf.Lerp(
                        0.2f,
                        0.65f,
                        progress
                    );
            }

            if (lensDistortion != null)
            {
                lensDistortion
                    .intensity.value =
                    Mathf.Lerp(
                        0f,
                        -0.7f,
                        pulse
                    );
            }

            yield return null;
        }

        float fadeElapsed = 0f;
        float fadeDuration = 0.6f;

        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.deltaTime;

            if (blackFade != null)
            {
                blackFade.alpha =
                    Mathf.Clamp01(
                        fadeElapsed /
                        fadeDuration
                    );
            }

            yield return null;
        }

        if (blackFade != null)
        {
            blackFade.alpha = 1f;
            blackFade.interactable = false;
            blackFade.blocksRaycasts = false;
        }
    }
}