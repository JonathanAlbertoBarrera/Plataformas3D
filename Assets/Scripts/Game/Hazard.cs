using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hazard : MonoBehaviour
{
    [SerializeField] private int danio = 1;
    [SerializeField] private float intervalo = 0.75f;

    private float siguienteDanio;

    private void OnTriggerStay(Collider other)
    {
        if (Time.time < siguienteDanio)
            return;

        PlayerHealth salud = other.GetComponentInParent<PlayerHealth>();
        if (salud == null)
            return;

        salud.RecibirDanio(danio);
        siguienteDanio = Time.time + intervalo;
    }
}
