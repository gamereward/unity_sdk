using Grd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour {

    [SerializeField]
    private InputField usernameInput, passwordInput,otpInput;
    [SerializeField]
    private Text statusText;
    [SerializeField]
    GameObject stepLogin, stepOtp;
	// Use this for initialization
	void Start () {
        const string appId = "cc8b8744dbb1353393aac31d371af9a55a67df16";
        const string apiSecret = "1679091c5a880faf6fb5e6087eb1b2dc4daa3db355ef2b0e64b472968cb70f0df4be00279ee2e0a53eafdaa94a151e2ccbe3eb2dad4e422a7cba7b261d923784";
        GrdManager.Init(appId, apiSecret,GrdNet.Test);
        stepLogin.SetActive(true);
        stepOtp.SetActive(false);
	}
    public void OnBackButton()
    {

        stepLogin.SetActive(true);
        stepOtp.SetActive(false);
    }
    public void OnResetPassword()
    {
        if (usernameInput.text.Trim().Length == 0)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = "You must provide email or username";
            return;
        }
        ProgressPanel.Instance.Show("Requesting...");
        GrdManager.RequestResetPassword(usernameInput.text, (error, args) =>
        {
            ProgressPanel.Instance.Hide();
            if (error == 0)
            {
                statusText.gameObject.SetActive(true);
                statusText.text = "";
                PanelDoResetPassword.Instance.gameObject.SetActive(true);
            }
            else
            {
                statusText.gameObject.SetActive(true);
                statusText.text = "Error code:" + error + ",message:" + args.ErrorMessage;
            }
        });
    }
    public void OnLogin()
    {
        ProgressPanel.Instance.Show("Login...");
        GrdManager.Login(usernameInput.text, passwordInput.text, otpInput.text, (error, args) =>
        {
            ProgressPanel.Instance.Hide();
            if (error==0)
            {
                SceneManager.LoadScene("02_Menu");
            }
            else if (error == 4)
            {
                statusText.text = "";
                if (stepOtp.activeSelf)
                {
                    statusText.text = args.ErrorMessage;
                }
                //Need otp code
                stepOtp.SetActive(true);
                stepLogin.SetActive(false);
            }
            else
            {
                statusText.gameObject.SetActive(true);
                statusText.text = "Error code:" + error + ",message:" + args.ErrorMessage;
            }
        });
    }
    public void OnRegister()
    {
        RegisterPanel.Instance.Show();
    }
    // Update is called once per frame
    void Update () {
		
	}
}
