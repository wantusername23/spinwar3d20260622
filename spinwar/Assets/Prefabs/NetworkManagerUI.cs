using Unity.Netcode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    //[SerializeField] private TextMeshProUGUI playersInGameText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        serverBtn.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("Server started...");
            }
            else
            {
                Debug.Log("Server could not be started...");
            }
        });
        hostBtn.onClick.AddListener(() =>
        {
            if(NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started...");
            }
            else
            {
                Debug.Log("Host could not be started...");
            }
        });
        clientBtn.onClick.AddListener(() =>
        {
            if(NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started...");
            }
            else
            {
                Debug.Log("Client could not be started...");
            }
        });
    }
    private void Update()
    {
        //playersInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame}";
    }
    private void Start()
    {

    }
}
