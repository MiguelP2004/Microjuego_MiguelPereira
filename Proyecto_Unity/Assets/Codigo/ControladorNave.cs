using System.Collections; 
using System.Collections.Generic;
using UnityEngine;

public class ControladorNave : MonoBehaviour
{
    [Header("Movimiento Fluido")]
    public float fuerzaMovimiento = 25f; 

    [Header("Disparo y Object Pooling")]
    public GameObject prefabBala;
    public Transform puntoDisparo;
    public float cooldownDisparo = 0.5f;
    
    // Lo subo a 30 por el disparo múltiple, para que no se acaben las balas
    private int tamanoPool = 30; 

    [Header("Vida e Invulnerabilidad")]
    public float tiempoInvulnerabilidad = 3f;
    private int vidas = 3;
    
    [Header("Efectos Visuales")]
    public float multiplicadorCura = 1.3f; 
    private float tiempoEfecto = 1.5f;  

    public ParticleSystem humoMovimiento;
    
    private Vector3 escalaOriginal;
    
    private Coroutine rutinaCura;
    private Coroutine rutinaInvulnerabilidad;
    
    private Coroutine rutinaMultiple;
    private bool tieneDisparoMultiple = false;

    private bool esInvulnerable = false;
    private bool estaCurandose = false; 
    
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Camera camaraPrincipal;
    
    private Vector2 inputMovimiento;
    private Vector2 posicionRaton;
    
    private float temporizadorDisparo;
    private List<GameObject> poolBalas = new List<GameObject>();

    private float anchoPantalla;
    private float altoPantalla;

    [Header("Audio")]
    public AudioClip[] disparos;
    public AudioClip derrota;
    public AudioClip recibirGolpe;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        camaraPrincipal = Camera.main; 

        escalaOriginal = transform.localScale;

        if (humoMovimiento != null) humoMovimiento.Play();

        altoPantalla = camaraPrincipal.orthographicSize;
        anchoPantalla = altoPantalla * camaraPrincipal.aspect;

