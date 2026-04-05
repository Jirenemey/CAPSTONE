using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;


public class ArenaManager : MonoBehaviour
{
    [SerializeField] AudioManager audioManager;
    [SerializeField] CinemachineCamera camera;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] PauseUI pauseUI;
	GameObject doorTrigger;
    GameObject door;
    int numberOfPlayersinArena = 0;
    int numberOfPlayerInLobby = 1;

    WaveManager waveManager;
    bool init = false;

	void Start(){
        if(!pauseUI) pauseUI = GameObject.Find("PauseContainer").GetComponent<PauseUI>();
        if (NetworkManager.Singleton)
        {
            SetCameraToOwner(false);
        } else
        {
            var player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
            camera.Target.TrackingTarget = player.transform;
            pauseUI.playerInput = player.GetComponent<PlayerInput>();
        }

		doorTrigger = GameObject.Find("DoorTrigger");
        var triggerProxy = doorTrigger.GetComponent<TriggerProxy>();//.OnTrigger += CloseDoor;
        triggerProxy.OnTrigger += CloseDoor;
        triggerProxy.tagName = "Player";

        door = GameObject.Find("White_Gate");
        waveManager = GetComponent<WaveManager>();
        if (!waveManager) Assert.Fail("WaveManager cannot be accessed by the ArenaManager");
        
        init = true;
    }

    public void SetCameraToOwner(bool spectate)
    {
        GameObject otherPlayer;
        if (NetworkManager.Singleton)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject player in players)
            {
                if(!player.GetComponent<Player>().IsOwner) otherPlayer = player;
                if(player.GetComponent<Player>().IsOwner){
                    camera.Target.TrackingTarget = player.transform;
                    if (spectate)
                    {
                        if(player.GetComponent<Player>().playerStats.GetCurrentHp() <= 0)
                        {
                            camera.Target.TrackingTarget = otherPlayer.transform;
                        }
                    }
                    
                    if(!init){
                        player.transform.position = spawnPoint.position;
                        player.GetComponent<Player>().playerStats.SetPlayerStats();
                        pauseUI.playerInput = player.GetComponent<PlayerInput>();

                    }
                }
            }
        }
    }

    void Update(){
        
    }

    void CloseDoor() {
        numberOfPlayersinArena++;
        numberOfPlayerInLobby = NetworkManager.Singleton == null ? 1 : 2;
		if (numberOfPlayersinArena >= numberOfPlayerInLobby) {
            // close door
            door.GetComponent<Animator>().SetTrigger("CloseNow");
            door.GetComponent<BoxCollider2D>().enabled = true;
            if (!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
			audioManager.PlaySFX("GateClose");

			doorTrigger.GetComponent<BoxCollider2D>().enabled = false;
            StartArena();
		}
	}

    void StartArena() {
        if(NetworkManager.Singleton){
            if(NetworkManager.Singleton.IsHost) waveManager.StartNextWaveServerRpc();
            if(NetworkManager.Singleton.IsClient) waveManager.DisplayWaveObjectsClientRpc(waveManager.waves[waveManager.currentWaveIndex]);
        }
        else {
            waveManager.StartNextWave();
        }
    }
}
