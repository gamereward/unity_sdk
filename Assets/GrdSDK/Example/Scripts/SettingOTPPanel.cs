using Grd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingOTPPanel : MonoBehaviour {

    [SerializeField]
    Image qrCodeOtpServerKey;
    [SerializeField]
    Text statusText, otpServerKeyText;
    [SerializeField]
    private Toggle toggleOtpEnabled;
    [SerializeField]
    private InputField otpCodeField;
    [SerializeField]
    private GameObject otpInstructionPanel;
	// Use this for initialization
	void OnEnable () {
        DisplayOTPOptions();
	}

    public void OnBackButtonClick()
    {
        this.gameObject.SetActive(false);
    }
    private void DisplayOTPOptions()
    {
        toggleOtpEnabled.isOn = GrdManager.User.otp;
        otpInstructionPanel.SetActive(false);
    }
    /// <summary>
    /// Button download Google authenticator application from 2 store click.
    /// </summary>
    /// <param name="store">Store google play or Appstore</param>
    public void OnDownloadOTPApp(string store)
    {
        if (store == "chplay")
        {
            Application.OpenURL("https://play.google.com/store/apps/details?id=com.google.android.apps.authenticator2");
        }
        else
        {
            Application.OpenURL("https://itunes.apple.com/vn/app/google-authenticator/id388497605?mt=8");
        }
    }
    /// <summary>
    /// Label Secret code from server click to copy to clipboard.
    /// </summary>
    public void CopyOtpServerKeyToClipBoard()
    {
        GUIUtility.systemCopyBuffer = otpServerKeyText.text;
    }
    /// <summary>
    /// Toogle the checkbox Enable Otp
    /// </summary>
    public void ToggleOtp()
    {
        if (GrdManager.User.otp)
        {
            //Already enabled
            if (!toggleOtpEnabled.isOn)
            {
                otpInstructionPanel.SetActive(false);
            }
        }
        else
        {
            //Not enabled yet
            if (toggleOtpEnabled.isOn)
            {
                if (!GrdManager.User.otp)
                {
                    //Not enabled-> Check to enabled
                    GrdManager.RequestEnableOtp((error, args) =>
                    {
                        if (error == 0)
                        {
                            otpServerKeyText.text = args.Text;
                            CopyOtpServerKeyToClipBoard();
                            Texture2D texture = args.Texture;
                            qrCodeOtpServerKey.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                            otpInstructionPanel.SetActive(true);
                        }
                        else
                        {
                            statusText.text = args.ErrorMessage;
                        }
                    });
                }
            }
            else
            {
                //Turn on and turn off however do not press save
                otpInstructionPanel.SetActive(false);
            }
        }
    }
    /// <summary>
    /// Buton save Otp settings click
    /// </summary>
    public void SaveEnableOtp()
    {
        bool enabled = false;
        if (toggleOtpEnabled.isOn)
        {
            enabled = true;
        }
        GrdManager.EnableOtp(otpCodeField.text.Trim(), enabled, (error, data) =>
        {
            if (error == 0)
            {
                statusText.text = toggleOtpEnabled.isOn ? "OTP is enabled" : "OTP is disabled";
                otpInstructionPanel.SetActive(false);
            }
            else
            {
                statusText.text = data.ErrorMessage;
            }
        });
    }
    
}
