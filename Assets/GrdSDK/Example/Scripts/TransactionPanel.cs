using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransactionPanel : MonoBehaviour {

    [SerializeField]
    private GameObject itemTemplate;
    private int lastIndex = 0;
    private const int count = 20;
    private bool isLoading = false;
    private List<GameObject> items = new List<GameObject>();
    private ScrollRect scrollRect;
    private bool isPointerDown;
    private Vector2 startPointerPos;
    void Awake()
    {
        scrollRect = this.GetComponentInChildren<ScrollRect>();
        itemTemplate.SetActive(false);
    }
    void OnEnable()
    {
        ReloadTransactions();
    }
    void LoadTransactions(int start)
    {
        if (isLoading && start > 0)
        {
            return;
        }
        isLoading = true;
        Grd.GrdManager.GetTransactions(start, count, (error, args) =>
        {
            if (start == 0)
            {
                //Reload ->Clear all item
                for (int i = 0; i < items.Count; i++)
                {
                    Destroy(items[i]);
                }
                items.Clear();
            }
            if (error == 0)
            {
                if (start == this.lastIndex)
                {
                    for (int i = 0; i < args.Data.Count; i++)
                    {
                        var dataItem = args.Data[i];
                        GameObject g = Instantiate(itemTemplate, itemTemplate.transform.parent);
                        g.SetActive(true); 
                        g.transform.FindChild("transdate").GetComponent<Text>().text = dataItem.GetTime().ToString();
                        g.transform.FindChild("from").GetComponent<Text>().text =  dataItem.from.ToString();
                        g.transform.FindChild("to").GetComponent<Text>().text = dataItem.to;
                        g.transform.FindChild("amount").GetComponent<Text>().text = dataItem.amount.ToString();
                        g.transform.FindChild("status").GetComponent<Text>().text = (dataItem.status==Grd.TransactionStatus.Pending?"pending":(dataItem.status==Grd.TransactionStatus.Success?"success":"error"));
                        g.transform.FindChild("transtype").GetComponent<Text>().text = (dataItem.transtype == Grd.TransactionType.Base ? "base" : (dataItem.transtype == Grd.TransactionType.External ? "external" : "internal"));
                        items.Add(g);
                    }
                    this.lastIndex += args.Data.Count;
                }
            }
            isLoading = false;
        });
    }
    public void ReloadTransactions()
    {
        this.lastIndex = 0;
        LoadTransactions(this.lastIndex);
    }
    public void OnPointerDown()
    {
        if (!isLoading)
        {
            isPointerDown = true;
            startPointerPos = Input.mousePosition;
        }
    }
    public void OnPointerUp()
    {
        if (isPointerDown && !isLoading)
        {
            isPointerDown = false;
            if (Vector2.Distance(Input.mousePosition, startPointerPos) >= 4 && Mathf.Approximately(scrollRect.verticalNormalizedPosition, 0))
            {
                LoadTransactions(this.lastIndex);
            }
        }

    }
    public void OnBackButton()
    {
        gameObject.SetActive(false);
    }
}
