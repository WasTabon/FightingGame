using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

public class BattlePassIAPController : MonoBehaviour
{
    public string _battlePassId = "com.battlepass.inapp";
    
    public GameObject loadingButton;
    public AudioClip buySound;
    public TextMeshProUGUI buttonText;
    public GameObject panel;
    
    public void OnPurchaseComplete(Product product)
    {
        if (product.definition.id == _battlePassId)
        {
            Debug.Log("Battle Pass Purchase Complete");
            BattlePassController.Instance.SetPremium(true);
            
            MusicController.Instance.PlaySpecificSound(buySound);
            loadingButton.SetActive(false);
            buttonText.transform.parent.gameObject.SetActive(false);
            panel.SetActive(true);
        }
    }
    
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
    {
        if (product.definition.id == _battlePassId)
        {
            loadingButton.SetActive(false);
            Debug.Log($"Battle Pass Purchase Failed: {description.message}");
        }
    }
    
    public void OnProductFetched(Product product)
    {
        Debug.Log("Battle Pass Product Fetched");
        buttonText.text = product.metadata.localizedPriceString;
    }
}