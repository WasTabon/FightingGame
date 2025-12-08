using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button))]
public class CardButton : MonoBehaviour
{
    [SerializeField] private int cardIndex;
    
    private Button button;
    private Action<int> onCardSelected;
    
    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        Debug.Log($"[CardButton] Awake: {gameObject.name}, button found: {button != null}");
    }
    
    public void Initialize(int index, Action<int> callback)
    {
        cardIndex = index;
        onCardSelected = callback;
        Debug.Log($"[CardButton] Initialize: {gameObject.name}, index: {index}, callback null: {callback == null}");
    }
    
    public void SetInteractable(bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
        Debug.Log($"[CardButton] SetInteractable: {gameObject.name}, interactable: {interactable}");
    }
    
    void OnClick()
    {
        Debug.Log($"[CardButton] OnClick: {gameObject.name}, index: {cardIndex}, callback null: {onCardSelected == null}");
        onCardSelected?.Invoke(cardIndex);
    }
    
    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick);
    }
}