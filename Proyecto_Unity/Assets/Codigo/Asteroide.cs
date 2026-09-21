using UnityEngine;
using UnityEngine.UI;
using System.Collections; 

public class Asteroide : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 4f;
    private Vector3 direccionVuelo = Vector3.zero;

    [Header("Animación")]
    public Sprite[] spritesAnimacion; 
    public float fps = 0.1f; 
    private SpriteRenderer spriteRenderer;
    private int fotogramaActual = 0;
    private float temporizadorAnimacion;

    [Header("Vida y UI")]
    public int vidaMaxima = 5; 
    private int vidaActual;
    public Image rellenoBarraVida; 
    public Transform canvasVida;   
    public Vector3 offsetBarra = new Vector3(0f, 1.5f, 0f);

    [Header("Efectos Visuales")]
    public float duracionGolpe = 0.15f;
    public float tiempoDesvanecer = 0.3f;
    public ParticleSystem efectoParticulas;

    [Header("Fragmentación")]
    public bool puedeDividirse = true;
    public GameObject prefabMiniAsteroide; 
    
    private Coroutine rutinaGolpe;
    private bool estaMuerto = false; 
    private bool esMini = false;

    public AudioClip explosion;
    private Rigidbody2D rb;

    void Awake()
    {
        efectoParticulas = GetComponentInChildren<ParticleSystem>(true);

        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        // Modo Kinematic hace que no caiga por la gravedad
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; 
    }

    public void ConfigurarMiniAsteroide(Vector3 nuevaDireccion)
    {
        esMini = true; 
        direccionVuelo = nuevaDireccion.normalized;
        vidaMaxima = 1;
        vidaActual = 1;
        puedeDividirse = false; 
        
        transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        if (efectoParticulas != null)
        {
            var main = efectoParticulas.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }
        offsetBarra = offsetBarra * 0.5f; 
        velocidad = velocidad * 1.5f; 
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        estaMuerto = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        if (canvasVida != null) canvasVida.gameObject.SetActive(false);
        if (rellenoBarraVida != null) rellenoBarraVida.color = Color.green;

        if (!esMini)
        {
            vidaActual = vidaMaxima; 
            
            GameObject jugador = GameObject.Find("Jugador");
            if (jugador != null)
                direccionVuelo = (jugador.transform.position - transform.position).normalized;
            else
                direccionVuelo = Random.insideUnitCircle.normalized;
        }

        if (rb != null) rb.linearVelocity = direccionVuelo * velocidad;

        float angulo = Mathf.Atan2(direccionVuelo.y, direccionVuelo.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo + 90f);
    }

    void Update()
    {
        if (estaMuerto) return;

        if (spritesAnimacion != null && spritesAnimacion.Length > 0 && spriteRenderer != null)
        {
            temporizadorAnimacion += Time.deltaTime;
            
            if (temporizadorAnimacion >= fps)
            {
                temporizadorAnimacion = 0f;
                fotogramaActual++;
                if (fotogramaActual >= spritesAnimacion.Length) fotogramaActual = 0;
                spriteRenderer.sprite = spritesAnimacion[fotogramaActual];
            }
        }
    }

    void LateUpdate() 
    {
        if (canvasVida != null)
        {
            canvasVida.rotation = Quaternion.identity; 
            canvasVida.position = transform.position + offsetBarra;
        }
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (estaMuerto) return; 

        Disparo disparo = otro.GetComponent<Disparo>();
        if (disparo != null)
        {
            disparo.Morir();
            RecibirDano(1);                   
        }
    }

    void RecibirDano(int cantidad)
    {
        if (esMini) cantidad = 999; 

        vidaActual -= cantidad;

        if (canvasVida != null && !canvasVida.gameObject.activeSelf)
        {
            canvasVida.gameObject.SetActive(true);
        }
        
        if (rellenoBarraVida != null)
        {
            float porcentaje = (float)vidaActual / vidaMaxima;
            rellenoBarraVida.fillAmount = porcentaje;
            rellenoBarraVida.color = Color.Lerp(Color.red, Color.green, porcentaje);
        }

        if (vidaActual <= 0)
        {
            Morir();
        }
        else
        {
            if (rutinaGolpe != null) StopCoroutine(rutinaGolpe);
            rutinaGolpe = StartCoroutine(EfectoGolpeBrillo());
        }
    }

    void Morir()
    {
        estaMuerto = true;

        if (explosion != null) AudioSource.PlayClipAtPoint(explosion, Camera.main.transform.position, 0.5f);

        if (efectoParticulas != null) efectoParticulas.Play();
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        if (rb != null) rb.linearVelocity = Vector2.zero; // Frenamos en seco

        if (canvasVida != null) canvasVida.gameObject.SetActive(false);

        if (puedeDividirse && prefabMiniAsteroide != null)
        {
            Vector3 adelante = direccionVuelo;
            Vector3 atras = -direccionVuelo;
            Vector3 derecha = new Vector3(-direccionVuelo.y, direccionVuelo.x, 0); 
            Vector3 izquierda = new Vector3(direccionVuelo.y, -direccionVuelo.x, 0); 

            Vector3[] direcciones = { adelante, atras, derecha, izquierda };

            foreach (Vector3 dir in direcciones)
            {
                GameObject mini = Instantiate(prefabMiniAsteroide, transform.position, Quaternion.identity);
                Asteroide scriptMini = mini.GetComponent<Asteroide>();
                if (scriptMini != null)
                {
                    scriptMini.ConfigurarMiniAsteroide(dir);
                }
            }
        }

        if (ControladorJuego.instancia != null) ControladorJuego.instancia.SumarPuntos(esMini ? 1 : 3);

        StartCoroutine(EfectoDesvanecer());
    }

    IEnumerator EfectoGolpeBrillo()
    {
        float tiempo = 0f;
        Color colorDanio = Color.red;
        Color colorNormal = Color.white;

        while (tiempo < duracionGolpe)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionGolpe;
            if (spriteRenderer != null) spriteRenderer.color = Color.Lerp(colorDanio, colorNormal, t);
            yield return null;
        }

        if (spriteRenderer != null) spriteRenderer.color = colorNormal;
        rutinaGolpe = null;
    }

    IEnumerator EfectoDesvanecer()
    {
        float tiempo = 0f;
        Color colorInicial = spriteRenderer != null ? spriteRenderer.color : Color.white;
        
        while (tiempo < tiempoDesvanecer)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / tiempoDesvanecer;

            if (spriteRenderer != null && spriteRenderer.enabled)
            {
                float alphaActual = Mathf.Lerp(colorInicial.a, 0f, t);
                spriteRenderer.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, alphaActual);
            }
            yield return null;
        }
        
        if (efectoParticulas != null)
        {
            efectoParticulas.transform.SetParent(null);
        }
        
        Destroy(gameObject);
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}