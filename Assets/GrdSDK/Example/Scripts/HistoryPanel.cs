using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryPanel : MonoBehaviour
{
    public FormatItemDataHandler formatItemFunction;
    [System.NonSerialized]
    public string store;
    [System.NonSerialized]
    public string[] keys;
    [SerializeField]
    GameObject itemTemplate;
    List<GameObject> items = new List<GameObject>();
    // Use this for initialization
    int count = 10;
    bool isLoading = false;
    void Start()
    {
        itemTemplate.SetActive(false);
    }
    private string GetItemText(Grd.SessionData item)
    {
        if (formatItemFunction == null)
        {
            string data = "";
            foreach (string key in item.values.Keys)
            {
                data += "," + key + "=" + item.values[key];
            }
            if (data.Length > 0)
            {
                data = data.Substring(1);
            }
            return item.GetTime().ToString("MM-dd HH:mm:ss-" + data);
        }
        return formatItemFunction(item);
    }
    public void AddHistory(Grd.SessionData data)
    {
        GameObject item = Instantiate(itemTemplate, itemTemplate.transform.parent);
        item.SetActive(true);
        items.Add(item);
        for (int i = items.Count - 1; i > 1; i--)
        {
            items[i].GetComponentInChildren<Text>().text = items[i - 1].GetComponentInChildren<Text>().text;
        }
        items[0].GetComponentInChildren<Text>().text = GetItemText(data);
    }
    public void LoadHistory()
    {
        if (isLoading) return;
        isLoading = true;
        Grd.GrdManager.GetUserSessionData(store,keys, 0, count, (error, args) =>
        {
            if (error == 0)
            {
                for (int i = 0; i < args.Data.Count; i++)
                {
                    if (i >= items.Count)
                    {
                        GameObject item = Instantiate(itemTemplate, itemTemplate.transform.parent);
                        item.SetActive(true);
                        items.Add(item);
                    }
                    items[i].GetComponentInChildren<Text>().text = GetItemText(args.Data[i]);
                }
            }
            isLoading = false;
        });
    }
}
public delegate string FormatItemDataHandler(Grd.SessionData item);