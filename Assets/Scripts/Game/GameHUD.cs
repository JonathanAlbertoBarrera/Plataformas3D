using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerHealth playerHealth;

    private Text textoMonedas;
    private Text textoVidas;
    private Text textoMensaje;
    private Font fuente;

    private void Awake()
    {
        CrearInterfaz();
    }

    private void Start()
    {
        if (gameManager == null)
            gameManager = GameManager.Instancia;

        if (playerHealth == null)
            playerHealth = Object.FindAnyObjectByType<PlayerHealth>();

        if (gameManager != null)
        {
            gameManager.MonedasCambiadas += ActualizarMonedas;
            ActualizarMonedas(gameManager.Monedas);
        }

        if (playerHealth != null)
        {
            playerHealth.VidasCambiadas += ActualizarVidas;
            playerHealth.JugadorMuerto += MostrarGameOver;
            ActualizarVidas(playerHealth.VidasActuales, playerHealth.VidasMaximas);
        }

        if (textoMensaje != null)
            textoMensaje.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.MonedasCambiadas -= ActualizarMonedas;

        if (playerHealth != null)
        {
            playerHealth.VidasCambiadas -= ActualizarVidas;
            playerHealth.JugadorMuerto -= MostrarGameOver;
        }
    }

    private void Update()
    {
        if (playerHealth != null && playerHealth.VidasActuales <= 0 &&
            Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void ActualizarMonedas(int cantidad)
    {
        if (textoMonedas != null)
            textoMonedas.text = $"Monedas: {cantidad}";
    }

    private void ActualizarVidas(int actuales, int maximas)
    {
        if (textoVidas != null)
            textoVidas.text = $"Vidas: {actuales}/{maximas}";
    }

    private void MostrarGameOver()
    {
        if (textoMensaje == null)
            return;

        textoMensaje.text = "Game Over\nPresiona R para reiniciar";
        textoMensaje.gameObject.SetActive(true);
    }

    private void CrearInterfaz()
    {
        fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fuente == null)
            fuente = Font.CreateDynamicFontFromOSFont("Arial", 28);

        GameObject canvasGO = new GameObject("CanvasHUD", typeof(Canvas), typeof(CanvasScaler));
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        textoMonedas = CrearTexto(canvasGO.transform, "TextoMonedas", new Vector2(40f, -40f), new Vector2(420f, 60f), 32, TextAnchor.UpperLeft);
        textoVidas = CrearTexto(canvasGO.transform, "TextoVidas", new Vector2(40f, -100f), new Vector2(420f, 60f), 32, TextAnchor.UpperLeft);

        textoMensaje = CrearTexto(canvasGO.transform, "TextoMensaje", Vector2.zero, new Vector2(1000f, 200f), 52, TextAnchor.MiddleCenter);
        RectTransform rtMensaje = textoMensaje.rectTransform;
        rtMensaje.anchorMin = new Vector2(0.5f, 0.5f);
        rtMensaje.anchorMax = new Vector2(0.5f, 0.5f);
        rtMensaje.pivot = new Vector2(0.5f, 0.5f);
        rtMensaje.anchoredPosition = Vector2.zero;
    }

    private Text CrearTexto(Transform padre, string nombre, Vector2 posicion, Vector2 tamano, int tamanoFuente, TextAnchor alineacion)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(padre, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamano;

        Text text = go.GetComponent<Text>();
        text.font = fuente;
        text.fontSize = tamanoFuente;
        text.alignment = alineacion;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return text;
    }
}
