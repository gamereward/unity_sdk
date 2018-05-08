using Grd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransferMoneyPanel : MonoBehaviour {

    [SerializeField]
    private InputField addressTranfer, amountTransfer, otpCode;
    [SerializeField]
    private Text statusTransferText;
    void OnEnable()
    {
        otpCode.gameObject.SetActive(GrdManager.User.otp);//Has otp transfer options
    }
    public void OnBackButtonClick()
    {
        this.gameObject.SetActive(false);
    }
	// Use this for initialization
    /// <summary>
    /// Button Transfer Money Click
    /// </summary>
    public void TransferMoney()
    {
        statusTransferText.text = "";
        ProgressPanel.Instance.Show("Transfering...");
        GrdManager.Transfer(addressTranfer.text.Trim(), decimal.Parse(amountTransfer.text), otpCode.text.Trim(), (error, data) =>
        {
            ProgressPanel.Instance.Hide();
            if (error == 0)
            {
                statusTransferText.text = "Transfer successfully!";
            }
            else
            {
                statusTransferText.text = "Transfer error!Code:" + error.ToString() + "-Message:" + data.ErrorMessage;
            }

        });
    }
}
