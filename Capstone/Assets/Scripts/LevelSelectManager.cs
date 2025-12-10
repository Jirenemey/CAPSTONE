using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour {
	[Header("Level Buttons")]
	public Button level1Button;
	public Button level2Button;
	public Button level3Button;

	void Start() {
		// Optional: Add click listeners programmatically
		level1Button.onClick.AddListener(() => LoadLevel("Chase and Evade"));
		level2Button.onClick.AddListener(() => LoadLevel("BehaviourTreeExample"));
		level3Button.onClick.AddListener(() => LoadLevel("Josh's Level"));
	}

	public void LoadLevel(string sceneName) {
		SceneManager.LoadScene(sceneName);
	}
}
