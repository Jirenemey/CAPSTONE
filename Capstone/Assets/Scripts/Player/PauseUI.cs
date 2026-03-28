using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PauseUI : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject pauseScreen;
    [SerializeField] GameObject settingsScreen;
    [SerializeField] GameObject confirmLeavePrompt;
    [SerializeField] InputActionReference pauseKey;
    [SerializeField] Button resume;
    [SerializeField] Button settings;
    [SerializeField] Button quit;
    [SerializeField] Button quitYes;
    [SerializeField] Button quitNo;
    public bool paused = false;
    [SerializeField] PlayerInput playerInput;
    
    void Start()
    {
        pauseKey.action.Enable();

        if(!pauseMenu) pauseMenu = GameObject.Find("PauseMenu");
        // Pause Screen
        if(!pauseScreen) pauseScreen = GameObject.Find("PauseScreen");
        if(!resume) resume = GameObject.Find("Resume").GetComponent<Button>();
        
        // Settings
        if(!settingsScreen) settingsScreen = GameObject.Find("SettingsScreen");
        if(!settings) settings = GameObject.Find("Settings").GetComponent<Button>();

        // Quit 
        if(!confirmLeavePrompt) confirmLeavePrompt = GameObject.Find("ConfirmLeave");
        if(!quit) quit = GameObject.Find("Quit").GetComponent<Button>();
        if(!quitYes) quitYes = GameObject.Find("Yes").GetComponent<Button>();
        if(!quitNo) quitNo = GameObject.Find("No").GetComponent<Button>();

        resume.onClick.AddListener(() => Resume());
        //settings.onClick.AddListener(() => Resume());
        quit.onClick.AddListener(() => QuitToMenu());
        quitYes.onClick.AddListener(() => ConfirmQuit());
        quitNo.onClick.AddListener(() => Back());

        Resume();
    }

    public void Update()
    {
        Debug.Log(Keyboard.current == null ? "KEYBOARD NULL" : "KEYBOARD OK");

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
        pauseMenu.SetActive(true);
        pauseScreen.SetActive(true);
        settingsScreen.SetActive(false);
        confirmLeavePrompt.SetActive(false);
        paused = true;
        playerInput.enabled = false;
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        pauseScreen.SetActive(false);
        settingsScreen.SetActive(false);
        confirmLeavePrompt.SetActive(false);
        paused = false;
        playerInput.enabled = true;
    }

    public void QuitToMenu()
    {
        pauseScreen.SetActive(false);
        settingsScreen.SetActive(false);
        confirmLeavePrompt.SetActive(true);
    }

    public void ConfirmQuit()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Back()
    {
        pauseScreen.SetActive(true);
        settingsScreen.SetActive(false);
        confirmLeavePrompt.SetActive(false);
    }


}
