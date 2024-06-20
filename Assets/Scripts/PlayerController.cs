using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    #region game variables
    public GameObject[] lifes;
    public GameObject[] gems;
    public GameObject[] diamonds;
    public GameObject[] gemEffects;
    public GameObject gem5;
    public GameObject FinalPortal;
    public GameObject box;
    public GameObject ocean;
    public GameObject goggles;
    public GameObject gogglesHelp;
    public GameObject ResumeButton;
    #endregion

    #region bool variables
    bool isJumpping = false;
    bool isInWater = false;
    public bool floorDetected = false;
    public bool heartActive = false;
    bool hasGoggles = false;
    bool isDrowning = false;
    #endregion

    #region audio variables
    public AudioClip jumpAudio;
    public AudioClip yellAudio;
    public AudioClip gameoverAudio;
    public AudioClip winAudio;
    public AudioClip splasAudio;
    public AudioClip gemAudio;
    #endregion

    public TMP_Text gogglesExplanation;
    public TMP_Text BoxTitle;
    private Rigidbody playerRB;
    private float jumpForce = 49.0f;
    private float movementSpeed = 15.0f;
    public float RotationSpeed = 2.0f;
    private Animator anim;
    public float x, y, z;
    private HashSet<GameObject> collidedIslands = new HashSet<GameObject>();
    public MenuPause menuPause;
    public Vector3 lastIsland;
    int lifesNumber;
    int gemsNumber;
    List<BoxCollider> boxColliders;

    SystemPickingUp systemPickingUp;
    SingletonPattern singletonPattern;
    [SerializeField] private float _gravity;
    [SerializeField] private float _fallVelocity;

    public void Start()
    {
        singletonPattern = SingletonPattern.Instance;
        systemPickingUp = GetComponent<SystemPickingUp>();
        playerRB = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        boxColliders = new List<BoxCollider>(ocean.GetComponents<BoxCollider>());
        StartCoroutine(LoadStatus());
        _gravity = 60f; // Gravedad
        if (singletonPattern.IsRestarting())
        {
            transform.position = new Vector3(-3.700000047683716f, 21.304550170898438f, 171.6999969482422f);
            singletonPattern.SetRestarting(false);
        }
    }

    public IEnumerator LoadStatus()
    {
        singletonPattern.SetIsLoaded(false);
        singletonPattern.GetDatabase().GetData();
        yield return new WaitUntil(() => singletonPattern.IsLoaded() == true);
        this.transform.position = singletonPattern.GetDatabase().GetDataUserInfo().position;
        lifesNumber = singletonPattern.GetDatabase().GetDataUserInfo().vidas;
        gemsNumber = singletonPattern.GetDatabase().GetDataUserInfo().gemas;
        systemPickingUp.SetCoins(singletonPattern.GetDatabase().GetDataUserInfo().totalCoins);
        hasGoggles = singletonPattern.GetDatabase().GetDataUserInfo().hasGoggles;
        singletonPattern.SetHasGoggles(hasGoggles);

        //Life configuration
        if (lifesNumber == 0)
        {
            menuPause.Restart();
            lifesNumber = 3;
        }
        int rest = 3 - lifesNumber;
        singletonPattern.SetLifes(lifesNumber);
        for (int i = rest; i > 0; i--)
        {
            DesactivateLife(3 - i);
        }

        //Gems configuration
        singletonPattern.SetGems(gemsNumber);
        if (gemsNumber > 0)
        {
            box.SetActive(true);
        }
        for (int i = gemsNumber; i > 0; i--)
        {
            gems[i-1].SetActive(true);
            gemEffects[i-1].SetActive(false);
            diamonds[i-1].SetActive(false);
        }
        
        singletonPattern.SetPlayer(this.gameObject);
        singletonPattern.SetPlayerController(this.GetComponent<PlayerController>());
    }

    public void DesactivateLife(int indice)
    {
        lifes[indice].SetActive(false);
    }

    public void ActivateLife(int indice)
    {
        lifes[indice].SetActive(true);
    }

    public void loseLife()
    {
        lifesNumber = lifesNumber - 1;
        if (lifesNumber == 0)
        {
            singletonPattern.PlaySoundEffect(gameoverAudio, 1.0f);
            menuPause.LastMemory();
        }
        DesactivateLife(lifesNumber);
        singletonPattern.SetLifes(lifesNumber);
    }

    public void winLife()
    {
        if (lifesNumber == 3)
        {
            return;
        }
        heartActive = true;
        ActivateLife(lifesNumber);
        lifesNumber = lifesNumber + 1;
        singletonPattern.SetLifes(lifesNumber);
    }

    public bool GetHeartActive()
    {
        return heartActive;
    }

    public void SetHeartActive(bool heartActive)
    {
        this.heartActive = heartActive;
    }

    public void DiamondDeactivation()
    {
        box.SetActive(false);
        for (int i = 0; i < gemsNumber; i++)
        {
            gems[i].SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Island") && !collidedIslands.Contains(other.gameObject))
        {
            lastIsland = this.gameObject.transform.position;
            collidedIslands.Add(other.gameObject);
            singletonPattern.SetPlayer(this.gameObject);
            singletonPattern.GetDatabase().UpdateData(Vector3.zero);
        }

        if (other.gameObject.CompareTag("bird"))
        {
            singletonPattern.PlaySoundEffect(yellAudio, 1.0f);
            loseLife();
        }

        if (other.gameObject.CompareTag("Ocean"))
        {
            singletonPattern.PlaySoundEffect(splasAudio, 1.0f);
            isInWater = true;
            singletonPattern.SetIsInWater(isInWater);
        }

        if (other.gameObject.CompareTag("gem"))
        {
            singletonPattern.SetGemGotten(true);
            singletonPattern.PlaySoundEffect(gemAudio, 1.0f);
            Destroy(other.gameObject);
            gemEffects[singletonPattern.GetGems()].SetActive(false);
            box.SetActive(true);
            gems[singletonPattern.GetGems()].SetActive(true);
            if (gemsNumber == 3)
            {
                gem5.SetActive(true);
                gemEffects[singletonPattern.GetGems()+1].SetActive(true);
            }
            //Aumentar el número de gemas
            gemsNumber += 1;
            singletonPattern.SetGems(gemsNumber);
            singletonPattern.GetDatabase().UpdateData(lastIsland);
        }

        if (other.gameObject.CompareTag("portal"))
        {
            singletonPattern.SetWin(true);
            singletonPattern.PlaySoundEffect(winAudio, 1.0f);
            singletonPattern.GetDatabase().UpdateData(Vector3.zero);
        }
        if (other.gameObject.CompareTag("pinchos"))
        {
            singletonPattern.PlaySoundEffect(yellAudio, 1.0f);
            loseLife();
        }
    }

    void ShowGogglesHelp()
    {
        if (gemsNumber == 3 && !hasGoggles)
        {
            gogglesExplanation.text = "Presiona E para usarlo, debes arrojarte al mar y bucar el resto de las gemas, no lo podrás hacer sin las gafas de buceo.";
            gogglesHelp.SetActive(true);
            gogglesHelp.transform.Rotate(Vector3.up, 20.0f * Time.deltaTime);
        }
    }

    void TakeGoggles()
    {
        if (Input.GetKeyDown(KeyCode.E) && gemsNumber == 3 && !hasGoggles)
        {
            gogglesHelp.SetActive(false);
            gogglesExplanation.enabled = false;
            goggles.SetActive(true);
            hasGoggles = true;
            singletonPattern.SetHasGoggles(hasGoggles);
            singletonPattern.GetDatabase().UpdateData(lastIsland);
        }
    }

    private IEnumerator Drown()
    {
        isDrowning = true;
        anim.SetBool("drowning", true);
        
        yield return new WaitForSeconds(2); 
        loseLife();
        if (lifesNumber > 0)
        {
            singletonPattern.GetDatabase().UpdateData(lastIsland);
            menuPause.LastMemory();
            isDrowning = false; 
        }
    }

    void Update()
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Forward");

        anim.SetFloat("VelX", x);
        anim.SetFloat("VelY", z);

        Vector3 floor = transform.TransformDirection(Vector3.down);
        if (Physics.Raycast(transform.position, floor, 0.6f))
        {
            floorDetected = true;
            anim.SetBool("landed", true);
        }
        else
        {
            floorDetected = false;
            anim.SetBool("landed", false);
            anim.SetBool("jumped", false);
        }

        if (Input.GetButtonDown("Jump") && floorDetected && !isInWater)
        {
            isJumpping = true;
        } 
        else
        {
            anim.SetBool("jumped", false);
        }

        if (isInWater)
        {
            WaterConfiguration();
        }

        if (isInWater && !hasGoggles && !isDrowning)
        {
            StartCoroutine(Drown());
        }

        ShowGogglesHelp();
        
        TakeGoggles();

        goggles.SetActive(hasGoggles);

        HasFullGems();

        if (singletonPattern.GetWin()){
            ResumeButton.SetActive(false);
            BoxTitle.text = "¡Ganaste!";
            this.gameObject.SetActive(false);
        }
    }

    void HasFullGems()
    {
        if (gemsNumber == 5)
        {
            //Aparece un portal en el agua para finalizar el juego
            FinalPortal.SetActive(true);
        }
    }
    void playerRBWaterConfig()
    {
        if (isInWater)
        {
            _gravity = 0.0f;
            playerRB.useGravity = false;

            Vector3 gravity = -0.4f * Vector3.down;
            playerRB.AddForce(gravity, ForceMode.Acceleration);

            Quaternion deltaRotation = Quaternion.Euler(0, x * Time.deltaTime * RotationSpeed, 0);
            playerRB.MoveRotation(playerRB.rotation * deltaRotation);

            Vector3 waterMovement = transform.TransformDirection(new Vector3(0, y, 0)) * (movementSpeed + 2.0f) * Time.deltaTime;
            Vector3 newPosition = playerRB.position + waterMovement;
            playerRB.MovePosition(newPosition);
        }
        else
        {
            _gravity = 60.0f;
            playerRB.useGravity = true;
        }
    }

    void WaterConfiguration()
    {
        if (isInWater)
        {
            y = Input.GetAxis("Vertical");

            anim.SetBool("in_water", true);
            float upperLimit = -38.36f;

            Vector3 newPosition = transform.position;
            if (newPosition.y < upperLimit)
            {
                boxColliders[0].enabled = false;
                boxColliders[1].enabled = true;
            }
            else if (newPosition.y > upperLimit)
            {
                boxColliders[0].enabled = true;
                boxColliders[1].enabled = false;
                isInWater = false; //Debe ser analizado mejor
            }
        }
        else
        {
            anim.SetBool("in_water", false);
            singletonPattern.SetIsInWater(isInWater);
        }
    }

    void FixedUpdate()
    {
        Vector3 movementDirection = transform.TransformDirection(new Vector3(x, 0, z).normalized);
        playerRB.MovePosition(transform.position + movementDirection * movementSpeed * Time.deltaTime);

        playerRBWaterConfig();

        Vector3 vecGravity = new Vector3(0, -_gravity, 0);
        playerRB.AddForce(vecGravity * playerRB.mass);

        if (isJumpping && floorDetected && !isInWater)
        {
            playerRB.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            singletonPattern.PlaySoundEffect(jumpAudio, 1.0f);
            isJumpping = false;
        }
    }
}





