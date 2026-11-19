using UnityEngine;

public class EnemigoOscilante : MonoBehaviour
{
    [SerializeField] private Vector3 direccion = Vector3.right;
    [SerializeField] private float distancia = 2f;
    [SerializeField] private float velocidad = 1.5f;

    private Vector3 origen;

    private void Start()
    {
        origen = transform.position;
    }

    private void Update()
    {
        Vector3 dir = direccion.sqrMagnitude > 0.001f ? direccion.normalized : Vector3.right;
        transform.position = origen + dir * (Mathf.Sin(Time.time * velocidad) * distancia);
    }

    public void Configurar(Vector3 nuevaDireccion, float nuevaDistancia, float nuevaVelocidad)
    {
        direccion = nuevaDireccion;
        distancia = nuevaDistancia;
        velocidad = nuevaVelocidad;
    }
}
