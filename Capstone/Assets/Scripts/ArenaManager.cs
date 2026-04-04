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

	void Start(){
        if(!pauseUI) pauseUI = GameObject.Find("PauseContainer").GetComponent<PauseUI>();
        if (NetworkManager.Singleton)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject player in players)
            {
                if (player.GetComponent<Player>().IsOwner)
                {
                    camera.Target.TrackingTarget = player.transform;
                     pauseUI.playerInput = player.GetComponent<PlayerInput>();
                }
                player.transform.position = spawnPoint.position;
                player.GetComponent<Player>().playerStats.SetPlayerStats();
            }
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
        if(NetworkManager.Singleton)
            waveManager.StartNextWaveServerRpc();
        else
            waveManager.StartNextWave();
    }
}
