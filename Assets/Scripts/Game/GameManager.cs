using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instancia { get; private set; }

    [SerializeField] private int monedas;

    public int Monedas => monedas;
    public event Action<int> MonedasCambiadas;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
    }

    private void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }

    public void SumarMoneda(int cantidad = 1)
    {
        monedas += cantidad;
        MonedasCambiadas?.Invoke(monedas);
    }

    public void ReiniciarMonedas()
    {
        monedas = 0;
        MonedasCambiadas?.Invoke(monedas);
    }
}
