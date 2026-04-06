using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class SettingsScript : MonoBehaviour
{
    [Header("Settings Screen References")]
    [SerializeField] public Button settingsBtn;
    [SerializeField] public GameObject settingsScrn;
    // Bindings
	[SerializeField] InputActionReference jumpAction;
	[SerializeField] InputActionReference dashAction;
	[SerializeField] InputActionReference attackAction;
	[SerializeField] InputActionReference quickCastAction;
    [SerializeField] GameObject[] bindBtns;
    [SerializeField] Button resetBindings;
    public PlayerInput playerInput;
    InputActionAsset actions;
    InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    [SerializeField] public GameObject pressKeyScreen;
    // Volume 
    [SerializeField] public AudioManager audioManager;
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
        Initialize();
    }

    public void Initialize()
    {
        if(!settingsBtn) settingsBtn = GameObject.Find("SettingsBtn").GetComponent<Button>();
        // Settings Items
        if(!settingsScrn) settingsScrn = GameObject.Find("Settings");
        // Bindings
        if(!pressKeyScreen) pressKeyScreen = GameObject.Find("PressKeyScreen");
        if(!resetBindings) resetBindings = GameObject.Find("ResetBindings").GetComponent<Button>();

        resetBindings.onClick.AddListener(() => ResetBindings());
        bindBtns[0].GetComponent<Button>().onClick.AddListener(() => RebindJumpAction());
        bindBtns[1].GetComponent<Button>().onClick.AddListener(() => RebindDashAction());
        bindBtns[2].GetComponent<Button>().onClick.AddListener(() => RebindAttackAction());
        bindBtns[3].GetComponent<Button>().onClick.AddListener(() => RebindQuickCastAction());
        
        ListBindingText();

        // Volume
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        
        if(!sfxSlider) sfxSlider = GameObject.Find("SFXSlider").GetComponent<Slider>();
        if(!sfxText) sfxText = GameObject.Find("SFXPercent").GetComponent<TextMeshProUGUI>();

        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        UpdateSFX();
        
        if(!musicSlider) musicSlider = GameObject.Find("MusicSlider").GetComponent<Slider>();
        if(!musicText) musicText = GameObject.Find("MusicPercent").GetComponent<TextMeshProUGUI>();

        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        UpdateMusic();

        sfxSlider.onValueChanged.AddListener(delegate { UpdateSFX(); });
        musicSlider.onValueChanged.AddListener(delegate { UpdateMusic(); });

        pressKeyScreen.SetActive(false);
    }

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    // static void OnAfterSceneLoad()
    // {
    //     SettingsScript settings = FindObjectOfType<SettingsScript>();
    //     if (settings != null)
    //     {
    //         settings.Initialize();
    //     }
    // }

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
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
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
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        audioManager.PlaySFX("BindingBtn");
        RebindAction(attackAction.action);
    }
    public void RebindQuickCastAction() {
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        audioManager.PlaySFX("BindingBtn");
        RebindAction(quickCastAction.action);
    }
    public void RebindJumpAction() {
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        audioManager.PlaySFX("BindingBtn");
        RebindAction(jumpAction.action);
    }
    public void RebindDashAction() {
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        audioManager.PlaySFX("BindingBtn");
        RebindAction(dashAction.action);
    }

    // Volume
    public void UpdateSFX(){
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        audioManager.SFXVolume(sfxSlider.value);
        sfxText.text = (sfxSlider.value * 100).ToString("F0") + "%";
        
        if (!audioManager.sfxSource.isPlaying)
            audioManager.PlaySFX("Btn");
    }

    public void UpdateMusic(){
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        audioManager.MusicVolume(musicSlider.value);
        musicText.text = (musicSlider.value * 100).ToString("F0") + "%";

        if (!audioManager.sfxSource.isPlaying)
            audioManager.PlaySFX("Btn");
    }
}


