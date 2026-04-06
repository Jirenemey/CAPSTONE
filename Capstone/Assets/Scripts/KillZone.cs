using UnityEngine;

public class KillZone : MonoBehaviour
{
    void Start(){
        
    }

    void Update(){
        
    }

	private void OnTriggerEnter2D(Collider2D collision) {
        print(collision.gameObject.name+" is about to die for breaking the laws of the universe");
        collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable);
        if (damageable != null) {
            damageable.TakeDamage(999999f);
        }
	}
}
