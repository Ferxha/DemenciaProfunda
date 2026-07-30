using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RealityFlashEffect : MonoBehaviour
{
    [Header("Modelos de la ilusión")]
    [Tooltip("Modelo normal de la cámara fotográfica.")]
    [SerializeField] private GameObject cameraModel;

    [Tooltip("Modelo del arma que aparecerá brevemente.")]
    [SerializeField] private GameObject weaponModel;

    [Header("Flash de pantalla")]
    [Tooltip("Imagen blanca que cubre toda la pantalla.")]
    [SerializeField] private Image flashImage;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Sonido del obturador de la cámara.")]
    [SerializeField] private AudioClip shutterSound;

    [Header("Duraciones")]
    [Tooltip("Duración del primer flash blanco.")]
    [SerializeField] private float firstFlashDuration = 0.08f;

    [Tooltip("Tiempo durante el que el arma permanece visible.")]
    [SerializeField] private float weaponVisibleDuration = 0.15f;

    [Tooltip("Duración del segundo flash blanco.")]
    [SerializeField] private float secondFlashDuration = 0.08f;

    [Tooltip("Tiempo que tarda el segundo flash en desaparecer.")]
    [SerializeField] private float returnToNormalDuration = 0.12f;

    private Coroutine effectCoroutine;
    private bool effectPlaying;

    public bool IsPlaying => effectPlaying;

    private void Awake()
    {
        if (flashImage != null)
        {
            SetFlashAlpha(0f);

            // Evita que la imagen bloquee botones o interacción UI.
            flashImage.raycastTarget = false;
        }

        if (weaponModel != null)
        {
            weaponModel.SetActive(false);
        }
    }

    /// <summary>
    /// Inicia la secuencia cámara → arma → normalidad.
    /// </summary>
    public void PlayEffect()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }

        effectCoroutine = StartCoroutine(RealityFlashRoutine());
    }

    private IEnumerator RealityFlashRoutine()
    {
        effectPlaying = true;

        /*
         * 1. La cámara fotográfica desaparece.
         */
        if (cameraModel != null)
        {
            cameraModel.SetActive(false);
        }

        /*
         * 2. Primer flash blanco.
         */
        SetFlashAlpha(1f);

        /*
         * 3. Sonido del obturador.
         */
        if (audioSource != null && shutterSound != null)
        {
            audioSource.PlayOneShot(shutterSound);
        }

        yield return new WaitForSeconds(firstFlashDuration);

        /*
         * Quitamos el primer flash para revelar el arma.
         */
        SetFlashAlpha(0f);

        /*
         * 4. El arma aparece en la misma posición.
         */
        if (weaponModel != null)
        {
            weaponModel.SetActive(true);
        }

        yield return new WaitForSeconds(weaponVisibleDuration);

        /*
         * 5. Segundo flash blanco.
         */
        SetFlashAlpha(1f);

        /*
         * 6. El arma desaparece mientras la pantalla está blanca.
         */
        if (weaponModel != null)
        {
            weaponModel.SetActive(false);
        }

        yield return new WaitForSeconds(secondFlashDuration);

        /*
         * 7. La pantalla vuelve gradualmente a la normalidad.
         */
        yield return FadeFlash(
            1f,
            0f,
            returnToNormalDuration
        );

        effectPlaying = false;
        effectCoroutine = null;
    }

    private IEnumerator FadeFlash(
        float startAlpha,
        float endAlpha,
        float duration)
    {
        if (flashImage == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            SetFlashAlpha(endAlpha);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / duration
            );

            float alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                progress
            );

            SetFlashAlpha(alpha);

            yield return null;
        }

        SetFlashAlpha(endAlpha);
    }

    private void SetFlashAlpha(float alpha)
    {
        if (flashImage == null)
        {
            return;
        }

        Color color = flashImage.color;
        color.a = Mathf.Clamp01(alpha);
        flashImage.color = color;
    }

    private void OnDisable()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
            effectCoroutine = null;
        }

        effectPlaying = false;

        if (flashImage != null)
        {
            SetFlashAlpha(0f);
        }

        if (weaponModel != null)
        {
            weaponModel.SetActive(false);
        }
    }
}
