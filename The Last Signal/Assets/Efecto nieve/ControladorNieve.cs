using UnityEngine;

public class ControladorNieve : MonoBehaviour
{
    public ParticleSystem sistemaNieve;
    public float velocidadViento = 1f;

    void Update()
    {
        ParticleSystem.VelocityOverLifetimeModule velocidad = sistemaNieve.velocityOverLifetime;
        // Simula un viento suave
        velocidad.x = Mathf.PerlinNoise(Time.time * 0.1f, 0) * velocidadViento - (velocidadViento / 2);
    }
}