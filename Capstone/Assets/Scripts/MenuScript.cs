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
	[SerializeField] Button settingsBtn;
    [SerializeField] Button backBtn;
    [Header("Screens")]
    [SerializeField] GameObject settingsScrn;
    [SerializeField] GameObject menuScrn;
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

    void Awake() {
        // check if bindings were already set before
        actions = playerInput.actions;

        if (PlayerPrefs.HasKey("rebinds"))
            actions.LoadBindingOverridesFromJson(PlayerPrefs.GetString("rebinds"));
    }

    void Start()
    {
        // Menu Items
        if(!singleplayerBtn) singleplayerBtn = GameObject.Find("SingleplayerBtn").GetComponent<Button>();
        if(!multiplayerBtn) multiplayerBtn = GameObject.Find("MultiplayerBtn").GetComponent<Button>();
        if(!settingsBtn) settingsBtn = GameObject.Find("SettingsBtn").GetComponent<Button>();
        if(!backBtn) backBtn = GameObject.Find("BackBtn").GetComponent<Button>();
        if(!menuScrn) menuScrn = GameObject.Find("Menu");

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
        

        singleplayerBtn.onClick.AddListener(() => LoadScene("SampleScene"));
        multiplayerBtn.onClick.AddListener(() => LoadScene("Multiplayer"));
        settingsBtn.onClick.AddListener(() => SettingsPage());
        backBtn.onClick.AddListener(() => Back());

        backBtn.gameObject.SetActive(false);
        pressKeyScreen.gameObject.SetActive(false);
        Back();
        ListBindingText();
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
    }

    public void Back()
    {
        menuScrn.SetActive(true);
        settingsScrn.SetActive(false);
        backBtn.gameObject.SetActive(false);
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
        RebindAction(attackAction.action);
    }
    public void RebindQuickCastAction() {
        RebindAction(quickCastAction.action);
    }
    public void RebindJumpAction() {
        RebindAction(jumpAction.action);
    }
    public void RebindDashAction() {
        RebindAction(dashAction.action);
    }

    // Volume
    public void UpdateSFX(){
        audioManager.SFXVolume(sfxSlider.value);
        sfxText.text = (sfxSlider.value * 100).ToString("F0") + "%";
    }

    public void UpdateMusic(){
        audioManager.MusicVolume(musicSlider.value);
        musicText.text = (musicSlider.value * 100).ToString("F0") + "%";
    }

}
