using DuloGames.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public static Shop instance;
    public GameObject shopUI;
    public Shopkeep currentShopkeep;
    public Text playerName, vendorName;

    //needed for the 2 main trade windows 
    public GameObject vendorItemSlotParent, playerItemSlotParent;
    public GameObject vendorSlotTemplate, playerSlotTemplate;
    public Text vendorMoneyText, playerMoneyText;
    public ScrollRect vendorInvScrollRect, playerInvScrollRect;

    [SerializeField] List<UIItemSlot> vendorSlotsList = new List<UIItemSlot>();
    [SerializeField] List<UIItemSlot> playerSlotsList = new List<UIItemSlot>();


    //needed for the center trading panel of the screen
    public GameObject vendorTradeSlotParent, playerTradeSlotParent;
    public GameObject vendorTradeSlotTemplate, playerTradeSlotTemplate;
    public ScrollRect playerTradeScrollRect, vendorTradeScrollRect;

    [SerializeField] List<UIItemSlot> vendorTradeSlotsList = new List<UIItemSlot>();
    [SerializeField] List<UIItemSlot> playerTradeSlotsList = new List<UIItemSlot>();

    public Text vendorMoneyTradeText, playerMoneyTradeText;
    public Button acceptButton;

    int goldSpent, goldReceived;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        vendorSlotsList.AddRange(vendorItemSlotParent.GetComponentsInChildren<UIItemSlot>());
        playerSlotsList.AddRange(playerItemSlotParent.GetComponentsInChildren<UIItemSlot>());
    }

    public void ToggleShop()
    {
        if(!shopUI.activeInHierarchy && !GameManager.instance.UIOpen)
        {
            SetNameTexts();
            SetMoneyTexts();

            RefreshSlotsAndItems();

            GameManager.instance.UIOpen = true;
            GameManager.instance.LockMovementUnlockCursor();
            shopUI.SetActive(true);

            playerInvScrollRect.verticalNormalizedPosition = 1;
            vendorInvScrollRect.verticalNormalizedPosition = 1;
        }
        else
        {
            ClearAllShopSlots();

            GameManager.instance.UIOpen = false;
            GameManager.instance.UnlockMovementLockCursor();
            shopUI.SetActive(false);
        }
    }

    void RefreshSlotsAndItems()
    {
        UpdateItems(currentShopkeep.shopkeepInventory, vendorSlotsList, vendorSlotTemplate);
        UpdateItems(InventoryManager.instance.allInventorySlotInfo, playerSlotsList, playerSlotTemplate);
        IncreaseSlots(playerTradeSlotTemplate, playerTradeSlotsList);
        IncreaseSlots(vendorTradeSlotTemplate, vendorTradeSlotsList);
    }

    void ClearAllShopSlots()
    {
        ClearShopSlots(playerSlotsList);
        ClearShopSlots(vendorSlotsList);
        ClearShopSlots(playerTradeSlotsList);
        ClearShopSlots(vendorTradeSlotsList);
    }

    void SetNameTexts()
    {
        playerName.text = PlayerStats.instance.playerName;
        vendorName.text = currentShopkeep.character.characterName;
    }

    void SetMoneyTexts()
    {
        playerMoneyText.text = InventoryManager.instance.currentGoldAmount.ToString();
        vendorMoneyText.text = currentShopkeep.shopkeepGoldAmount.ToString();
    }

    void AddVendorSlotCushion()
    {
        int amount;

        if (vendorSlotsList.Count % 2 == 0) //make sure there is always at least one empty slot at the end
            amount = 6;
        else
            amount = 5;

        for (int i = 0; i < amount; i++)
            IncreaseSlots(vendorSlotTemplate, vendorSlotsList);
    }

    void UpdateItems(List<UIItemInfo> listThatHoldsItemInfo, List<UIItemSlot> listToTransferItemInfoTo, GameObject slotTemplate)
    {
        for (int i = 0; i < listThatHoldsItemInfo.Count; i++)
        {
            if (i >= listToTransferItemInfoTo.Count)
                IncreaseSlots(slotTemplate, listToTransferItemInfoTo);

            listToTransferItemInfoTo[i].Assign(listThatHoldsItemInfo[i]);
            UpdateSlotDisplay(listToTransferItemInfoTo[i]);
        }
    }

    void RandomizeVendorWaresFromPool(int numVenderItemsFromPool)
    {
        for (int i = 0; i < numVenderItemsFromPool; i++)
            vendorSlotsList[i].Assign(currentShopkeep.shopkeepInventory[Random.Range(0, currentShopkeep.shopkeepInventory.Count)]);
    }

    void IncreaseSlots(GameObject template, List<UIItemSlot> listToAddTo)
    {
        GameObject newSlot = Instantiate(template);
        newSlot.transform.SetParent(template.transform.parent, false);
        listToAddTo.Add(newSlot.GetComponentInChildren<UIItemSlot>());
        ClearSlot(newSlot.GetComponentInChildren<UIItemSlot>());
        newSlot.SetActive(true);
    }

    void ClearSlot(UIItemSlot slot)
    {
        if (slot.IsAssigned())
            slot.Unassign();
        UpdateSlotDisplay(slot);
    }

    void UpdateSlotDisplay(UIItemSlot slot)
    {
        UIItemInfo slotItemInfo = slot.GetItemInfo();
        Text[] slotText = slot.transform.parent.GetComponentsInChildren<Text>(); //0 is name text, 1 is gold amount text
        bool playerSide = slot.GetComponent<ShopDragNDrop>().playerShopSide;

        if (slotItemInfo != null)
        {
            for(int i = 0; i < slotText.Length; i++)
            {
                if(slotText[i].name == "Name Text")
                    slotText[i].text = slotItemInfo.Name;
                if (slotText[i].name == "Gold Text")
                {
                    if(playerSide)
                        slotText[i].text = slotItemInfo.GoldValue.ToString();
                    else
                        slotText[i].text = Mathf.FloorToInt(slotItemInfo.GoldValue * currentShopkeep.merchantMarkup).ToString();
                }
            }
        }
        else
        {
            for (int i = 0; i < slotText.Length; i++)
            {
                if (slotText[i].name == "Name Text")
                    slotText[i].text = "";
                if (slotText[i].name == "Gold Text")
                    slotText[i].text = "--";
            }
        }
    }

    void ClearShopSlots(List<UIItemSlot> listToClear)
    {
        foreach(UIItemSlot slot in listToClear)
        {
            if (slot.isActiveAndEnabled)
                Destroy(slot.transform.parent.gameObject);
        }
        listToClear.Clear();
    }

    public void UpdateAllSlotDisplays()
    {
        foreach(UIItemSlot slot in playerSlotsList)
            UpdateSlotDisplay(slot);

        foreach (UIItemSlot slot in vendorSlotsList)
            UpdateSlotDisplay(slot);

        foreach (UIItemSlot slot in playerTradeSlotsList)
            UpdateSlotDisplay(slot);

        foreach (UIItemSlot slot in vendorTradeSlotsList)
            UpdateSlotDisplay(slot);
    }

    public void DoubleClickTrade(UIItemSlot itemSlot)
    {
        bool playerSide = itemSlot.GetComponent<ShopDragNDrop>().playerShopSide;

        if (playerSide)
        {
            TradePlayerOrVendorSide(itemSlot, playerTradeSlotTemplate, playerTradeSlotsList);
            StartCoroutine(ScrollToBottom("player"));
        }
        else
        {
            TradePlayerOrVendorSide(itemSlot, vendorTradeSlotTemplate, vendorTradeSlotsList);
            StartCoroutine(ScrollToBottom("vendor"));
        }
    }

    void TradePlayerOrVendorSide(UIItemSlot itemSlot, GameObject tradeSlotTemplate, List<UIItemSlot> tradeSlotList)
    {
        UIItemInfo slotItemInfo = itemSlot.GetItemInfo();
        if (slotItemInfo != null)
        {
            for (int i = 0; i < tradeSlotList.Count; i++)
            {
                if (!tradeSlotList[i].IsAssigned())
                {
                    tradeSlotList[i].Assign(slotItemInfo);
                    UpdateSlotDisplay(tradeSlotList[i]);
                    itemSlot.Unassign();
                    break;
                }
            }
        }
    }

    public void DoubleClickReturn(UIItemSlot itemSlot)
    {
        bool playerSide = itemSlot.GetComponent<ShopDragNDrop>().playerShopSide;
        UIItemInfo slotItemInfo = itemSlot.GetItemInfo();

        if (playerSide)
            ReturnSlotToMainGroupAndUnassign(itemSlot, playerSlotsList);
        else
            ReturnSlotToMainGroupAndUnassign(itemSlot, vendorSlotsList);
    }

    public void ReturnSlotToMainGroupAndUnassign(UIItemSlot itemSlot, List<UIItemSlot> listToTransferTo)
    {
        for (int i = 0; i < listToTransferTo.Count; i++)
        {
            if (!listToTransferTo[i].IsAssigned())
            {
                listToTransferTo[i].Assign(itemSlot);
                itemSlot.Unassign();
                break;
            }
        }
    }

    public void AddNewTradeSlot(UIItemSlot itemSlot)
    {
        bool playerSide = itemSlot.GetComponent<ShopDragNDrop>().playerShopSide;
        if (playerSide)
            IncreaseSlots(playerTradeSlotTemplate, playerTradeSlotsList);
        else
            IncreaseSlots(vendorTradeSlotTemplate, vendorTradeSlotsList);

    }

    public void RemoveUnneededTradeSlots(UIItemSlot itemSlot) //placed on unassign event in inspector
    {
        int unassignedSlots = 0;
        List<UIItemSlot> listToRemoveFrom = GetTradeListToRemoveFrom(itemSlot);

        for (int i = 0; i < listToRemoveFrom.Count; i++)
        {
            if (!listToRemoveFrom[i].IsAssigned())
                unassignedSlots++;

            if (unassignedSlots > 1)
            {
                unassignedSlots--;
                DeleteTradeSlotAndRemoveFromTradeList(listToRemoveFrom[i]);
            }
        }
    }
    public void DeleteTradeSlotAndRemoveFromTradeList(UIItemSlot itemSlot)
    {
        List<UIItemSlot> listToRemoveFrom = GetTradeListToRemoveFrom(itemSlot);

        Destroy(itemSlot.transform.parent.gameObject);
        listToRemoveFrom.Remove(itemSlot);
    }

    List<UIItemSlot> GetTradeListToRemoveFrom(UIItemSlot itemSlot)
    {
        List<UIItemSlot> listToRemoveFrom;
        bool playerSide = itemSlot.GetComponent<ShopDragNDrop>().playerShopSide;
        if (playerSide)
            listToRemoveFrom = playerTradeSlotsList;
        else
            listToRemoveFrom = vendorTradeSlotsList;

        return listToRemoveFrom;
    }

    IEnumerator ScrollToBottom(string side)
    {
        yield return new WaitForEndOfFrame();
        if(side == "player")
            playerTradeScrollRect.verticalNormalizedPosition = 0;
        else
            vendorTradeScrollRect.verticalNormalizedPosition = 0;
    }

    public void CalculateTradeValue()
    {
        goldSpent = 0;
        goldReceived = 0;

        for(int i = 0; i < playerTradeSlotsList.Count; i++)
        {
            if(playerTradeSlotsList[i].IsAssigned())
                goldReceived += playerTradeSlotsList[i].GetItemInfo().GoldValue;
        }

        for (int i = 0; i < vendorTradeSlotsList.Count; i++)
        {
            if (vendorTradeSlotsList[i].IsAssigned())
                goldSpent += Mathf.FloorToInt(vendorTradeSlotsList[i].GetItemInfo().GoldValue * currentShopkeep.merchantMarkup);
        }

        UpdateTradeUI();
    }

    public void UpdateTradeUI()
    {
        if (goldReceived >= goldSpent)
        {
            playerMoneyTradeText.text = "0";
            vendorMoneyTradeText.text = (goldReceived - goldSpent).ToString();
        }
        else
        {
            vendorMoneyTradeText.text = "0";
            playerMoneyTradeText.text = (goldSpent - goldReceived).ToString();
        }

        if ((goldSpent-goldReceived) <= InventoryManager.instance.currentGoldAmount && 
            (vendorTradeSlotsList.Count - 1) <= InventoryManager.instance.GetEmptyInventorySpaceAmount() && 
            (goldReceived - goldSpent) <= currentShopkeep.shopkeepGoldAmount)
        {
            acceptButton.interactable = true;
        }
        else
            acceptButton.interactable = false;
    }

    public void FinalizeTrade()
    {
        TradeGold();
        RemoveTradedItems(InventoryManager.instance.allInventorySlots, playerSlotsList);
        RemoveTradedItems(currentShopkeep.shopkeepInventory, vendorSlotsList);
        TransferTradedItems();

        ClearAllShopSlots();
        RefreshSlotsAndItems();
        CalculateTradeValue();
        SetNameTexts();
        SetMoneyTexts();
    }

    void TradeGold()
    {
        if (goldReceived >= goldSpent)
        {
            InventoryManager.instance.AddGold(goldReceived - goldSpent);
            currentShopkeep.shopkeepGoldAmount -= (goldReceived - goldSpent);
        }
        else
        {
            InventoryManager.instance.SpendGold(goldSpent - goldReceived);
            currentShopkeep.shopkeepGoldAmount += (goldSpent - goldReceived);
        }
    }

    void RemoveTradedItems(List<UIItemSlot> originalList, List<UIItemSlot> updatedTradeList)
    {
        for (int i = 0; i < originalList.Count; i++)
        {
            if (originalList[i].IsAssigned())
            {
                if (updatedTradeList[i].IsAssigned())
                {
                    if (originalList[i].GetItemInfo().ID != updatedTradeList[i].GetItemInfo().ID)
                        originalList[i].Assign(updatedTradeList[i].GetItemInfo());
                }
                else
                    originalList[i].Unassign();
            }
            else
            {
                if (updatedTradeList[i].IsAssigned())
                    originalList[i].Assign(updatedTradeList[i].GetItemInfo());
            }
        }
    }

    void RemoveTradedItems(List<UIItemInfo> originalList, List<UIItemSlot> updatedTradeList)
    {
        for (int i = 0; i < originalList.Count; i++)
        {
            if (originalList[i] != null)
            {
                if (updatedTradeList[i].IsAssigned())
                {
                    if (originalList[i].ID != updatedTradeList[i].GetItemInfo().ID)
                        originalList[i] = updatedTradeList[i].GetItemInfo();
                }
                else
                    originalList[i] = null;
            }
            else
            {
                if (updatedTradeList[i].IsAssigned())
                    originalList[i] = updatedTradeList[i].GetItemInfo();
            }
        }
    }

    void TransferTradedItems()
    {
        for (int i = 0; i < vendorTradeSlotsList.Count; i++)
        {
            if (vendorTradeSlotsList[i].IsAssigned())
                InventoryManager.instance.AddItem(vendorTradeSlotsList[i].GetItemInfo());
        }

        for (int i = 0; i < playerTradeSlotsList.Count; i++)
        {
            if (playerTradeSlotsList[i].IsAssigned())
                currentShopkeep.AddItem(playerTradeSlotsList[i].GetItemInfo());
        }
    }
}
