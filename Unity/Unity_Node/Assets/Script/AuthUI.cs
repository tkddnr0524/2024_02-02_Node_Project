using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class AuthUI : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;

    public Button registerButton;
    public Button loginButton;
    public Button logoutButton;
    public Button getDataButton;


    public Text statusText;

    private AuthManager authManager;

    // Start is called before the first frame update
    void Start()
    {
        authManager = GetComponent<AuthManager>();
        registerButton.onClick.AddListener(OnRegisterClick);
        loginButton.onClick.AddListener(OnLoginClick);
        logoutButton.onClick.AddListener(OnLogoutClick);
        getDataButton.onClick.AddListener(OnGetDataClick);

    }

    private void OnLoginClick()
    {
        StartCoroutine(LoginCorouitine());
    }

    private IEnumerator LoginCorouitine()
    {
        statusText.text = "�α��� ��...";
        yield return StartCoroutine(authManager.Login(usernameInput.text, passwordInput.text));
        statusText.text = "�α��� ����";
    }
    private void OnRegisterClick()
    {
        StartCoroutine(RegisterCorouitine());
    }

    private IEnumerator RegisterCorouitine()
    {
        statusText.text = "ȸ������ ��...";
        yield return StartCoroutine(authManager.Register(usernameInput.text, passwordInput.text));
        statusText.text = "ȸ������ ����. �α��� ���ּ���";
    }

    private void OnLogoutClick()
    {
        StartCoroutine(LogoutCorouitine());
    }

    private IEnumerator LogoutCorouitine()
    {
        statusText.text = "�α׾ƿ� ��...";
        yield return StartCoroutine(authManager.Logout());
        statusText.text = "�α׾ƿ� ����";
    }
    private void OnGetDataClick()
    {
        StartCoroutine(GetDataCorouitine());
    }

    private IEnumerator GetDataCorouitine()
    {
        statusText.text = "������ ��û ��...";
        yield return StartCoroutine(authManager.GetProtectedData());
        statusText.text = "������ ��û ����";
    }
}