        for (int i = 0; i < tamanoPool; i++)
        {
            GameObject bala = Instantiate(prefabBala);
            bala.SetActive(false);
            poolBalas.Add(bala);
        }
    }

    void Update()
    {
        inputMovimiento = Vector2.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputMovimiento.y = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputMovimiento.y = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputMovimiento.x = 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputMovimiento.x = -1f;
        
        inputMovimiento = inputMovimiento.normalized;

        posicionRaton = camaraPrincipal.ScreenToWorldPoint(Input.mousePosition);

        temporizadorDisparo += Time.deltaTime;
        if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && temporizadorDisparo >= cooldownDisparo)
        {
            EjecutarDisparo();
            
            if (disparos.Length > 0)
            {
                int indiceAleatorio = Random.Range(0, disparos.Length);
                AudioSource.PlayClipAtPoint(disparos[indiceAleatorio], Camera.main.transform.position, 0.35f);
            }
            temporizadorDisparo = 0f;
        }
        ChequearBordesPantalla();
    }

    void FixedUpdate()
    {
        rb.AddForce(inputMovimiento * fuerzaMovimiento);

        Vector2 direccionMirar = posicionRaton - rb.position;
        float angulo = Mathf.Atan2(direccionMirar.y, direccionMirar.x) * Mathf.Rad2Deg;
        rb.rotation = angulo;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        Asteroide asteroide = otro.GetComponent<Asteroide>();
        if (asteroide != null) RecibirDano();
    }
    
    void OnCollisionEnter2D(Collision2D choque)
    {
        Asteroide asteroide = choque.gameObject.GetComponent<Asteroide>();
        if (asteroide != null) RecibirDano();
    }

    void RecibirDano()
    {
        if (esInvulnerable) return;

        if (rutinaCura != null) StopCoroutine(rutinaCura);
        estaCurandose = false;
        transform.localScale = escalaOriginal; 

        vidas--;
        if (recibirGolpe != null) AudioSource.PlayClipAtPoint(recibirGolpe, Camera.main.transform.position);
        
        if (ControladorJuego.instancia != null)
        {
            ControladorJuego.instancia.ActualizarVidasUI(vidas);
        }

        if (vidas <= 0)
        {
            if (ControladorJuego.instancia != null) ControladorJuego.instancia.MostrarGameOver();
            
            if (derrota != null) AudioSource.PlayClipAtPoint(derrota, Camera.main.transform.position);
            gameObject.SetActive(false); 
        }
        else
        {
            if (rutinaInvulnerabilidad != null) StopCoroutine(rutinaInvulnerabilidad);
            rutinaInvulnerabilidad = StartCoroutine(EfectoInvulnerabilidad());
        }
    }

    public void GanarVida()
    {
        if (vidas < 3)
        {
            vidas++;
            if (ControladorJuego.instancia != null)
            {
                ControladorJuego.instancia.ActualizarVidasUI(vidas);
            }
        }

        if (rutinaInvulnerabilidad != null) StopCoroutine(rutinaInvulnerabilidad);
        esInvulnerable = false; 

        if (rutinaCura != null) StopCoroutine(rutinaCura);
        rutinaCura = StartCoroutine(EfectoCuraVisual(Color.green));
    }

    public void ActivarDisparoMultiple()
    {
        if (rutinaMultiple != null) StopCoroutine(rutinaMultiple);
        rutinaMultiple = StartCoroutine(RutinaPoderMultiple());

        if (rutinaCura != null) StopCoroutine(rutinaCura);
        rutinaCura = StartCoroutine(EfectoCuraVisual(Color.blue));
    }

    IEnumerator RutinaPoderMultiple()
    {
        tieneDisparoMultiple = true;
        yield return new WaitForSeconds(6f); // Dura 6 segundos
        tieneDisparoMultiple = false;
    }

    void EjecutarDisparo()
    {
        if (tieneDisparoMultiple)
        {
            LanzarUnaBala(0f, true);    // Centro
            LanzarUnaBala(8f, true);    // Arriba
            LanzarUnaBala(-8f, true);   // Abajo
        }
        else
        {
            LanzarUnaBala(0f, false);
        }
    }

    void LanzarUnaBala(float desviacionAngulo, bool esVerde)
    {
        foreach (GameObject bala in poolBalas)
        {
            if (!bala.activeInHierarchy)
            {
                bala.transform.position = puntoDisparo.position;
                
                bala.transform.rotation = transform.rotation * Quaternion.Euler(0, 0, desviacionAngulo);
                
                Disparo scriptDisparo = bala.GetComponent<Disparo>();
                if (scriptDisparo != null) scriptDisparo.ConfigurarColor(esVerde);

                bala.SetActive(true);
                return; 
            }
        }
    }

    IEnumerator EfectoCuraVisual(Color colorEfecto)
    {
        estaCurandose = true; 
        float tiempo = 0f;
        Vector3 escalaDestino = escalaOriginal * multiplicadorCura;

        while (tiempo < tiempoEfecto)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / tiempoEfecto;

            transform.localScale = Vector3.Lerp(escalaDestino, escalaOriginal, t);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(colorEfecto, Color.white, t);
            }

            yield return null;
        }

        transform.localScale = escalaOriginal;
        estaCurandose = false; 

        if (!esInvulnerable && spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    IEnumerator EfectoInvulnerabilidad()
    {
        esInvulnerable = true;
        float tiempo = 0f;
        bool enRojo = false;

        while (tiempo < tiempoInvulnerabilidad)
        {
            if (spriteRenderer != null && !estaCurandose)
            {
                spriteRenderer.color = enRojo ? Color.white : Color.red;
                enRojo = !enRojo;
            }
            
            yield return new WaitForSeconds(0.15f); 
            tiempo += 0.15f;
        }

        esInvulnerable = false;
        if (!estaCurandose && spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    void ChequearBordesPantalla()
    {
        Vector2 posicionActual = transform.position;
        float margen = 0.5f; 

        if (posicionActual.x > anchoPantalla + margen) posicionActual.x = -anchoPantalla - margen;
        else if (posicionActual.x < -anchoPantalla - margen) posicionActual.x = anchoPantalla + margen;

        if (posicionActual.y > altoPantalla + margen) posicionActual.y = -altoPantalla - margen;
        else if (posicionActual.y < -altoPantalla - margen) posicionActual.y = altoPantalla + margen;

        transform.position = posicionActual;
    }
}