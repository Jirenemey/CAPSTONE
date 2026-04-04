using UnityEngine;

public class Spikes : MonoBehaviour
{
    void Start(){
        
    }

    void Update(){
        
    }

	void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.TryGetComponent<Player>(out Player player)) {
            // kill player then respawn them
            player.Respawn();
        }
    }

}
