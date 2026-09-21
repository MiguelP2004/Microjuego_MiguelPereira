using UnityEngine;
using System.Collections;

public class ItemMultiple : MonoBehaviour
{
    public float tiempoDesaparicion = 8f; 
    public AudioClip sonidoCoger;

    void Start()
    {
        Destroy(gameObject, tiempoDesaparicion);
        
        StartCoroutine(EfectoBote());
        StartCoroutine(EfectoLatidoYDesvanecido());
    }

    IEnumerator EfectoLatidoYDesvanecido()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        Vector3 escalaBase = transform.localScale;
        Color[] coloresBase = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            coloresBase[i] = renderers[i].color;
        }

        while (true)
        {
            float pulso = (Mathf.Sin(Time.time * 4f) + 1f) * 0.5f;
            pulso = Mathf.SmoothStep(0f, 1f, pulso);
            transform.localScale = escalaBase * Mathf.Lerp(1f, 1.15f, pulso);

            for (int i = 0; i < renderers.Length; i++)
            {
                Color color = coloresBase[i];
                color.a = coloresBase[i].a * Mathf.Lerp(0.7f, 1f, pulso);
                renderers[i].color = color;
            }

            yield return null;
        }
    }

    IEnumerator EfectoBote()
    {
        Vector3 posInicial = transform.position;
        Vector3 posFinal = posInicial + (Vector3)Random.insideUnitCircle * 1.5f; 
        
        float tiempo = 0f;
        float duracion = 1.2f; 
        float alturaMaxima = 1.3f; 
        int botes = 3; 

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;

            float tHorizontal = 1f - Mathf.Pow(1f - t, 3f); 
            Vector3 posBase = Vector3.Lerp(posInicial, posFinal, tHorizontal);
            
            float alturaActual = alturaMaxima * (1f - t); 
            float bote = Mathf.Abs(Mathf.Sin(t * Mathf.PI * botes)) * alturaActual; 

            transform.position = posBase + new Vector3(0, bote, 0);
            yield return null;
        }

        transform.position = posFinal;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        ControladorNave nave = otro.GetComponent<ControladorNave>();
        if (nave != null)
        {
            nave.ActivarDisparoMultiple();
            
            if (sonidoCoger != null) 
            {
                AudioSource.PlayClipAtPoint(sonidoCoger, Camera.main.transform.position, 0.6f);
            }
            
            Destroy(gameObject); 
        }
    }
}