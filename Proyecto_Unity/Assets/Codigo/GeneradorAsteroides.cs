using UnityEngine;

public class GeneradorAsteroides : MonoBehaviour
{
    [Header("Asteroides")]
    public GameObject prefabAsteroide;
    private float tiempoEntreSpawns = 1.2f;
    private float temporizadorAsteroides;
    private float tiempoPartida;

    [Header("Cajas Sorpresa")]
    public GameObject prefabCaja;
    private float temporizadorCaja;
    private float tiempoSiguienteCaja;

    private Camera camara;

    void Start()
    {
        camara = Camera.main;
        
        tiempoSiguienteCaja = Random.Range(10f, 15f);
    }

    void Update()
    {
        tiempoPartida += Time.deltaTime;

        if (tiempoPartida >= 30f)
        {
            tiempoEntreSpawns = 0.8f;
        }
        else if (tiempoPartida >= 20f)
        {
            tiempoEntreSpawns = 1f;
        }

        temporizadorAsteroides += Time.deltaTime;
        if (temporizadorAsteroides >= tiempoEntreSpawns)
        {
            SpawnearObjeto(prefabAsteroide);
            temporizadorAsteroides = 0f;
        }

        if (prefabCaja != null)
        {
            temporizadorCaja += Time.deltaTime;
            if (temporizadorCaja >= tiempoSiguienteCaja)
            {
                SpawnearObjeto(prefabCaja);
                
                temporizadorCaja = 0f;
                tiempoSiguienteCaja = Random.Range(10f, 15f); // 10 Y 15 segundos
            }
        }
    }

    void SpawnearObjeto(GameObject prefabAInstanciar)
    {
        float altoCamara = camara.orthographicSize;
        float anchoCamara = altoCamara * camara.aspect;

        Vector3 posicionSpawn = Vector3.zero;
        int ladoAleatorio = Random.Range(0, 4);

        switch (ladoAleatorio)
        {
            case 0: posicionSpawn = new Vector3(Random.Range(-anchoCamara, anchoCamara), altoCamara + 2f, 0f); break;
            case 1: posicionSpawn = new Vector3(Random.Range(-anchoCamara, anchoCamara), -altoCamara - 2f, 0f); break;
            case 2: posicionSpawn = new Vector3(-anchoCamara - 2f, Random.Range(-altoCamara, altoCamara), 0f); break;
            case 3: posicionSpawn = new Vector3(anchoCamara + 2f, Random.Range(-altoCamara, altoCamara), 0f); break;
        }

        Instantiate(prefabAInstanciar, posicionSpawn, Quaternion.identity);
    }
}