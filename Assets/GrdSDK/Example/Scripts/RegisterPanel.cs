using Grd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : MonoBehaviour {
    public RegisterPanel()
    {
        _instance = this;
    }
    private static RegisterPanel _instance;
    public static RegisterPanel Instance { get { return _instance; } }
    [SerializeField]
    private InputField usernameField, passwordField, emailField;
    [SerializeField]
    private Text statusText;
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void OnAccept()
    {
        if (usernameField.text.Trim() == "")
        {
            statusText.text = "Username can not be empty!";
            return;
        }
        if (passwordField.text.Trim() == "")
        {
            statusText.text = "Password can not be empty!";
            return;
        }
        if (emailField.text.Trim() == "")
        {
            statusText.text = "Email can not be empty!";
            return;
        }
        statusText.text = "";
        ProgressPanel.Instance.Show("Registering...");
        GrdManager.Register(usernameField.text, passwordField.text, emailField.text,"", (error,args) =>
        {
            ProgressPanel.Instance.Hide();
            if (error==0)
            {
                statusText.text = "CREATE ACCOUNT SUCCESSFULLY!";
            }
            else
            {
                statusText.text = "Errorcode:" + error.ToString() + ",message:" + args.ErrorMessage;
            }
        });
    }
    public void OnBack()
    {
        gameObject.SetActive(false);
    }
}
