using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class MenuScript : MonoBehaviour
{
    [Header("Buttons")]
	[SerializeField] Button singleplayerBtn;
	[SerializeField] Button multiplayerBtn;
    [SerializeField] Button quitBtn;
    [SerializeField] Button backBtn;
    [Header("Screens")]
    [SerializeField] GameObject menuScrn;
    [SerializeField] GameObject menuTitle;
    [Header("Settings Screen References")]
    [SerializeField] SettingsScript settings;
    [SerializeField] AudioManager audioManager;
    [Header("Animation Related")]
    [SerializeField] GameObject menuBackground;
    int xWidthLimit = Screen.width / 4;
    public float movementAmountX = 10f;
    public float movementAmountY = 2f;

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        float normalizedX = (mousePos.x / Screen.width) * 2f - 1f;
        float normalizedY = (mousePos.y / Screen.height) * 2f - 1f;

        float easedX = Mathf.Sin(normalizedX * Mathf.PI * 0.5f);
        float easedY = Mathf.Sin(normalizedY * Mathf.PI * 0.5f);

        menuBackground.GetComponent<RectTransform>().anchoredPosition = new Vector2(easedX * movementAmountX, easedY * movementAmountY);
    }

    public void Initialize()
    {
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        
        // Menu Items
        if(!singleplayerBtn) singleplayerBtn = GameObject.Find("SingleplayerBtn").GetComponent<Button>();
        if(!multiplayerBtn) multiplayerBtn = GameObject.Find("MultiplayerBtn").GetComponent<Button>();
        if(!quitBtn) quitBtn = GameObject.Find("QuitBtn").GetComponent<Button>();
        if(!backBtn) backBtn = GameObject.Find("BackBtn").GetComponent<Button>();
        if(!menuScrn) menuScrn = GameObject.Find("Menu");
        if(!menuTitle) menuTitle = GameObject.Find("MenuTitle");

        // Settings
        if(!settings) settings = GameObject.Find("SettingsContainer").GetComponent<SettingsScript>();

        // Animations
        if(!menuBackground) menuBackground = GameObject.Find("MenuBackground");

        singleplayerBtn.onClick.AddListener(() => LoadSinglePlayer());
        multiplayerBtn.onClick.AddListener(() => LoadScene("Multiplayer"));
        settings.settingsBtn.onClick.AddListener(() => SettingsPage());
        quitBtn.onClick.AddListener(() => Quit());
        backBtn.onClick.AddListener(() => Back());

        backBtn.gameObject.SetActive(false);
        settings.pressKeyScreen.gameObject.SetActive(false);
        settings.settingsScrn.SetActive(false);
    }

    void Start()
    {
        Initialize();
        audioManager.PlayMusic("Menu");
    }

    // Main Menu Button management
    public void LoadScene(string sceneName) {
		SceneManager.LoadScene(sceneName);
	}

    public void LoadSinglePlayer()
    {
        SceneManager.LoadScene("Arena");
        var networkManager = GameObject.Find("NetworkManager");
        if(networkManager)
            Destroy(networkManager);
    }

    public void SettingsPage()
    {
        menuScrn.SetActive(false);
        settings.settingsScrn.SetActive(true);
        backBtn.gameObject.SetActive(true);
        menuTitle.SetActive(false);
    }

    public void Back()
    {
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        audioManager.PlaySFX("BindingBtn");
        menuScrn.SetActive(true);
        settings.settingsScrn.SetActive(false);
        backBtn.gameObject.SetActive(false);
        menuTitle.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }

}
