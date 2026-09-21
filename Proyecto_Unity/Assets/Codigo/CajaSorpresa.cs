using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CajaSorpresa : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 2f;
    public float velocidadRotacion = 45f; 
    
    [Header("Vida y UI")]
    private int vidaMaxima = 4;
    private int vidaActual;
    public Image rellenoBarraVida; 
    public Transform canvasVida;   
    public Vector3 offsetBarra = new Vector3(0f, 1.5f, 0f);
    
    [Header("Efectos y Drops")]
    public GameObject prefabCorazon;
    public GameObject prefabMultiple; 
    
    [Header("Sprites de la Caja Sorpresa")]
    public Sprite spriteCajaCorazon;
    public Sprite spriteCajaMultiple;
    
    public float duracionGolpe = 0.15f;

    public AudioClip romperCaja;

    private Coroutine rutinaGolpe;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    private bool contieneCorazon;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; 
    }

    void Start()
    {
        vidaMaxima = 4; 
        vidaActual = vidaMaxima;

        if (canvasVida != null) canvasVida.gameObject.SetActive(false);

        // O sale corazón o disparo múltiple, 50% de probabilidad
        contieneCorazon = (Random.value > 0.5f);
        
        // Le ponemos la "pegatina" correspondiente a la caja
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = contieneCorazon ? spriteCajaCorazon : spriteCajaMultiple;
        }

        Vector3 direccionVuelo = Vector3.zero;
        GameObject jugador = GameObject.Find("Jugador"); 
        
        if (jugador != null)
            direccionVuelo = (jugador.transform.position - transform.position).normalized;
        else
            direccionVuelo = Random.insideUnitCircle.normalized;

        if (rb != null) rb.linearVelocity = direccionVuelo * velocidad;
    }

    void Update()
    {
        transform.Rotate(0, 0, velocidadRotacion * Time.deltaTime);
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
        Disparo disparo = otro.GetComponent<Disparo>();
        if (disparo != null)
        {
            disparo.Morir();
            RecibirDano();
        }
    }

    void RecibirDano()
    {
        vidaActual--;

        if (canvasVida != null && !canvasVida.gameObject.activeSelf)
        {
            canvasVida.gameObject.SetActive(true);
        }
        
        if (rellenoBarraVida != null)
        {
            float porcentaje = (float)vidaActual / vidaMaxima;
            rellenoBarraVida.fillAmount = porcentaje;
        }

        if (vidaActual <= 0)
        {
            if (romperCaja != null) AudioSource.PlayClipAtPoint(romperCaja, Camera.main.transform.position);

            GameObject dropElegido = contieneCorazon ? prefabCorazon : prefabMultiple;
            
            if (dropElegido != null) 
            {
                Instantiate(dropElegido, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
        else
        {
            if (rutinaGolpe != null) StopCoroutine(rutinaGolpe);
            rutinaGolpe = StartCoroutine(EfectoGolpeBrillo());
        }
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
}