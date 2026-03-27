using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Authentication;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

#if NEW_INPUT_SYSTEM_INSTALLED
using UnityEngine.InputSystem.UI;
#endif

public class NetworkLobyyUI : MonoBehaviour
{

    [SerializeField] GameObject startScreen;
    [SerializeField] GameObject hostScreen;
    [SerializeField] GameObject joinScreen;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TMP_Text codeText;
    [SerializeField] Button m_StartHostButton;
    [SerializeField] Button m_StartClientButton;
    [SerializeField] GameObject backButton;
    [SerializeField] Button menuBackButton;
    [SerializeField] AudioManager audioManager;


    void Start()
    {
        if(!startScreen) startScreen = GameObject.Find("Start Screen");
        if(!hostScreen) hostScreen = GameObject.Find("Host Lobby Screen");
        if(!joinScreen) joinScreen = GameObject.Find("Join Lobby Screen");
        if(!inputField) inputField = GameObject.Find("Input Lobby Code").GetComponent<TMP_InputField>();
        if(!codeText) codeText = GameObject.Find("Lobby Code").GetComponent<TMP_Text>();

        if(!m_StartHostButton) m_StartHostButton = GameObject.Find("StartHostButton").GetComponent<Button>();
        if(!m_StartClientButton) m_StartClientButton = GameObject.Find("StartClientButton").GetComponent<Button>();
        if(!backButton) backButton = GameObject.Find("BackButton");
        if(!menuBackButton) menuBackButton = GameObject.Find("MenuBackButton").GetComponent<Button>();

        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        Back();
        menuBackButton.onClick.AddListener(() => Menu());

        m_StartHostButton.onClick.AddListener(async () => await StartHostWithRelay(1, "UDP"));
        m_StartClientButton.onClick.AddListener(async () => await StartClientWithRelay(inputField.text, "UDP"));
    }

    void Update()
    {
        Debug.Log(inputField.text);
    }

    public void HostScreen()
    {
        hostScreen.SetActive(true);
        startScreen.SetActive(false);
        backButton.SetActive(true);
    }

    public void JoinScreen()
    {
        joinScreen.SetActive(true);
        startScreen.SetActive(false);
        backButton.SetActive(true);
    }

    public void JoinComplete()
    {
        joinScreen.SetActive(false);
        backButton.SetActive(true);
    }

    public void Back()
    {
        audioManager.PlaySFX("Btn");
        startScreen.SetActive(true);
        hostScreen.SetActive(false);
        joinScreen.SetActive(false);
        backButton.SetActive(false);
        LeaveLobby();
    }

    public void Menu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public async Task<string> StartHostWithRelay(int maxConnections, string connectionType) {
        HostScreen();

        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Debug.Log(joinCode);
        codeText.text = "Code: " + joinCode;
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    public void LeaveLobby() {
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.Shutdown();
            if(NetworkManager.Singleton.IsHost) codeText.text = "Code: XXXXXX";
            if(NetworkManager.Singleton.IsClient) Debug.Log("Client disconnected");
        }
    }

    public async Task<bool> StartClientWithRelay(string joinCode, string connectionType) {
        Debug.Log("Attempting to join server.");
        Debug.Log("Attempted code: " + inputField.text);
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        JoinComplete();
        var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
}
