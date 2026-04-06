using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PauseUI : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject pauseScreen;
    [SerializeField] SettingsScript settings;
    [SerializeField] GameObject confirmLeavePrompt;
    [SerializeField] InputActionReference pauseKey;
    [SerializeField] Button back;
    [SerializeField] Button resume;
    [SerializeField] Button quit;
    [SerializeField] Button quitYes;
    [SerializeField] Button quitNo;
    public bool paused = false;
    [SerializeField] public PlayerInput playerInput;
    
    void Start()
    {
        pauseKey.action.Enable();

        if(!pauseMenu) pauseMenu = GameObject.Find("PauseMenu");
        // Pause Screen
        if(!pauseScreen) pauseScreen = GameObject.Find("PauseScreen");
        if(!resume) resume = GameObject.Find("Resume").GetComponent<Button>();
        
        // Settings
        if(!settings) settings = GetComponent<SettingsScript>();
        if(!back) back = GameObject.Find("Back").GetComponent<Button>();

        // Quit 
        if(!confirmLeavePrompt) confirmLeavePrompt = GameObject.Find("ConfirmLeave");
        if(!quit) quit = GameObject.Find("Quit").GetComponent<Button>();
        if(!quitYes) quitYes = GameObject.Find("Yes").GetComponent<Button>();
        if(!quitNo) quitNo = GameObject.Find("No").GetComponent<Button>();

        resume.onClick.AddListener(() => Resume());
        settings.settingsBtn.onClick.AddListener(() => Settings());
        back.onClick.AddListener(() => Back());
        quit.onClick.AddListener(() => QuitToMenu());
        quitYes.onClick.AddListener(() => ConfirmQuit());
        quitNo.onClick.AddListener(() => Back());

        pauseMenu.SetActive(false);
        pauseScreen.SetActive(false);
        settings.settingsScrn.SetActive(false);
        confirmLeavePrompt.SetActive(false);
        paused = false;
        
    }

    public void Update()
    {
        if (pauseKey.action.WasPressedThisFrame())
        {
            Debug.Log("PAUSE KEY PRESSED");
            if(!paused)
                Pause();
            else
                Resume();
        }
    }

    public void Pause()
    {
        if(!NetworkManager.Singleton) Time.timeScale = 0.0f;
        pauseMenu.SetActive(true);
        pauseScreen.SetActive(true);
        settings.settingsScrn.SetActive(false);
        confirmLeavePrompt.SetActive(false);
        paused = true;
        playerInput.enabled = false;
    }

    public void Resume()
    {
        if(!NetworkManager.Singleton) Time.timeScale = 1.0f;
        pauseMenu.SetActive(false);
        pauseScreen.SetActive(false);
        settings.settingsScrn.SetActive(false);
        confirmLeavePrompt.SetActive(false);
        paused = false;
        if(playerInput.gameObject.GetComponent<PlayerStats>().isDead.Value == false)
            playerInput.enabled = true;
    }

    public void Settings()
    {
        settings.settingsScrn.SetActive(true);
        pauseScreen.SetActive(false);
        confirmLeavePrompt.SetActive(false);
    }

    public void QuitToMenu()
    {
        pauseScreen.SetActive(false);
        settings.settingsScrn.SetActive(false);
        confirmLeavePrompt.SetActive(true);
    }

    public void ConfirmQuit()
    {
        playerInput.enabled = true;
        if(NetworkManager.Singleton) {
            NetworkManager.Singleton.SceneManager.LoadScene(
            "MainMenu",
            LoadSceneMode.Single
        );
            NetworkManager.Singleton.Shutdown();
            Destroy(GameObject.Find("NetworkManager"));

        } else {
        SceneManager.LoadScene("MainMenu");
        }

        SceneManager.UnloadSceneAsync("Arena");
    }

    public void Back()
    {
        settings.audioManager.PlaySFX("BindingBtn");
        pauseScreen.SetActive(true);
        settings.settingsScrn.SetActive(false);
        confirmLeavePrompt.SetActive(false);
    }


}
