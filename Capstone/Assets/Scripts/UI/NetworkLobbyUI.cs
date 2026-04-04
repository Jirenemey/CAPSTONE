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

public class NetworkLobyyUI : NetworkBehaviour
{

    [SerializeField] GameObject startScreen;
    [SerializeField] GameObject hostScreen;
    [SerializeField] GameObject joinScreen;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TMP_Text codeText;
    [SerializeField] GameObject invalidCodeText;
    [SerializeField] Button m_StartHostButton;
    [SerializeField] Button m_StartClientButton;
    [SerializeField] GameObject backButton;
    [SerializeField] Button menuBackButton;
    [SerializeField] AudioManager audioManager;
    [SerializeField] GameObject readySystem;
    [SerializeField] Button readyUp;
    [SerializeField] Button unReady;
    [SerializeField] TMP_Text readyText;
    [SerializeField] Button startBtn;
    public NetworkVariable<int> readyCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int playerCount = 2;
    private bool localReady = false;


    void Initialize() {
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        if(!startScreen) startScreen = GameObject.Find("Start Screen");
        if(!hostScreen) hostScreen = GameObject.Find("Host Lobby Screen");
        if(!joinScreen) joinScreen = GameObject.Find("Join Lobby Screen");
        if(!inputField) inputField = GameObject.Find("Input Lobby Code").GetComponent<TMP_InputField>();
        if(!codeText) codeText = GameObject.Find("Lobby Code").GetComponent<TMP_Text>();
        if(!invalidCodeText) invalidCodeText = GameObject.Find("InvalidCode");

        if(!m_StartHostButton) m_StartHostButton = GameObject.Find("StartHostButton").GetComponent<Button>();
        if(!m_StartClientButton) m_StartClientButton = GameObject.Find("StartClientButton").GetComponent<Button>();
        if(!backButton) backButton = GameObject.Find("BackButton");
        if(!menuBackButton) menuBackButton = GameObject.Find("MenuBackButton").GetComponent<Button>();

        if(!readySystem) readySystem = GameObject.Find("ReadySystem");
        if(!readyUp) readyUp = GameObject.Find("ReadyBtn").GetComponent<Button>();
        if(!unReady) unReady = GameObject.Find("UnReadyBtn").GetComponent<Button>();
        if(!readyText) readyText = GameObject.Find("ReadyText").GetComponent<TMP_Text>();
        if(!startBtn) startBtn = GameObject.Find("StartBtn").GetComponent<Button>();

        m_StartHostButton.onClick.AddListener(async () => await StartHostWithRelay(playerCount - 1, "UDP"));
        m_StartClientButton.onClick.AddListener(async () => await StartClientWithRelay(inputField.text, "UDP"));

        backButton.GetComponent<Button>().onClick.AddListener(() => Back());

        hostScreen.SetActive(false);
        joinScreen.SetActive(false);
        backButton.SetActive(false);
        invalidCodeText.SetActive(false);
        unReady.gameObject.SetActive(false);
        readySystem.SetActive(false);

        menuBackButton.onClick.AddListener(() => Menu());
    }

    void Start()
    {
        Initialize();
        audioManager.PlayMusic("Multiplayer");
    }

    void Update() {
        Debug.Log(inputField.text);
    }

    public void HostScreen() {
        hostScreen.SetActive(true);
        startScreen.SetActive(false);
        backButton.SetActive(true);
    }

    public void JoinScreen() {
        joinScreen.SetActive(true);
        startScreen.SetActive(false);
        backButton.SetActive(true);
        invalidCodeText.SetActive(false);
    }

    public void JoinComplete() {
        joinScreen.SetActive(false);
        backButton.SetActive(true);
        invalidCodeText.SetActive(false);
    }

    public void JoinFailed() {
        Debug.Log("Failed to connect to server!");
        invalidCodeText.SetActive(true);
    }

    public void Back() {
        audioManager.PlaySFX("Btn");
        startScreen.SetActive(true);
        hostScreen.SetActive(false);
        joinScreen.SetActive(false);
        backButton.SetActive(false);
        invalidCodeText.SetActive(false);
        unReady.gameObject.SetActive(false);
        readySystem.SetActive(false);
        LeaveLobby();
    }

    public void Menu() {
        audioManager.PlayMusic("Menu");
        SceneManager.LoadScene("MainMenu");
    }

