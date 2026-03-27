using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    GameObject buttonSides;
    AudioManager audioManager;

    void Start()
    {
        if(transform.Find("ButtonSides").gameObject) buttonSides = transform.Find("ButtonSides").gameObject;
        if(buttonSides) buttonSides.SetActive(false);
        
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        GetComponent<Button>().onClick.AddListener(() => {
            audioManager.PlaySFX("Btn");
            if(buttonSides) buttonSides.SetActive(false);
        });
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse entered button");
        audioManager.PlaySFX("BtnHover");
        if(buttonSides) buttonSides.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse exited button");
        if(buttonSides) buttonSides.SetActive(false);
    }
}
