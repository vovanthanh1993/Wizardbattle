using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

public class EmailLoginUI : MonoBehaviour
{
    [Header("Login Panel")]
    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private TMP_InputField _emailInput;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _switchToRegisterButton;
    
    [Header("Register Panel")]
    [SerializeField] private GameObject _registerPanel;
    [SerializeField] private TMP_InputField _registerEmailInput;
    [SerializeField] private TMP_InputField _registerPasswordInput;
    [SerializeField] private TMP_InputField _registerDisplayNameInput;
    [SerializeField] private Button _registerButton;
    [SerializeField] private Button _switchToLoginButton;
    
    [Header("Status")]
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private GameObject _loadingPanel;
    
    [Header("User Info")]
    [SerializeField] private GameObject _userInfoPanel;
    [SerializeField] private TMP_Text _userNameText;
    [SerializeField] private TMP_Text _userLevelText;
    [SerializeField] private TMP_Text _userStatsText;
    [SerializeField] private Button _logoutButton;
    [SerializeField] private Button _playGameButton;
    
    private bool _isLoading = false;
    
    private void Start()
    {
        SetupButtons();
        CheckAuthStatus();
        
        // Subscribe to Firebase events
        if (FirebaseAuthSystem.Instance != null)
        {
            FirebaseAuthSystem.Instance.OnLoginResult += OnLoginResult;
            FirebaseAuthSystem.Instance.OnRegisterResult += OnRegisterResult;
            FirebaseAuthSystem.Instance.OnUserDataLoaded += OnUserDataLoaded;
            FirebaseAuthSystem.Instance.OnError += OnError;
        }
    }
    
    private void SetupButtons()
    {
        // Login buttons
        _loginButton.onClick.AddListener(OnLoginClicked);
        _switchToRegisterButton.onClick.AddListener(SwitchToRegister);
        
        // Register buttons
        _registerButton.onClick.AddListener(OnRegisterClicked);
        _switchToLoginButton.onClick.AddListener(SwitchToLogin);
        
        // User info buttons
        _logoutButton.onClick.AddListener(OnLogoutClicked);
        _playGameButton.onClick.AddListener(OnPlayGameClicked);
    }
    
    private void CheckAuthStatus()
    {
        if (FirebaseAuthSystem.Instance != null && FirebaseAuthSystem.Instance.IsLoggedIn)
        {
            ShowUserInfo();
        }
        else
        {
            ShowLoginPanel();
        }
    }
    
    private void ShowLoginPanel()
    {
        _loginPanel.SetActive(true);
        _registerPanel.SetActive(false);
        _userInfoPanel.SetActive(false);
        _loadingPanel.SetActive(false);
        SetStatus("");
    }
    
    private void ShowRegisterPanel()
    {
        _loginPanel.SetActive(false);
        _registerPanel.SetActive(true);
        _userInfoPanel.SetActive(false);
        _loadingPanel.SetActive(false);
        SetStatus("");
    }
    
    private void ShowUserInfo()
    {
        _loginPanel.SetActive(false);
        _registerPanel.SetActive(false);
        _userInfoPanel.SetActive(true);
        _loadingPanel.SetActive(false);
        
        if (FirebaseAuthSystem.Instance.CurrentUser != null)
        {
            var userData = FirebaseAuthSystem.Instance.CurrentUser;
            _userNameText.text = $"Welcome, {userData.displayName}!";
            _userLevelText.text = $"Level {userData.level} ({userData.experience} XP)";
            
            float winRate = userData.wins + userData.losses > 0 
                ? (float)userData.wins / (userData.wins + userData.losses) * 100 
                : 0;
            
            _userStatsText.text = $"Wins: {userData.wins} | Losses: {userData.losses} | Win Rate: {winRate:F1}%\n" +
                                 $"Kills: {userData.kills} | Deaths: {userData.deaths} | Coins: {userData.coins}";
        }
    }
    
    private void ShowLoading(bool show)
    {
        _isLoading = show;
        _loadingPanel.SetActive(show);
        _loginButton.interactable = !show;
        _registerButton.interactable = !show;
    }
    
    private void SetStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.text = message;
            _statusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }
    }
    
    private async void OnLoginClicked()
    {
        if (_isLoading) return;
        
        string email = _emailInput.text.Trim();
        string password = _passwordInput.text.Trim();
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetStatus("Please enter both email and password!");
            return;
        }
        
        if (!FirebaseAuthSystem.Instance.IsValidEmail(email))
        {
            SetStatus("Please enter a valid email address!");
            return;
        }
        
        ShowLoading(true);
        SetStatus("Logging in...");
        
        bool success = await FirebaseAuthSystem.Instance.LoginUser(email, password);
        
        ShowLoading(false);
        
        if (success)
        {
            SetStatus("Login successful!");
            ShowUserInfo();
        }
        else
        {
            SetStatus("Login failed. Please check your credentials.");
        }
    }
    
    private async void OnRegisterClicked()
    {
        if (_isLoading) return;
        
        string email = _registerEmailInput.text.Trim();
        string password = _registerPasswordInput.text.Trim();
        string displayName = _registerDisplayNameInput.text.Trim();
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(displayName))
        {
            SetStatus("Please fill in all fields!");
            return;
        }
        
        if (!FirebaseAuthSystem.Instance.IsValidEmail(email))
        {
            SetStatus("Please enter a valid email address!");
            return;
        }
        
        if (!FirebaseAuthSystem.Instance.IsValidPassword(password))
        {
            SetStatus("Password must be at least 6 characters!");
            return;
        }
        
        ShowLoading(true);
        SetStatus("Creating account...");
        
        bool success = await FirebaseAuthSystem.Instance.RegisterUser(email, password, displayName);
        
        ShowLoading(false);
        
        if (success)
        {
            SetStatus("Registration successful!");
            ShowUserInfo();
        }
        else
        {
            SetStatus("Registration failed. Please try again.");
        }
    }
    
    private void SwitchToRegister()
    {
        ShowRegisterPanel();
    }
    
    private void SwitchToLogin()
    {
        ShowLoginPanel();
    }
    
    private void OnLogoutClicked()
    {
        FirebaseAuthSystem.Instance.Logout();
        ShowLoginPanel();
        SetStatus("Logged out successfully!");
    }
    
    private void OnPlayGameClicked()
    {
        // Navigate to game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GamePlay");
    }
    
    private void OnLoginResult(bool success)
    {
        if (success)
        {
            SetStatus("Login successful!");
            ShowUserInfo();
        }
        else
        {
            SetStatus("Login failed. Please check your credentials.");
        }
    }
    
    private void OnRegisterResult(bool success)
    {
        if (success)
        {
            SetStatus("Registration successful!");
            ShowUserInfo();
        }
        else
        {
            SetStatus("Registration failed. Please try again.");
        }
    }
    
    private void OnUserDataLoaded(UserData userData)
    {
        ShowUserInfo();
    }
    
    private void OnError(string errorMessage)
    {
        SetStatus($"Error: {errorMessage}");
    }
    
    private void OnDestroy()
    {
        if (FirebaseAuthSystem.Instance != null)
        {
            FirebaseAuthSystem.Instance.OnLoginResult -= OnLoginResult;
            FirebaseAuthSystem.Instance.OnRegisterResult -= OnRegisterResult;
            FirebaseAuthSystem.Instance.OnUserDataLoaded -= OnUserDataLoaded;
            FirebaseAuthSystem.Instance.OnError -= OnError;
        }
    }
} 