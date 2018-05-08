using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject itemTemplate;
    public string scoreType;
    private int lastIndex = 0;
    private const int count = 20;
    private bool isLoading = false;
    private List<GameObject> items = new List<GameObject>();
    private ScrollRect scrollRect;
    private bool isPointerDown;
    private Vector2 startPointerPos;
    // Use this for initialization
    void Awake()
    {
        scrollRect = this.GetComponent<ScrollRect>();
        itemTemplate.SetActive(false);
    }
    void LoadLeaderBoard(int start)
    {
        if (isLoading && start > 0)
        {
            return;
        }
        isLoading = true;
        Grd.GrdManager.GetLeaderBoard(scoreType, start, count, (error, args) =>
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
                        g.transform.FindChild("rank").GetComponent<Text>().text = "#" + dataItem.rank.ToString();
                        g.transform.FindChild("name").GetComponent<Text>().text = dataItem.username;
                        g.transform.FindChild("score").GetComponent<Text>().text = dataItem.score.ToString();
                        items.Add(g);
                    }
                    this.lastIndex += args.Data.Count;
                }
            }
            isLoading = false;
        });
    }
    public void ReloadLeaderBoard()
    {
        this.lastIndex = 0;
        LoadLeaderBoard(this.lastIndex);
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
            if (Vector2.Distance(Input.mousePosition, startPointerPos) >= 4&&Mathf.Approximately(scrollRect.verticalNormalizedPosition,0))
            {
                LoadLeaderBoard(this.lastIndex);
            }
        }

    }
}
