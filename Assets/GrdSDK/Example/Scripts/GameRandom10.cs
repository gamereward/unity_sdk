using Grd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameRandom10 : MonoBehaviour
{
    [SerializeField]
    GameObject button;
    [SerializeField]
    Text statusText, totalMoney;
    [SerializeField]
    InputField betInput;
    GameObject[] buttons = new GameObject[10];
    [SerializeField]
    LeaderBoardPanel leaderBoard;
    [SerializeField]
    HistoryPanel history;
    void Start()
    {
        
        leaderBoard.scoreType = "random9_score";
        leaderBoard.ReloadLeaderBoard();
        history.store = "GAME-9";
        history.keys = new string[] { "rand" };
        history.formatItemFunction = new FormatItemDataHandler(GetHistoryText);
        history.LoadHistory();
        InitialBoard();
        UpdateMoney();
    }
    private string GetHistoryText(Grd.SessionData item)
    {
        if (item.values.ContainsKey("rand"))
        {
            string[] values = item.values["rand"].Split(',');//On server script we save the data: randNumber,playerNumber,winOrLoseMoney
            string st = item.GetTime().ToString("MM-dd HH:mm:ss-");
            st += "SELECT:" + values[1] + "-RESULT:" + values[0];
            if (decimal.Parse(values[2]) > 0)
            {
                st += "\r\nWIN:" + values[2];
            }
            else if (values[2] == "0")
            {
                st += "\r\nDRAW";
            }
            else
            {
                st += "\r\nLOSE:" + values[2];
            }
            return st;
        }
        return "";
    }
    private void UpdateMoney()
    {
        totalMoney.text = GrdManager.User.balance.ToString();
    }
    public void OnBack()
    {
        SceneManager.LoadScene("02_Menu");
    }
    private void InitialBoard()
    {
        button.SetActive(false);
        for (int i = 0; i < 9; i++)
        {
            GameObject btn = Instantiate(button, button.transform.parent);
            btn.SetActive(true);
            buttons[i] = btn;
            Text text = btn.GetComponentInChildren<Text>();
            Button b = btn.GetComponentInChildren<Button>();
            text.text = (i + 1).ToString();
            int number = i;
            b.onClick.AddListener(() =>
            {
                for (int j = 0; j < 9; j++)
                {
                    if (j != number)
                    {
                        buttons[j].GetComponent<Animator>().SetTrigger("Normal");
                    }
                    else
                    {
                        buttons[j].GetComponent<Animator>().SetTrigger("Select");
                    }
                }
                double bet = 0;
                double.TryParse(betInput.text, out bet);
                GrdManager.CallServerScript("testscript", "random9", new object[] { number + 1, bet }, (error, args) =>
                {
                    if (error == 0)
                    {
                        List<decimal> ls = args.GetData<List<decimal>>(); //random9 function Server return a array of number.
                        if (ls[0].ToString() == "0")
                        {
                            int randNumber = (int)(ls[1]);
                            int yournumber = (int)(ls[2]);
                            decimal money = ls[3];
                            SessionData session = new SessionData()
                            {
                                sessionstart = GrdManager.GetEpochTime(),
                                values = new Dictionary<string, string>()
                            };
                            session.values.Add("rand", ls[1].ToString() + "," + ls[2].ToString() + "," + ls[3].ToString());
                            if (money > 0)
                            {
                                statusText.text = "CONRATULATIONS! YOU WIN:" + money.ToString() + " GRD";
                                leaderBoard.ReloadLeaderBoard();
                            }
                            else
                            {
                                statusText.text = "NO LUCKY:" + money.ToString() + " GRD";

                            }
                            history.AddHistory(session);
                            buttons[randNumber - 1].GetComponent<Animator>().SetTrigger("Win");
                            GrdManager.User.balance += money;
                            UpdateMoney();
                        }
                        else
                        {
                            statusText.text = "Not success:" + ls[1].ToString();
                        }

                    }
                    else
                    {
                        statusText.text = "Errorcode:" + error + ",message:" + args.ErrorMessage;
                    }
                });
            });
        }
    }
}
