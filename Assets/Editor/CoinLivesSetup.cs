using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CoinLivesSetup
{
    private const string RutaEscena = "Assets/Scenes/Nivel01.unity";
    private const string RutaMoneda = "Assets/Prefabs/Moneda.prefab";
    private const string RutaObstaculo = "Assets/Prefabs/Obstaculo.prefab";

    [MenuItem("Tools/Plataformas3D/Configurar monedas y vidas")]
    public static void ConfigurarMenu()
    {
        Configurar(false);
    }

    public static void ConfigurarBatch()
    {
        EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);
        Configurar(true);
    }

    [InitializeOnLoadMethod]
    private static void RegistrarAutoConfiguracion()
    {
        EditorApplication.delayCall += IntentarAutoConfigurar;
        EditorApplication.playModeStateChanged += CambioModoJuego;
        EditorSceneManager.sceneOpened += EscenaAbierta;
    }

    private static void CambioModoJuego(PlayModeStateChange estado)
    {
        if (estado == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += IntentarAutoConfigurar;
    }

    private static void EscenaAbierta(Scene escena, OpenSceneMode modo)
    {
        if (escena.path.Replace('\\', '/').EndsWith("Assets/Scenes/Nivel01.unity"))
            EditorApplication.delayCall += IntentarAutoConfigurar;
    }

    private static void IntentarAutoConfigurar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling ||
            EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += IntentarAutoConfigurar;
            return;
        }

        Scene escena = SceneManager.GetActiveScene();
        if (!escena.IsValid())
            return;

        string nombre = escena.name;
        string ruta = escena.path.Replace('\\', '/');
        bool esNivel = nombre.Contains("Nivel01") || ruta.EndsWith("Assets/Scenes/Nivel01.unity");
        if (!esNivel)
            return;

        if (GameObject.Find("SistemasMonedasVidas") != null)
            return;

        Configurar(true);
    }

    private static void Configurar(bool automatico)
    {
        if (EditorApplication.isPlaying)
        {
            if (!automatico)
                EditorUtility.DisplayDialog("Monedas y vidas", "Sal del modo Play antes de configurar.", "OK");
            return;
        }

        Scene escena = SceneManager.GetActiveScene();

        GameObject jugador = GameObject.Find("Player");
        if (jugador == null)
        {
            PlayerMovement movimiento = Object.FindAnyObjectByType<PlayerMovement>();
            if (movimiento != null)
                jugador = movimiento.gameObject;
        }

        if (jugador == null)
        {
            Debug.LogError("[MonedasVidas] No encontre un objeto llamado 'Player' en la escena.");
            return;
        }

        try { jugador.tag = "Player"; } catch { }

        if (jugador.GetComponent<PlayerHealth>() == null)
            AgregarComponente<PlayerHealth>(jugador);

        GameObject sistemas = GameObject.Find("SistemasMonedasVidas");
        if (sistemas == null)
        {
            sistemas = new GameObject("SistemasMonedasVidas");
            RegistrarCreado(sistemas, "Crear sistemas de monedas y vidas");
        }

        if (sistemas.GetComponent<GameManager>() == null)
            AgregarComponente<GameManager>(sistemas);

        if (sistemas.GetComponent<GameHUD>() == null)
            AgregarComponente<GameHUD>(sistemas);

        Material materialMoneda = ObtenerMaterial(
            "Assets/Materials/MatMoneda.mat",
            "MatMoneda",
            new Color(1f, 0.78f, 0.05f),
            new Color(1f, 0.55f, 0f) * 0.8f);

        Material materialPeligro = ObtenerMaterial(
            "Assets/Materials/MatPeligro.mat",
            "MatPeligro",
            new Color(0.85f, 0.08f, 0.08f),
            new Color(0.5f, 0f, 0f));

        GameObject prefabMoneda = ObtenerOCrearMoneda(materialMoneda);
        GameObject prefabObstaculo = ObtenerOCrearObstaculo(materialPeligro);

        ColocarMonedas(prefabMoneda, escena, jugador.transform);
        ColocarObstaculos(prefabObstaculo, escena);

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena, RutaEscena);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[MonedasVidas] Configuracion completada. Monedas, obstaculos, vidas y HUD listos.");
    }

    private static Material ObtenerMaterial(string ruta, string nombre, Color colorBase, Color emision)
    {
        Material existente = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (existente != null)
            return existente;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        material.name = nombre;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", colorBase);

        if (material.HasProperty("_Color"))
            material.SetColor("_Color", colorBase);

        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emision);
        }

        AsegurarCarpeta("Assets/Materials");
        AssetDatabase.CreateAsset(material, ruta);
        AssetDatabase.SaveAssets();
        return material;
    }

    private static GameObject ObtenerOCrearMoneda(Material material)
    {
        GameObject existente = AssetDatabase.LoadAssetAtPath<GameObject>(RutaMoneda);
        if (existente != null)
            return existente;

        AsegurarCarpeta("Assets/Prefabs");

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Moneda";

        Object.DestroyImmediate(go.GetComponent<Collider>());

        SphereCollider trigger = go.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 1.5f;
        AgregarCuerpoCinematico(go);

        go.transform.localScale = new Vector3(0.35f, 0.05f, 0.35f);
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = material;

        go.AddComponent<Coin>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, RutaMoneda);
        Object.DestroyImmediate(go);
        AssetDatabase.SaveAssets();
        return prefab;
    }

    private static GameObject ObtenerOCrearObstaculo(Material material)
    {
        GameObject existente = AssetDatabase.LoadAssetAtPath<GameObject>(RutaObstaculo);
        if (existente != null)
            return existente;

        AsegurarCarpeta("Assets/Prefabs");

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Obstaculo";
        go.transform.localScale = Vector3.one * 0.8f;

        SphereCollider collider = go.GetComponent<SphereCollider>();
        if (collider != null)
            collider.isTrigger = true;
        AgregarCuerpoCinematico(go);

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = material;

        go.AddComponent<Hazard>();
        go.AddComponent<EnemigoOscilante>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, RutaObstaculo);
        Object.DestroyImmediate(go);
        AssetDatabase.SaveAssets();
        return prefab;
    }

    private static void AgregarCuerpoCinematico(GameObject go)
    {
        Rigidbody cuerpo = go.AddComponent<Rigidbody>();
        cuerpo.useGravity = false;
        cuerpo.isKinematic = true;
    }

    private static void ColocarMonedas(GameObject prefab, Scene escena, Transform jugador)
    {
        if (GameObject.Find("Monedas") != null)
            return;

        GameObject raiz = new GameObject("Monedas");
        RegistrarCreado(raiz, "Crear monedas");

        string[] nombres = { "Plataforma01", "Plataforma02", "Plataforma03", "Suelo" };

        foreach (string nombre in nombres)
        {
            GameObject plataforma = GameObject.Find(nombre);
            if (plataforma == null)
                continue;

            Renderer renderer = plataforma.GetComponentInChildren<Renderer>();
            if (renderer == null)
                continue;

            Bounds bounds = renderer.bounds;
            int cantidad = nombre == "Suelo" ? 3 : 2;

            for (int i = 0; i < cantidad; i++)
            {
                float t = cantidad == 1 ? 0.5f : (i + 0.5f) / cantidad;
                float ancho = Mathf.Max(0.2f, bounds.size.x - 1.6f);
                Vector3 posicion = new Vector3(
                    bounds.center.x + (t - 0.5f) * ancho,
                    bounds.max.y + 0.9f,
                    bounds.center.z);

                if (nombre == "Suelo" && i == 1)
                {
                    posicion.x = Mathf.Clamp(jugador.position.x, bounds.min.x, bounds.max.x);
                    posicion.z = Mathf.Clamp(jugador.position.z + 3f, bounds.min.z, bounds.max.z);
                }

                GameObject moneda = (GameObject)PrefabUtility.InstantiatePrefab(prefab, escena);
                moneda.transform.position = posicion;
                moneda.transform.SetParent(raiz.transform, true);
                RegistrarCreado(moneda, "Crear moneda");
            }
        }
    }

    private static void ColocarObstaculos(GameObject prefab, Scene escena)
    {
        if (GameObject.Find("Obstaculos") != null)
            return;

        GameObject raiz = new GameObject("Obstaculos");
        RegistrarCreado(raiz, "Crear obstaculos");

        string[] nombres = { "Plataforma02", "Plataforma03" };

        foreach (string nombre in nombres)
        {
            GameObject plataforma = GameObject.Find(nombre);
            if (plataforma == null)
                continue;

            Renderer renderer = plataforma.GetComponentInChildren<Renderer>();
            if (renderer == null)
                continue;

            Bounds bounds = renderer.bounds;
            GameObject obstaculo = (GameObject)PrefabUtility.InstantiatePrefab(prefab, escena);
            obstaculo.transform.position = new Vector3(bounds.center.x, bounds.max.y + 1f, bounds.center.z);
            obstaculo.transform.SetParent(raiz.transform, true);

            EnemigoOscilante movimiento = obstaculo.GetComponent<EnemigoOscilante>();
            if (movimiento != null)
                movimiento.Configurar(Vector3.right, Mathf.Max(1f, bounds.size.x * 0.25f), 1.4f);

            RegistrarCreado(obstaculo, "Crear obstaculo");
        }

        if (raiz.transform.childCount == 0)
        {
            GameObject jugador = GameObject.Find("Player");
            Vector3 posicion = jugador != null
                ? jugador.transform.position + new Vector3(3f, 1f, 0f)
                : new Vector3(3f, 1f, 0f);

            GameObject obstaculo = (GameObject)PrefabUtility.InstantiatePrefab(prefab, escena);
            obstaculo.transform.position = posicion;
            obstaculo.transform.SetParent(raiz.transform, true);
            RegistrarCreado(obstaculo, "Crear obstaculo");
        }
    }

    private static T AgregarComponente<T>(GameObject go) where T : Component
    {
        return Application.isBatchMode ? go.AddComponent<T>() : Undo.AddComponent<T>(go);
    }

    private static void RegistrarCreado(GameObject go, string nombre)
    {
        if (!Application.isBatchMode)
            Undo.RegisterCreatedObjectUndo(go, nombre);
    }

    private static void AsegurarCarpeta(string ruta)
    {
        if (!Directory.Exists(ruta))
            Directory.CreateDirectory(ruta);
    }
}
