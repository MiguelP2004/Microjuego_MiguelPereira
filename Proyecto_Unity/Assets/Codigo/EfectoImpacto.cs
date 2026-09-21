using UnityEngine;
using System.Collections;

public class EfectoImpacto : MonoBehaviour
{
    [Header("Animación")]
    public Sprite[] spritesAnimacion;
    public float tiempoPorFrame = 0.05f;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>(); 
            Color c = spriteRenderer.color;
            c.a = 0.75f;
            spriteRenderer.color = c;
        }
    }

    public void IniciarEfecto(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
        
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        
        StartCoroutine(AnimarYDestruir());
    }

    IEnumerator AnimarYDestruir()
    {
        if (spritesAnimacion != null && spritesAnimacion.Length > 0 && spriteRenderer != null)
        {
            for (int i = 0; i < spritesAnimacion.Length; i++)
            {
                spriteRenderer.sprite = spritesAnimacion[i];
                yield return new WaitForSeconds(tiempoPorFrame);
            }
        }

        Destroy(gameObject);
    }
}