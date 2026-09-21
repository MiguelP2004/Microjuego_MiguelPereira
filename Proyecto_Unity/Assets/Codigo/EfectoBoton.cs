using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EfectoBoton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animación de Escala")]
    public float multiplicadorTamano = 1.15f;
    public float multiplicadorClic = 0.9f;
    public float velocidadAnimacion = 12f;

    [Header("Balanceo")]
    public bool balanceoSoloAlPasarRaton = true;
    
    private float velocidadBalanceo = 2.5f;
    private float anguloMaximo = 2.5f;

    [Header("Audio")]
    public AudioClip boton;
    
    private Vector3 escalaOriginal;
    private Vector3 escalaObjetivo;
    private bool ratonEncima = false; 
    public bool esPausa = false;
    private Button miBoton;

    void Start()
    {
        miBoton = GetComponent<Button>();
        escalaOriginal = transform.localScale;
        escalaObjetivo = escalaOriginal;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, escalaObjetivo, Time.unscaledDeltaTime * velocidadAnimacion);

        if (balanceoSoloAlPasarRaton && ratonEncima)
        {
            float anguloActual = Mathf.Sin(Time.unscaledTime * velocidadBalanceo) * anguloMaximo;
            transform.localRotation = Quaternion.Euler(0f, 0f, anguloActual);
        }
        else
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.unscaledDeltaTime * velocidadAnimacion);
        }

        if (esPausa && Input.GetKeyDown(KeyCode.Escape))
        {
            if (miBoton != null && miBoton.interactable)
            {
                ReproducirSonido();
                miBoton.onClick.Invoke(); 
            }
        }
    }

    void OnDisable()
    {
        ratonEncima = false;
        escalaObjetivo = escalaOriginal;
        transform.localScale = escalaOriginal;
        transform.localRotation = Quaternion.identity;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        escalaObjetivo = escalaOriginal * multiplicadorTamano;
        ratonEncima = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        escalaObjetivo = escalaOriginal;
        ratonEncima = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        escalaObjetivo = escalaOriginal * multiplicadorClic;
        ReproducirSonido();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        escalaObjetivo = escalaOriginal * multiplicadorTamano;
    }

    private void ReproducirSonido()
    {
        if (boton != null)
        {
            GameObject altavozUI = new GameObject("SonidoBotonUI");
            DontDestroyOnLoad(altavozUI);
            AudioSource fuenteVirtual = altavozUI.AddComponent<AudioSource>();
            fuenteVirtual.clip = boton;
            fuenteVirtual.volume = 1f;
            fuenteVirtual.spatialBlend = 0f;
            
            fuenteVirtual.ignoreListenerPause = true;
            fuenteVirtual.ignoreListenerVolume = true; 
            
            fuenteVirtual.Play();
            Destroy(altavozUI, boton.length);
        }
    }
}