using UnityEngine;
using UnityEngine.Events;

public class TriggerProxy : MonoBehaviour {
	// This allows other scripts to subscribe to this trigger
	public UnityAction OnTrigger;
	public string tagName = "Player";

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag(tagName)) {
			OnTrigger?.Invoke();
		}
	}
}