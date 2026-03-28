using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Threading.Tasks;

public class MenuScript : MonoBehaviour
{
    [Header("Buttons")]
	[SerializeField] Button singleplayerBtn;
	[SerializeField] Button multiplayerBtn;
	[SerializeField] Button settingsBtn;
    [SerializeField] Button quitBtn;
    [SerializeField] Button backBtn;
    [Header("Screens")]
    [SerializeField] GameObject settingsScrn;
    [SerializeField] GameObject menuScrn;
    [SerializeField] GameObject menuTitle;
    [Header("Settings Screen References")]
    // Bindings
	[SerializeField] InputActionReference jumpAction;
	[SerializeField] InputActionReference dashAction;
	[SerializeField] InputActionReference attackAction;
	[SerializeField] InputActionReference quickCastAction;
    [SerializeField] GameObject[] bindBtns;
    public PlayerInput playerInput;
    InputActionAsset actions;
    InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    [SerializeField] GameObject pressKeyScreen;
    // Volume 
    [SerializeField] AudioManager audioManager;
    [SerializeField] Slider sfxSlider;
    [SerializeField] TMP_Text sfxText;
    [SerializeField] Slider musicSlider;
    [SerializeField] TMP_Text musicText;
    [Header("Animation Related")]
    [SerializeField] GameObject menuBackground;
    int xWidthLimit = Screen.width / 4;
    public float movementAmountX = 10f;
    public float movementAmountY = 2f;

    void Awake() {
        // check if bindings were already set before
        actions = playerInput.actions;

        if (PlayerPrefs.HasKey("rebinds"))
            actions.LoadBindingOverridesFromJson(PlayerPrefs.GetString("rebinds"));
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Debug.Log("Mouse X Pos: " + mousePos.x);

        float normalizedX = (mousePos.x / Screen.width) * 2f - 1f;
        float normalizedY = (mousePos.y / Screen.height) * 2f - 1f;

        float easedX = Mathf.Sin(normalizedX * Mathf.PI * 0.5f);
        float easedY = Mathf.Sin(normalizedY * Mathf.PI * 0.5f);

        menuBackground.GetComponent<RectTransform>().anchoredPosition = new Vector2(easedX * movementAmountX, easedY * movementAmountY);
    }

    void Start()
    {
        // Menu Items
        if(!singleplayerBtn) singleplayerBtn = GameObject.Find("SingleplayerBtn").GetComponent<Button>();
        if(!multiplayerBtn) multiplayerBtn = GameObject.Find("MultiplayerBtn").GetComponent<Button>();
        if(!settingsBtn) settingsBtn = GameObject.Find("SettingsBtn").GetComponent<Button>();
        if(!quitBtn) quitBtn = GameObject.Find("QuitBtn").GetComponent<Button>();
        if(!backBtn) backBtn = GameObject.Find("BackBtn").GetComponent<Button>();
        if(!menuScrn) menuScrn = GameObject.Find("Menu");
        if(!menuTitle) menuTitle = GameObject.Find("MenuTitle");

        // Settings Items
        if(!settingsScrn) settingsScrn = GameObject.Find("Settings");
        // Bindings
        if(!pressKeyScreen) pressKeyScreen = GameObject.Find("PressKeyScreen");
        // Volume
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        if(!sfxSlider) sfxSlider = GameObject.Find("SFXSlider").GetComponent<Slider>();
        if(!sfxText) sfxText = GameObject.Find("SFXPercent").GetComponent<TextMeshProUGUI>();
        if(!musicSlider) musicSlider = GameObject.Find("MusicSlider").GetComponent<Slider>();
        if(!musicText) musicText = GameObject.Find("MusicPercent").GetComponent<TextMeshProUGUI>();
        // Animations
        if(!menuBackground) menuBackground = GameObject.Find("MenuBackground");

        singleplayerBtn.onClick.AddListener(() => LoadScene("SampleScene"));
        multiplayerBtn.onClick.AddListener(() => LoadScene("Multiplayer"));
        settingsBtn.onClick.AddListener(() => SettingsPage());
        quitBtn.onClick.AddListener(() => Quit());
        backBtn.onClick.AddListener(() => Back());

        backBtn.gameObject.SetActive(false);
        pressKeyScreen.gameObject.SetActive(false);
        Back();
        ListBindingText();

        //audioManager.PlayMusic("MenuMusic");
    }
    // Main Menu Button management
    public void LoadScene(string sceneName) {
		SceneManager.LoadScene(sceneName);
	}

