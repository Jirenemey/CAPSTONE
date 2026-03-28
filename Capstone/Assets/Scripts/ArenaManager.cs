using Unity.Netcode;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    [SerializeField] AudioManager audioManager;
	GameObject doorTrigger;
    GameObject door;
    int numberOfPlayersinArena = 0;
    int numberOfPlayerInLobby = 1;

	void Start(){
		if (!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

		doorTrigger = GameObject.Find("DoorTrigger");
        var triggerProxy = doorTrigger.GetComponent<TriggerProxy>();//.OnTrigger += CloseDoor;
        triggerProxy.OnTrigger += CloseDoor;
        triggerProxy.tagName = "Player";

        door = GameObject.Find("White_Gate");
        
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
			audioManager.PlaySFX("GateClose");

			doorTrigger.GetComponent<BoxCollider2D>().enabled = false;
		}

	}
}
