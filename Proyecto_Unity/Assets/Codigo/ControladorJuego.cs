using UnityEngine;
using TMPro;
using UnityEngine.UI; 
using System.Collections; 
using UnityEngine.SceneManagement; 
using UnityEngine.EventSystems; // <-- IMPORTANTE: Para soltar botones al despausar

public class ControladorJuego : MonoBehaviour
{
    public static ControladorJuego instancia;

    [Header("Interfaz")]
    public TextMeshProUGUI textoPuntuacion;
    
    [Header("Sistema de Vidas")]
    public Image[] corazonesUI; 
    
    [Header("Game Over")]
    public GameObject panelGameOver;
    public float velocidadFade = 1.5f; 
    public RectTransform posicionFinalPuntuacion; 

    [Header("Pausa")]
    public GameObject panelPausa;
    public GameObject botonPausa;
    private bool juegoPausado = false;
    
    [Header("Animación de Puntos")]
    private float multiplicadorTamano = 1.3f; 
    public float duracionEfecto = 0.2f;      
    
    private int puntosActuales = 0;
    private Vector3 escalaOriginal;
    private Coroutine animacionTexto;

    void Awake()
    {
        if (instancia == null) instancia = this;
    }

    void Start()
    {
        Time.timeScale = 1f; 

        if (textoPuntuacion != null)
        {
            escalaOriginal = textoPuntuacion.rectTransform.localScale;
        }
        
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelPausa != null) panelPausa.SetActive(false); // El panel de pausa empieza oculto
        
        ActualizarTexto();
    }

    public void PausarJuego()
    {
        if (panelGameOver != null && panelGameOver.activeSelf) return; // No pausa el tiempo si ya estás muerto

        juegoPausado = true;
        Time.timeScale = 0f; // Congela el tiempo

        if (panelPausa != null) panelPausa.SetActive(true);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void ReanudarJuego()
    {
        juegoPausado = false;
        Time.timeScale = 1f; // Descongela el tiempo

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (panelPausa != null)
        {
            Button[] botones = panelPausa.GetComponentsInChildren<Button>();
            foreach (Button b in botones)
            {
                b.interactable = false;
                b.interactable = true;
            }
            
            panelPausa.SetActive(false);
        }
    }

    public void ActualizarVidasUI(int vidasRestantes)
    {
        for (int i = 0; i < corazonesUI.Length; i++)
        {
            if (i < vidasRestantes) corazonesUI[i].enabled = true;  
            else corazonesUI[i].enabled = false; 
        }
    }

    public void SumarPuntos(int puntosGanados)
    {
        puntosActuales += puntosGanados;
        ActualizarTexto();

        if (animacionTexto != null) StopCoroutine(animacionTexto);
        animacionTexto = StartCoroutine(LatidoTexto());
    }

    private void ActualizarTexto()
    {
        if (textoPuntuacion != null) textoPuntuacion.text = puntosActuales.ToString(); 
    }

    public void MostrarGameOver()
    {
        if (botonPausa != null) botonPausa.SetActive(false);

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);

            if (textoPuntuacion != null) textoPuntuacion.transform.SetAsLastSibling();

            StartCoroutine(EfectoFadeGameOver());
        }
    }

    private IEnumerator EfectoFadeGameOver()
    {
        float tiempo = 0f;
        
        CanvasGroup grupo = panelGameOver.GetComponent<CanvasGroup>();
        if (grupo == null) grupo = panelGameOver.AddComponent<CanvasGroup>();

        grupo.alpha = 0f;
        grupo.interactable = false; 

        Vector3 posicionInicialTexto = textoPuntuacion != null ? textoPuntuacion.rectTransform.position : Vector3.zero;
        Vector3 posicionDestinoTexto = posicionFinalPuntuacion != null ? posicionFinalPuntuacion.position : posicionInicialTexto;

        while (tiempo < velocidadFade)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / velocidadFade;
            
            grupo.alpha = Mathf.Lerp(0f, 1f, t);

            if (textoPuntuacion != null && posicionFinalPuntuacion != null)
            {
                textoPuntuacion.rectTransform.position = Vector3.Lerp(posicionInicialTexto, posicionDestinoTexto, Mathf.SmoothStep(0f, 1f, t));
            }

            yield return null;
        }

        grupo.alpha = 1f;
        grupo.interactable = true; 
    }

    public void ReiniciarJuego()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator LatidoTexto()
    {
        if (Time.timeScale == 0) yield break; 

        float tiempo = 0f;
        Vector3 escalaMaxima = escalaOriginal * multiplicadorTamano;
        float mitadTiempo = duracionEfecto / 2f;

        while (tiempo < mitadTiempo)
        {
            tiempo += Time.deltaTime;
            textoPuntuacion.rectTransform.localScale = Vector3.Lerp(escalaOriginal, escalaMaxima, tiempo / mitadTiempo);
            yield return null;
        }

        tiempo = 0f;

        while (tiempo < mitadTiempo)
        {
            tiempo += Time.deltaTime;
            textoPuntuacion.rectTransform.localScale = Vector3.Lerp(escalaMaxima, escalaOriginal, tiempo / mitadTiempo);
            yield return null;
        }

        textoPuntuacion.rectTransform.localScale = escalaOriginal;
    }
}