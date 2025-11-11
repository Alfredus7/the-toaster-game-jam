using UnityEngine;

public class OptimizarParticulas : MonoBehaviour
{
    public ParticleSystem particula;
    public float distanciaMaxima = 100f;
    private ParticleSystem.EmissionModule emision;
    private float tasaEmisionInicial;

    void Start()
    {
        emision = particula.emission;
        tasaEmisionInicial = emision.rateOverTime.constant;
    }

    void Update()
    {
        float distancia = Vector3.Distance(Camera.main.transform.position, transform.position);
        if (distancia > distanciaMaxima)
        {
            emision.rateOverTime = 0; // Detiene la emisión si está muy lejos
        }
        else
        {
            // Reduce la tasa de emisión según la distancia
            float factorDistancia = 1 - Mathf.Clamp01(distancia / distanciaMaxima);
            emision.rateOverTime = tasaEmisionInicial * factorDistancia;
        }
    }
}