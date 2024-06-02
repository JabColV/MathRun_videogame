using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonPattern : MonoBehaviour
{
    public static SingletonPattern Instance;

    #region Own variables
    GameObject? player;
    public GameObject? WelcomeInterface;
    public GameObject? MainInterface;
    bool isLoaded = false;
    int coins;
    #endregion

    #region External variables
    FirebaseDatabase database;
    FirebaseAuth firebaseAuth;
    #endregion

    private void Awake()
    {
        // Verificar si ya existe una instancia de FirebaseAuth
        if (Instance == null)
        {
            // Si no existe, asignar esta instancia a la variable Instance 
            Instance = this;
            // y no destruir el objeto al cargar una nueva escena
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            if (Instance != this)
            {
                // Si ya existe una instancia de FirebaseAuth, destruir este objeto
                Destroy(gameObject);
            }
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (database == null)
        {
            Debug.LogError("FirebaseDatabase no se encontró en ButtonStart.");
        }

        if (firebaseAuth == null)
        {
            Debug.LogError("FirebaseAuth no se encontró en ButtonStart.");
        }
        if (WelcomeInterface == null)
        {
            Debug.LogError("WelcomeInterface no está asignado.");
        }
        if (MainInterface == null)
        {
            Debug.LogError("MainInterface no está asignado.");
        }
    }

    public void LoadData(){
        // Asignar las instancias de FirebaseDatabase, FirebaseAuth 
        database = this.GetComponent<FirebaseDatabase>();
        firebaseAuth = GameObject.Find("ButtonStart")?.GetComponent<FirebaseAuth>();
    }

    public GameObject GetPlayer()
    {
        return player;
    }

    public GameObject GetWelcomeInterface()
    {
        return WelcomeInterface;
    }

    public GameObject GetMainInterface()
    {
        return MainInterface;
    }

    public int GetCoins()
    {
        return coins;
    }

    public bool IsLoaded()
    {
        return isLoaded;
    }

    public void SetPlayer(GameObject player)
    {
        this.player = player;
    }

    public void SetCoins(int coins)
    {
        this.coins = coins;
    }

    public void SetIsLoaded(bool isLoaded)
    {
        this.isLoaded = isLoaded;
    }

    public FirebaseDatabase GetDatabase()
    {
        return database;
    }

    public FirebaseAuth GetFirebaseAuth()
    {
        return firebaseAuth;
    }

    public void ClearData()
    {
        player = null;
        isLoaded = false;
        coins = 0;
        GetDatabase().ClearData();
        GetFirebaseAuth().ClearData();
    }
}
