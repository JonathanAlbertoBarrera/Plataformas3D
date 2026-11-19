using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemigoNPC : MonoBehaviour
{
    public enum Estado
    {
        Patrulla,
        Persecucion,
        Ataque
    }

    [SerializeField] private PlayerHealth jugador;
    [SerializeField] private Transform[] puntosPatrulla;

    [Header("Movimiento")]
    [SerializeField, Min(0.1f)] private float velocidadPatrulla = 2f;
    [SerializeField, Min(0.1f)] private float velocidadPersecucion = 3.8f;
    [SerializeField, Min(0.1f)] private float rangoDeteccion = 4.5f;
    [SerializeField, Min(0.1f)] private float rangoPerdida = 7f;

    [Header("Ataque")]
    [SerializeField, Min(0.1f)] private float rangoAtaque = 1.25f;
    [SerializeField, Min(1)] private int danio = 1;
    [SerializeField, Min(0.1f)] private float intervaloAtaque = 1f;

    private const float IntervaloDecision = 0.15f;
    private const float DistanciaAlNavMesh = 0.6f;
    private const float MargenLlegada = 0.15f;
    private const float DesplazamientoParaRecalcular = 0.2f;

    public Estado EstadoActual { get; private set; }

    private NavMeshAgent agente;
    private CharacterController controladorJugador;
    private NavMeshPath camino;
    private int siguientePunto;
    private float siguienteDecision;
    private float siguienteAtaque;

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        camino = new NavMeshPath();
        if (jugador != null)
            controladorJugador = jugador.GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!agente.isOnNavMesh || Time.time < siguienteDecision)
            return;

        siguienteDecision = Time.time + IntervaloDecision;
        ActualizarEstado();
        if (EstadoActual == Estado.Patrulla)
            Patrullar();
    }

    private void ActualizarEstado()
    {
        if (!LocalizarJugador(out Vector3 pies, out Vector3 destino))
        {
            CambiarEstado(Estado.Patrulla);
            return;
        }

        if (Vector3.Distance(transform.position, pies) <= rangoAtaque &&
            !agente.Raycast(destino, out _))
        {
            CambiarEstado(Estado.Ataque);
            Atacar();
            return;
        }

        if (EstadoActual == Estado.Persecucion && agente.hasPath && !agente.isPathStale &&
            Vector3.Distance(agente.destination, destino) < DesplazamientoParaRecalcular)
            return;

        if (CalcularCamino(destino) && agente.SetPath(camino))
            CambiarEstado(Estado.Persecucion);
        else
            CambiarEstado(Estado.Patrulla);
    }

    private bool LocalizarJugador(out Vector3 pies, out Vector3 destino)
    {
        pies = default;
        destino = default;
        if (jugador == null || jugador.VidasActuales <= 0 || controladorJugador == null)
            return false;

        Bounds limites = controladorJugador.bounds;
        pies = new Vector3(limites.center.x, limites.min.y, limites.center.z);
        float alcance = EstadoActual == Estado.Patrulla
            ? rangoDeteccion
            : Mathf.Max(rangoDeteccion, rangoPerdida);
        return Vector3.Distance(transform.position, pies) <= alcance &&
            ProyectarEnNavMesh(pies, out destino);
    }

    private void CambiarEstado(Estado estado)
    {
        if (EstadoActual == estado)
            return;

        EstadoActual = estado;
        agente.isStopped = estado == Estado.Ataque;
        if (estado == Estado.Persecucion)
        {
            agente.speed = velocidadPersecucion;
        }
        else
        {
            agente.ResetPath();
            agente.speed = velocidadPatrulla;
        }
    }

    private void Patrullar()
    {
        if (puntosPatrulla == null || agente.pathPending ||
            (agente.hasPath && agente.remainingDistance > agente.stoppingDistance + MargenLlegada))
            return;

        agente.speed = velocidadPatrulla;
        for (int i = 0; i < puntosPatrulla.Length; i++)
        {
            Transform punto = puntosPatrulla[siguientePunto];
            siguientePunto = (siguientePunto + 1) % puntosPatrulla.Length;
            if (punto != null && ProyectarEnNavMesh(punto.position, out Vector3 destino) &&
                CalcularCamino(destino) && agente.SetPath(camino))
                return;
        }
    }

    private bool ProyectarEnNavMesh(Vector3 posicion, out Vector3 destino)
    {
        var filtro = new NavMeshQueryFilter { agentTypeID = agente.agentTypeID, areaMask = agente.areaMask };
        bool encontrado = NavMesh.SamplePosition(posicion, out NavMeshHit hit, DistanciaAlNavMesh, filtro);
        destino = hit.position;
        return encontrado;
    }

    private bool CalcularCamino(Vector3 destino)
    {
        return agente.CalculatePath(destino, camino) && camino.status == NavMeshPathStatus.PathComplete;
    }

    private void Atacar()
    {
        if (Time.time < siguienteAtaque || jugador.EstaInvulnerable)
            return;

        siguienteAtaque = Time.time + intervaloAtaque;
        jugador.RecibirDanio(danio);
    }
}
