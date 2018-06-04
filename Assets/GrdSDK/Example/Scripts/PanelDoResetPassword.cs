using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelDoResetPassword : MonoBehaviour {
    public PanelDoResetPassword()
    {
        _instance = this;
    }
    public InputField tokenField, passwordField, retypePasswordField;
    public Text messageText;
    // Use this for initialization
    static PanelDoResetPassword _instance;
    public static PanelDoResetPassword Instance { get { return _instance; } }
    void OnEnable()
    {
        messageText.text = "An email was sent to your email. Please check email to get token to change your password!";
    }
    public void OnResetClick()
    {
        if (tokenField.text.Trim() == "")
        {
            messageText.text = "Token can not be empty!. Please check email to get token to change your password!";
            return;
        }
        if (passwordField.text != retypePasswordField.text)
        {
            messageText.text = "Retype password is not matched!";
            return;
        }
        Grd.GrdManager.ResetPassword(tokenField.text, passwordField.text,(error,args)=>{
            if (error == 0)
            {
                messageText.text = "YOUR PASSWORD HAD BEEN RESET!";
            }
            else
            {
                messageText.text = args.ErrorMessage;
            }
        });
    }
    public void OnMenuClick()
    {
        this.gameObject.SetActive(false);
    }
}
