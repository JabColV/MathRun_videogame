using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SystemPickingUp : MonoBehaviour
{
    public TMP_Text coinsText;
    int coins;
    SingletonPattern singletonPattern;
    public AudioClip coinAudio;

    private void Start()
    {
        singletonPattern = SingletonPattern.Instance;
        coinsText.text = coins.ToString();
    }
    
    public void SetCoins(int coins)
    {
        this.coins = coins;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            singletonPattern.PlaySoundEffect(coinAudio, 1.0f);
            Destroy(other.gameObject);
            coins += 1;
            coinsText.text = coins.ToString();

            if(coins == 10)
            {
                //Gana una vida
                singletonPattern.GetPlayerController().winLife();
                if (singletonPattern.GetPlayerController().GetHeartActive() == true)
                {
                    //Reinicio de monedas
                    coins -= 10;
                    //Actualizar estado de las corazones
                    singletonPattern.GetPlayerController().SetHeartActive(false);
                }
            }
            singletonPattern.SetCoins(coins);
        }
    }
}
