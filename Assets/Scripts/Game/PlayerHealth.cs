using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vidas")]
    [SerializeField] private int vidasMaximas = 3;
    [SerializeField] private float tiempoInvulnerabilidad = 1.5f;
    [SerializeField] private float caidaExtra = 10f;

    [Header("Reaparicion")]
    [SerializeField] private Transform puntoReaparicion;

    public int VidasActuales { get; private set; }
    public int VidasMaximas => vidasMaximas;
    public bool EstaInvulnerable => Time.time < finInvulnerabilidad;

    public event Action<int, int> VidasCambiadas;
    public event Action JugadorMuerto;

    private CharacterController controller;
    private PlayerMovement movimiento;
    private Vector3 posicionReaparicion;
    private float finInvulnerabilidad;
    private bool muerto;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        movimiento = GetComponent<PlayerMovement>();
        posicionReaparicion = puntoReaparicion != null ? puntoReaparicion.position : transform.position;
        VidasActuales = vidasMaximas;
    }

    private void Start()
    {
        VidasCambiadas?.Invoke(VidasActuales, vidasMaximas);
    }

    private void Update()
    {
        if (muerto)
            return;

        if (transform.position.y < posicionReaparicion.y - caidaExtra)
            RecibirDanio(1, true);
    }

    public void RecibirDanio(int cantidad = 1, bool ignorarInvulnerabilidad = false)
    {
        if (muerto)
            return;

        if (!ignorarInvulnerabilidad && EstaInvulnerable)
            return;

        VidasActuales = Mathf.Max(0, VidasActuales - cantidad);
        VidasCambiadas?.Invoke(VidasActuales, vidasMaximas);

        if (VidasActuales <= 0)
        {
            StartCoroutine(Morir());
            return;
        }

        StartCoroutine(Reaparecer());
    }

    private IEnumerator Reaparecer()
    {
        finInvulnerabilidad = Time.time + tiempoInvulnerabilidad;

        if (controller != null)
            controller.enabled = false;

        transform.position = posicionReaparicion;

        if (controller != null)
            controller.enabled = true;

        if (movimiento != null)
            movimiento.ReiniciarEstado();

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        float fin = finInvulnerabilidad;

        while (Time.time < fin)
        {
            bool visible = Mathf.FloorToInt(Time.time * 10f) % 2 == 0;
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].enabled = visible;

            yield return null;
        }

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = true;
    }

    private IEnumerator Morir()
    {
        muerto = true;
        JugadorMuerto?.Invoke();

        if (movimiento != null)
            movimiento.enabled = false;

        if (controller != null)
            controller.enabled = false;

        yield return new WaitForSeconds(3f);

        Scene escena = SceneManager.GetActiveScene();
        if (escena.buildIndex >= 0)
            SceneManager.LoadScene(escena.buildIndex);
    }
}