    public override void OnNetworkSpawn() {

        base.OnNetworkSpawn();

        readyUp.interactable = true;
        unReady.interactable = true;
        startBtn.interactable = true;

        readyUp.onClick.AddListener(() => ReadyUp());
        unReady.onClick.AddListener(() => UnReady());
        startBtn.onClick.AddListener(() => StartGame());

        startBtn.gameObject.SetActive(false);

        readyText.text = "0/2";

        readyCount.OnValueChanged += (prev, curr) =>
        {
            readyText.text = curr + "/2";

            if(readyCount.Value == playerCount && NetworkManager.Singleton.IsHost)
                startBtn.gameObject.SetActive(true);
            else
                startBtn.gameObject.SetActive(false);

        };
    }

    public void ReadyUp() {
        audioManager.PlaySFX("Btn");
        readyUp.gameObject.SetActive(false);
        unReady.gameObject.SetActive(true);
        if (localReady) return;
        localReady = true;

        if (NetworkManager.Singleton.IsClient)
            UpdateReadyStatusServerRpc(1);
        else if (NetworkManager.Singleton.IsHost)
            readyCount.Value += 1;
    }

    public void UnReady() {
        audioManager.PlaySFX("BindingBtn");
        unReady.gameObject.SetActive(false);
        readyUp.gameObject.SetActive(true);
        if (!localReady) return;
        localReady = false;

        if (NetworkManager.Singleton.IsClient)
            UpdateReadyStatusServerRpc(-1);
        else if (NetworkManager.Singleton.IsHost)
            readyCount.Value += -1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateReadyStatusServerRpc(int value)
    {
        readyCount.Value += value; 
    }

    public void StartGame() {
        audioManager.PlaySFX("Btn");
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Only the host can start the game!");
            return;
        }
        NetworkManager.Singleton.SceneManager.LoadScene(
            "Arena",
            LoadSceneMode.Single
        );
        
    }

    public void LeaveLobby() {
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.Shutdown();
            if(NetworkManager.Singleton.IsHost) codeText.text = "Code: XXXXXX";
            if(NetworkManager.Singleton.IsClient) Debug.Log("Client disconnected");
        }
    }


    public async Task<bool> StartHostWithRelay(int maxConnections, string connectionType) {
        try {
        m_StartHostButton.interactable = false;
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

        bool started = NetworkManager.Singleton.StartHost();

        if (started) {
            Debug.Log("Host started successfully!");
            OnHostCreated();
        }
        else {
            m_StartHostButton.interactable = true;
            Debug.LogError("Host failed to start.");
        }

        return started;
        }
        catch (RelayServiceException ex) {
            Debug.LogError("Failed to create relay. " + ex.Message);
            return false;
        }
        catch (AuthenticationException ex) {
            Debug.LogError("Authentication failed: " + ex.Message);
            return false;
        }
        catch (System.Exception ex) {
            Debug.LogError("Unexpected error creating server: " + ex.Message);
            return false;
        }
    }

    private void OnHostCreated() {
        Debug.Log("Successfully created server!");
        HostScreen();
        m_StartHostButton.interactable = true;
        readySystem.SetActive(true);
        OnNetworkSpawn();

    }

    public async Task<bool> StartClientWithRelay(string joinCode, string connectionType) {
        try {
        m_StartClientButton.interactable = false;
        if (string.IsNullOrEmpty(joinCode)) {
            Debug.LogError("Join code is empty!");
            return false;
        }

        Debug.Log("Attempting to join server.");
        Debug.Log("Attempted code: " + inputField.text);

        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        return NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException ex) {
            Debug.LogError("Failed to join relay. The join code may be incorrect: " + ex.Message);
            JoinFailed();
            m_StartClientButton.interactable = true;
            return false;
        }
        catch (AuthenticationException ex) {
            Debug.LogError("Authentication failed: " + ex.Message);
            m_StartClientButton.interactable = true;
            return false;
        }
        catch (System.Exception ex) {
            Debug.LogError("Unexpected error joining server: " + ex.Message);
            m_StartClientButton.interactable = true;
            return false;
        }
    }

    private void OnClientConnected(ulong clientId) {
        Debug.Log("Successfully connected to server! Client ID: " + clientId);
        JoinComplete();
        m_StartClientButton.interactable = true;
        readySystem.SetActive(true);

    }
}
