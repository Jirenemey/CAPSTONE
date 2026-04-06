
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
    [SerializeField] Transform arenaEntrance;
    [SerializeField] PauseUI pauseUI;
	GameObject doorTrigger;
    GameObject door;
    int numberOfPlayersinArena = 0;
    int numberOfPlayerInLobby = 1;

    public WaveManager waveManager;
    public GameObject otherPlayer;
    public GameObject ownerPlayer;

	void Start(){
        if(!pauseUI) pauseUI = GameObject.Find("PauseContainer").GetComponent<PauseUI>();
        if (NetworkManager.Singleton)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if(player.GetComponent<Player>().IsOwner){
                    ownerPlayer = player.gameObject;
                    SetCameraTarget(player.transform);
                    player.transform.position = spawnPoint.transform.position;
                    player.GetComponent<Player>().playerStats.SetPlayerStats();
                    pauseUI.playerInput = player.GetComponent<PlayerInput>();
                } else
                {
                    otherPlayer = player.gameObject;
                }
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
    }

    public void SetCameraTarget(Transform target)
    {
        camera.Target.TrackingTarget = target;
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
            otherPlayer.transform.position = arenaEntrance.position;
            ownerPlayer.transform.position = arenaEntrance.position;
            waveManager.StartNextWaveServerRpc();

        }
        else {
            waveManager.StartNextWave();
        }
    }
}
