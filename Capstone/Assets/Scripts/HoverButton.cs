using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    GameObject buttonSides;
    AudioManager audioManager;

    void Start()
    {
        buttonSides = transform.Find("ButtonSides").gameObject;
        buttonSides.SetActive(false);
        
        if(!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        GetComponent<Button>().onClick.AddListener(() => {
            audioManager.PlaySFX("Btn");
            buttonSides.SetActive(false);
        });
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse entered button");
        buttonSides.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse exited button");
        buttonSides.SetActive(false);
    }
}
