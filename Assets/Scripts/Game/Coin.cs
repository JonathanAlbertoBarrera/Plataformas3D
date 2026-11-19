using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    [SerializeField] private int valor = 1;
    [SerializeField] private float velocidadRotacion = 180f;
    [SerializeField] private float amplitudFlotacion = 0.12f;
    [SerializeField] private float velocidadFlotacion = 2f;

    private Vector3 posicionInicial;
    private bool recogida;

    private void Start()
    {
        posicionInicial = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime, Space.World);

        Vector3 posicion = posicionInicial;
        posicion.y += Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        transform.position = posicion;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (recogida)
            return;

        if (other.GetComponentInParent<PlayerHealth>() == null)
            return;

        recogida = true;
        GameManager.Instancia?.SumarMoneda(valor);
        Destroy(gameObject);
    }
}
