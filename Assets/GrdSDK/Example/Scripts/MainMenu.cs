using Grd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    // Use this for initialization
    [SerializeField]
    private Text addressText, balanceText;
    [SerializeField]
    private Image qrcodeAddress;
    [SerializeField]
    private GameObject optPanel, transferPanel,transactionPanel;
    void Start()
    {
        addressText.text = GrdManager.User.address;
        balanceText.text = GrdManager.User.balance.ToString();
        GrdManager.GetAddressQRCode(GrdManager.User.address, (error, args) =>
        {
            if (qrcodeAddress == null) return;//Destroyed
            Texture2D texture = args.Texture;
            qrcodeAddress.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        });
        optPanel.SetActive(false);
        transferPanel.SetActive(false);
    }
    public void OnCopyAddress()
    {
        GUIUtility.systemCopyBuffer = addressText.text;
    }
    public void OnOtpSettingButtonClick()
    {
        optPanel.SetActive(true);
    }

    public void OnTransactionButtonClick()
    {
        transactionPanel.SetActive(true);
    }
    public void OnTransferButtonClick()
    {
        transferPanel.SetActive(true);
    } 
    /// <summary>
    /// Buton Game 1 demo click
    /// </summary>
    public void DemoGame1()
    {
        SceneManager.LoadScene("Game1");
    }
    /// <summary>
    /// Button game 2 demo click
    /// </summary>
    public void DemoGame2()
    {

        SceneManager.LoadScene("Game2");
    }
    
}
