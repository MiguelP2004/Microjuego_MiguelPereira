using UnityEngine;

public class Disparo : MonoBehaviour
{
    public float velocidad = 15f;
    public float tiempoDeVida = 2f; 
    public GameObject prefabImpacto;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Capturamos la pintura
    }

    void OnEnable()
    {
        Invoke("Apagar", tiempoDeVida);
        
        if (rb != null)
        {
            rb.linearVelocity = transform.right * velocidad;
        }
    }

    public void ConfigurarColor(bool esMúltiple)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = esMúltiple ? Color.green : Color.white;
        }
    }

    void Apagar()
    {
        gameObject.SetActive(false);
    }

    public void Morir()
    {
        if (prefabImpacto != null)
        {
            GameObject impacto = Instantiate(prefabImpacto, transform.position, Quaternion.identity);
            EfectoImpacto scriptImpacto = impacto.GetComponent<EfectoImpacto>();
            if (scriptImpacto != null)
            {
                scriptImpacto.IniciarEfecto(new Color(1f, 1f, 1f));
            }
        }

        Apagar();
    }

    void OnDisable()
    {
        CancelInvoke(); 
        if (rb != null) rb.linearVelocity = Vector2.zero; 
    }
}