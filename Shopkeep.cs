using DuloGames.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Shopkeep : MonoBehaviour
{
    public List<UIItemInfo> shopkeepInventory;
    public int shopkeepGoldAmount;
    public Character character;
    public float merchantMarkup = 1.2f;

    void OpenShop()
    {
        Shop.instance.currentShopkeep = this;
        Shop.instance.ToggleShop();
    }

    public void AddItem(UIItemInfo itemInfo)
    {
        shopkeepInventory.Add(itemInfo);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            DialogueManager.instance.OnChoiceMadeToActivateFunction.AddListener(OpenShop);
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
            DialogueManager.instance.OnChoiceMadeToActivateFunction.RemoveListener(OpenShop);
    }
}
