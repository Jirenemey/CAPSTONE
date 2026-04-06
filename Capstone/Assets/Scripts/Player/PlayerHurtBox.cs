using UnityEngine;

public class PlayerHurtBox : MonoBehaviour
{
	Player player;
    void Start(){
        if(player == null) {
			player = transform.parent.GetComponent<Player>();
		}
    }
	private void OnTriggerEnter2D(Collider2D other) {
		var enemyColl = other.gameObject.TryGetComponent<EnemyTag>(out EnemyTag component);
		if (enemyColl) {
			player.TakeDamage(1);
		}
	}
}
