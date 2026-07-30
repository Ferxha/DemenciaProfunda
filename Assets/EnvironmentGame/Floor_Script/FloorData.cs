using UnityEngine;

[System.Serializable]
public class FloorData
{
    [Header("Identificación")]
    public string floorName;

    [Header("Escenario")]
    public GameObject levelObject;
    public GameObject panelNumberObject;

    [Header("Objeto clave")]
    public InteractableKeyObject keyObject;

    [Header("Tiempo")]
    public float timeLimit;

    [Header("Audio inicial")]
    public AudioClip introductionAudio;

    [Header("Susurro")]
    public AudioClip whisperAudio;
    public float whisperDelay = 4f;

    [Header("Efecto especial")]
    public bool useRealityFlash;
}
