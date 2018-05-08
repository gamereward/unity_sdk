using Grd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHighLow : MonoBehaviour
{
    [SerializeField]
    Text totalMoney, highRate, lowRate, messageText;
    [SerializeField]
    InputField betInput;
    [SerializeField]
    Image card1, card2;
    [SerializeField]
    Sprite[] cardSprites;
    [SerializeField]
    Sprite cardBack;
    [SerializeField]
    GameObject[] buttons;
    [SerializeField]
    LeaderBoardPanel leaderBoard;
    [SerializeField]
    HistoryPanel history;
    // Use this for initialization
    Dictionary<int, Sprite> cardSpriteByIndex = new Dictionary<int, Sprite>();
    Card currentCard, resultCard;
    List<string> lsSuit = new List<string>(new string[] { "clubs", "spades", "diamonds", "hearts" });
    private bool isShow = false;
    void Start()
    {
        leaderBoard.scoreType = "lowhighgame_score";
        leaderBoard.ReloadLeaderBoard();
        history.store = "LOWHIGHGAME";
        history.keys = new string[] { "result" };
        history.formatItemFunction = new FormatItemDataHandler((data) =>
        {
            return data.GetTime().ToString("MM-dd HH:mm:ss " + data.values["result"]);
        });
        history.LoadHistory();
        InitCardSprite();
        ShowCardBack(card2);
        RandomCard();
        DisplayMoney();
    }
    private void InitCardSprite()
    {
        foreach (Sprite s in cardSprites)
        {
            int i = s.name.IndexOf("_");
            int symbol = int.Parse(s.name.Substring(0, i));
            i = s.name.IndexOf("_of_") + 4;
            string ss = s.name.Substring(i).ToLower();
            if (char.IsNumber(ss[ss.Length - 1]))
            {
                ss = ss.Substring(0, ss.Length - 1);
            }
            int suit = lsSuit.IndexOf(ss);
            int key = symbol * 4 + suit;
            if (!cardSpriteByIndex.ContainsKey(key))
                cardSpriteByIndex.Add(key, s);
            else
                cardSpriteByIndex[key] = s;
        }
    }
    private void AllowUI(bool isAllow)
    {
        foreach (GameObject g in buttons)
        {
            g.SetActive(isAllow);
        }
    }
    private void RandomCard()
    {
        int rsymbol = Random.Range(4, 10);
        int rsuit = Random.Range(0, 3);
        currentCard = new Card()
        {
            symbol = rsymbol,
            suit = rsuit
        };
        card1.sprite = GetCardSprite(currentCard);

        UpdateRate();
    }
    private Sprite GetCardSprite(Card c)
    {
        int rindex = (c.symbol) * 4 + c.suit;
        return cardSpriteByIndex[rindex];
    }
    private void UpdateRate()
    {
        decimal bet = 0;
        decimal.TryParse(betInput.text, out bet);
        lowRate.text = System.Math.Round(((14 - currentCard.symbol) * bet) / (currentCard.symbol - 2),2).ToString() + "/" + bet.ToString();
        highRate.text = System.Math.Round(((currentCard.symbol - 2) * bet) / (14 - currentCard.symbol),2).ToString() + "/" + bet.ToString();
    }
    public void ShowCardBack(Image img)
    {
        img.sprite = cardBack;
    }
    private IEnumerator HideCard(Image img,bool islow)
    {
        if (img.sprite.name != cardBack.name)
        {
            for (int i = 0; i < 10; i++)
            {
                img.transform.localScale = new Vector3(Mathf.Abs(i - 5.0f) / 5.0f, 1, 1);
                if (i == 5)
                {
                    img.sprite = cardBack;
                }
                yield return new WaitForSeconds(0.05f);
            }
        }
        img.transform.localScale = new Vector3(1, 1, 1);
        Bet(islow);
    }
    private void DisplayMoney()
    {
        totalMoney.text = GrdManager.User.balance.ToString();
    }
    private IEnumerator ShowCard(Image img, Card card,decimal money,bool isLow)
    {
        for (int i = 0; i < 10; i++)
        {
            img.transform.localScale = new Vector3(Mathf.Abs(i - 5.0f) / 5.0f, 1, 1);
            if (i == 5)
            {
                img.sprite = GetCardSprite(card);
            }
            yield return new WaitForSeconds(0.05f);
        }
        SessionData session = new SessionData()
        {
            sessionstart = GrdManager.GetEpochTime(),
            values = new Dictionary<string, string>()
        };
        if (money > 0)
        {
            messageText.text = "YOU WIN:"+money;
        }
        else if(money<0)
        {

            messageText.text = "YOU LOSE:" + money;
        }
        else
        {
            messageText.text = "DRAW";
        }
        session.values.Add("result", "["+(isLow?"1":"0")+","+currentCard.symbol.ToString()+","+resultCard.symbol+","+money.ToString()+"]");
        history.AddHistory(session);
        img.transform.localScale = new Vector3(1, 1, 1);
        AllowUI(true);
    }
    public void OnBetChanged()
    {
        UpdateRate();
    }
    public void OnRandom()
    {
        RandomCard();
    }
    public void OnBet(bool isLow)
    {
        AllowUI(false);
        StartCoroutine(HideCard(card2,isLow));
    }

    private void Bet(bool isLow)
    {
        decimal bet = 0;
        decimal.TryParse(betInput.text, out bet);

        GrdManager.CallServerScript("testscript", "lowhighgame", new object[] { isLow ? 1 : 0, currentCard.symbol, bet }, (error, args) =>
        {
            if (error == 0)
            {
                List<object> ls = (List<object>)args.Data;//lowhighgame return array: element 0 is success or failed, element 1 is card object and element 2 is money
                if (ls[0].ToString() == "0")
                {
                    resultCard = MiniJSON.Json.GetObject<Card>(ls[1]);
                    isShow = true;
                    decimal money = decimal.Parse(ls[2].ToString());
                    GrdManager.User.balance += money;
                    DisplayMoney();
                    if (resultCard.symbol >= 2 && resultCard.symbol <= 14)
                    {
                        StartCoroutine(ShowCard(card2, resultCard, money,isLow));
                    }
                    else
                    {
                        AllowUI(true);
                    }
                }
                else
                {
                    AllowUI(true);
                    messageText.text = ls[1].ToString();
                }
            }
            else
            {
                AllowUI(true);
                messageText.text = args.ErrorMessage;
            }
        });
    }

    public void OnBack()
    {
        SceneManager.LoadScene("02_Menu");
    }
}
public class Card
{
    public int symbol;
    public int suit;
}
