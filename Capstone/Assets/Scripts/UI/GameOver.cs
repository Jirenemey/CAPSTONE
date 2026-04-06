using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class GameOver : NetworkBehaviour
{
    [SerializeField] AudioManager audioManager;
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] Button mainMenuBtn;
    [SerializeField] Button restartBtn;
    [SerializeField] GameObject voteRestartContainer;
    [SerializeField] Button voteRestartBtn;
    [SerializeField] Button cancelRestartBtn;
    [SerializeField] TMP_Text restartText;
    public NetworkVariable<int> restartCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool localReady = false;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        if(!gameOverScreen) gameOverScreen = GameObject.Find("GameOverScreen");
        if(!mainMenuBtn) mainMenuBtn = GameObject.Find("MainMenuBtn").GetComponent<Button>();

        if(!restartBtn) restartBtn = GameObject.Find("RestartBtn").GetComponent<Button>();

        if(!voteRestartContainer) voteRestartContainer = GameObject.Find("VoteRestartContainer");
        if(!voteRestartBtn) voteRestartBtn = GameObject.Find("VoteRestartBtn").GetComponent<Button>();
        if(!cancelRestartBtn) cancelRestartBtn = GameObject.Find("CancelVoteBtn").GetComponent<Button>();
        if(!restartText) restartText = GameObject.Find("VoteRestartTxt").GetComponent<TMP_Text>();

        mainMenuBtn.onClick.AddListener(() => MainMenu());
        restartBtn.onClick.AddListener(() => Restart());

        if (NetworkManager.Singleton)
        {
            OnNetworkSpawn();
        } 
        else {
            restartBtn.interactable = true;
            restartBtn.onClick.AddListener(() => Restart());
        }

        gameOverScreen.SetActive(false);

    }

    public void SetGameOver()
    {
        gameOverScreen.SetActive(true);

        mainMenuBtn.gameObject.SetActive(true);
        if (NetworkManager.Singleton)
        {
            voteRestartContainer.SetActive(true);
            restartBtn.gameObject.SetActive(false);
            voteRestartBtn.gameObject.SetActive(true);
            cancelRestartBtn.gameObject.SetActive(false);
            restartText.gameObject.SetActive(true);
        } else
        {
            voteRestartContainer.SetActive(false);
            restartBtn.gameObject.SetActive(true);
        }
    }

    void MainMenu()
    {
        if(NetworkManager.Singleton){
            NetworkManager.Singleton.Shutdown();
            Destroy(GameObject.Find("NetworkManager"));
        }
        SceneManager.LoadScene("MainMenu");
    }
    
    void Restart(){
        if(!NetworkManager.Singleton){
            SceneManager.LoadScene("Arena");
        } 
        else {
            TryRestartServerRpc();
        }
    }

    [ServerRpc]
    void TryRestartServerRpc()
    {
        var enemies = FindObjectsOfType<EnemyTag>();
        foreach(var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
        EnablePlayersClientRpc();
        NetworkManager.Singleton.SceneManager.LoadScene(
            "Arena",
            LoadSceneMode.Single
        );
    }

    [ClientRpc]
    void EnablePlayersClientRpc()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null) continue;

            var player = client.PlayerObject;
            var stats = player.GetComponent<PlayerStats>();            
            
            stats.SetPlayerStats();
            stats.isDead.Value = false;
            stats.Heal(stats.GetMaxHp());
            
            player.GetComponent<PlayerInput>().enabled = true;
            player.GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        restartBtn.interactable = true;
        voteRestartBtn.interactable = true;
        cancelRestartBtn.interactable = true;

        restartBtn.onClick.AddListener(() => Restart());
        voteRestartBtn.onClick.AddListener(() => VoteRestart());
        cancelRestartBtn.onClick.AddListener(() => CancelVote());

        restartText.text = "0/2";

        restartCount.OnValueChanged += (prev, curr) =>
        {
            restartText.text = curr + "/2";

            if(restartCount.Value == 2 && NetworkManager.Singleton.IsHost)
                restartBtn.gameObject.SetActive(true);
            else
                restartBtn.gameObject.SetActive(false);

        };
    }

    public void VoteRestart()
    {
        voteRestartBtn.interactable = false;
        voteRestartBtn.gameObject.SetActive(false);
        cancelRestartBtn.interactable = true;
        cancelRestartBtn.gameObject.SetActive(true);
        if (localReady) return;
        localReady = true;

        if (NetworkManager.Singleton.IsClient)
            VoteRestartServerRpc(1);
        else if (NetworkManager.Singleton.IsHost)
            restartCount.Value += 1;
    }

    public void CancelVote()
    {
        cancelRestartBtn.interactable = false;
        cancelRestartBtn.gameObject.SetActive(false);
        voteRestartBtn.interactable = true;
        voteRestartBtn.gameObject.SetActive(true);
        if (!localReady) return;
        localReady = false;

        if (NetworkManager.Singleton.IsClient)
            VoteRestartServerRpc(-1);
        else if (NetworkManager.Singleton.IsHost)
            restartCount.Value += -1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void VoteRestartServerRpc(int value)
    {
        restartCount.Value += value; 
    } 


}
