using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressPanel : MonoBehaviour {
    public ProgressPanel()
    {
        _instance = this;
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    [SerializeField]
    private Text statusText;
    public static ProgressPanel Instance { get { return _instance; } }
    private static ProgressPanel _instance;

    public void Show(string message)
    {
        statusText.text = message;
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