    public void SettingsPage()
    {
        menuScrn.SetActive(false);
        settingsScrn.SetActive(true);
        backBtn.gameObject.SetActive(true);
        menuTitle.SetActive(false);
    }

    public void Back()
    {
        audioManager.PlaySFX("BindingBtn");
        menuScrn.SetActive(true);
        settingsScrn.SetActive(false);
        backBtn.gameObject.SetActive(false);
        menuTitle.SetActive(true);
    }

    public void Quit()
    {
        audioManager.PlaySFX("Btn");
        Application.Quit();
    }

    //Settings menu stuff
    // Bindings
    string GetBindingText(InputAction action) {
    if (Gamepad.current != null) 
        return action.GetBindingDisplayString(group: "Gamepad");
    else 
        return action.GetBindingDisplayString(group: "Keyboard&Mouse");
    }

    void ListBindingText()
    {
        bindBtns[0].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GetBindingText(jumpAction);
        bindBtns[1].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GetBindingText(dashAction);
        bindBtns[2].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GetBindingText(attackAction);
        bindBtns[3].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GetBindingText(quickCastAction);
    }

    void SaveBindings()
    {
        PlayerPrefs.SetString("rebinds", actions.SaveBindingOverridesAsJson());
    }

    public void ResetBindings()
    {
        audioManager.PlaySFX("Btn");
        actions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey("rebinds");
        ListBindingText();
    }

    void RebindAction(InputAction action)
    {
        action.Disable(); // stops action from activating since its multiplayer its safe to disable it for any weird interactions
        int bindingIndex = Gamepad.current != null ? 1 : 0;
        Debug.Log("Attempting Rebinding " + action.name);
        pressKeyScreen.gameObject.SetActive(true);

        string oldBinding = action.bindings[bindingIndex].effectivePath;

        rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(op => {
                var newBinding = action.bindings[bindingIndex].effectivePath;

                Debug.Log("New binding: " + newBinding);

                // Block Escape
                if (newBinding == "<Keyboard>/escape")
                {
                    Debug.Log("Escape is not allowed");

                    action.ApplyBindingOverride(bindingIndex, oldBinding);
                    Cleanup(op, action);
                    return;
                }

                // Check duplicates
                foreach (var map in action.actionMap.asset.actionMaps)
                {
                    foreach (var existingAction in map.actions)
                    {
                        if (existingAction == action)
                            continue;

                        foreach (var binding in existingAction.bindings)
                        {
                            if (binding.effectivePath == newBinding)
                            {
                                Debug.Log("Duplicate key detected");

                                // revert
                                action.ApplyBindingOverride(bindingIndex, oldBinding);
                                Cleanup(op, action);
                                return;
                            }
                        }
                    }
                }

                // Valid binding
                Cleanup(op, action);
                SaveBindings();
            })
            .Start();
    }

    void Cleanup(InputActionRebindingExtensions.RebindingOperation op, InputAction action)
    {
        pressKeyScreen.gameObject.SetActive(false);
        action.Enable();
        op.Dispose();
        rebindingOperation = null;
        ListBindingText();
    }

    public void CancelRebinding() {
        rebindingOperation?.Cancel();
    }

    // Binding buttons in UI
    public void RebindAttackAction() {
        audioManager.PlaySFX("BindingBtn");
        RebindAction(attackAction.action);
    }
    public void RebindQuickCastAction() {
        audioManager.PlaySFX("BindingBtn");
        RebindAction(quickCastAction.action);
    }
    public void RebindJumpAction() {
        audioManager.PlaySFX("BindingBtn");
        RebindAction(jumpAction.action);
    }
    public void RebindDashAction() {
        audioManager.PlaySFX("BindingBtn");
        RebindAction(dashAction.action);
    }

    // Volume
    public  void UpdateSFX(){
        audioManager.SFXVolume(sfxSlider.value);
        sfxText.text = (sfxSlider.value * 100).ToString("F0") + "%";
        if (!audioManager.sfxSource.isPlaying)
            audioManager.PlaySFX("Btn");
    }

    public  void UpdateMusic(){
        audioManager.MusicVolume(musicSlider.value);
        musicText.text = (musicSlider.value * 100).ToString("F0") + "%";

        if (!audioManager.sfxSource.isPlaying)
            audioManager.PlaySFX("Btn");
    }

}
