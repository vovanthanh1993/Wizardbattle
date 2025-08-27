using System.Collections;
using UnityEngine;
using TMPro;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class EmailPassLogin : MonoBehaviour
{
    #region variables
    [Header("Login")]
    [SerializeField] private TMP_InputField _loginEmail;
    [SerializeField] private TMP_InputField _loginPassword;
    [SerializeField] private TMP_InputField _firstName;
    [SerializeField] private TMP_InputField _lastName;

    [Header("Sign up")]
    [SerializeField] private TMP_InputField _signupEmail;
    [SerializeField] private TMP_InputField _signupPassword;

    [Header("Extra")]
    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private GameObject _loginUi, _signupUi;
    [SerializeField] private NoticePopup _noticePopup;

    private FirebaseAuth auth;
    private bool isFirebaseInitialized = false;

    #endregion

    #region Unity Methods
    void Start()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Firebase đã sẵn sàng
                auth = FirebaseAuth.DefaultInstance;
                isFirebaseInitialized = true;
            }
            else
            {
                ShowLogMsg("Firebase initialization failed. Please check your configuration.");
            }
        });
    }
    #endregion

    #region signup 
    public void SignUp()
    {
        if (!isFirebaseInitialized)
        {
            ShowLogMsg("Data is not initialized. Please wait...");
            return;
        }

        _loadingScreen.SetActive(true);

        string email = _signupEmail.text;
        string password = _signupPassword.text;
        string firstName = _firstName.text;
        string lastName = _lastName.text;

        if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
        {
            ShowLogMsg("Please enter your email, password, first name and last name.");
            _loadingScreen.SetActive(false);
            return;
        }

        if (!IsValidEmail(email))
        {
            ShowLogMsg("Please enter a valid email address.");
            _loadingScreen.SetActive(false);
            return;
        }

        // Kiểm tra độ mạnh mật khẩu
        if (password.Length < 6)
        {
            ShowLogMsg("Password must be at least 6 characters long.");
            _loadingScreen.SetActive(false);
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(async task => {
            if (task.IsCanceled)
            {
                ShowLogMsg("Registration cancelled. Please try again!");
                _loadingScreen.SetActive(false);
                return;
            }
            if (task.IsFaulted)
            {
                
                // Xử lý lỗi Firebase cụ thể
                if (task.Exception != null)
                {
                    FirebaseException firebaseException = task.Exception.GetBaseException() as FirebaseException;
                    if (firebaseException != null)
                    {
                        AuthError error = (AuthError)firebaseException.ErrorCode;
                        string errorMessage = GetSignUpErrorMessage(error);
                        ShowLogMsg(errorMessage);
                    }
                    else
                    {
                        ShowLogMsg("Registration error: " + task.Exception.Message);
                    }
                }
                else
                {
                    ShowLogMsg("Unspecified registration error. Please try again!");
                }
                _loadingScreen.SetActive(false);
                return;
            }

            AuthResult result = task.Result;

            // Lưu dữ liệu user vào Firebase Database
            bool saveSuccess = await SaveUserDataToFirebase(result.User, firstName, lastName);
            
            if (saveSuccess)
            {
                _signupEmail.text = "";
                _signupPassword.text = "";
                _firstName.text = "";
                _lastName.text = "";

                if (result.User.IsEmailVerified)
                {
                    ShowLogMsg("Sign up Successful! Your data has been saved.");
                }
                else {
                    ShowLogMsg("Please verify your email!! Your data has been saved.");
                    SendEmailVerification();
                }
            }
            else
            {
                ShowLogMsg("Account created but failed to save data. Please contact support.");
            }
            
            _loadingScreen.SetActive(false);
        });
    }

    private async Task<bool> SaveUserDataToFirebase(FirebaseUser user, string firstName, string lastName)
    {
        try
        {
            // Đợi FirebaseDataManager sẵn sàng
            int maxWaitTime = 20;
            int waitCount = 0;
            
            while ((FirebaseDataManager.Instance == null || !FirebaseDataManager.Instance.IsInitialized()) && waitCount < maxWaitTime)
            {
                await Task.Delay(1000);
                waitCount++;
                Debug.Log($"Waiting for FirebaseDataManager... ({waitCount}/{maxWaitTime})");
            }
            
            if (FirebaseDataManager.Instance == null || !FirebaseDataManager.Instance.IsInitialized())
            {
                return false;
            }

            string displayName = $"{firstName}{lastName}";
            PlayerData newPlayerData = new PlayerData(user.Email, displayName, user.UserId);
            bool saveSuccess = await FirebaseDataManager.Instance.SavePlayerData(newPlayerData);
            
            if (saveSuccess)
            {
                return true;
            }
            else
            {
                Debug.LogError("Failed to save player data to Firebase");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving user data to Firebase: {e.Message}");
            return false;
        }
    }

    public void SendEmailVerification() {
        StartCoroutine(SendEmailForVerificationAsync());
    }

    IEnumerator SendEmailForVerificationAsync() {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user!=null)
        {
            var sendEmailTask = user.SendEmailVerificationAsync();
            yield return new WaitUntil(() => sendEmailTask.IsCompleted);

            if (sendEmailTask.Exception != null)
            {
                FirebaseException firebaseException = sendEmailTask.Exception.GetBaseException() as FirebaseException;
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string errorMessage = GetSignUpErrorMessage(error);
                ShowLogMsg(errorMessage);
            }
            else {
                print("Email successfully send");
                _signupUi.SetActive(false);
                _loginUi.SetActive(true);
            }
        }
    }


    #endregion

    #region Login
    public void Login() {
        if (!isFirebaseInitialized)
        {
            ShowLogMsg("Data is not initialized. Please wait...");
            return;
        }

        _loadingScreen.SetActive(true);

        string email = _loginEmail.text;
        string password = _loginPassword.text;

        if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowLogMsg("Please enter your email and password.");
            _loadingScreen.SetActive(false);
            return;
        }

        // Kiểm tra định dạng email
        if (!IsValidEmail(email))
        {
            ShowLogMsg("Please enter a valid email address.");
            _loadingScreen.SetActive(false);
            return;
        }

        try
        {
            Credential credential = EmailAuthProvider.GetCredential(email, password);
            
            auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(async task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                    ShowLogMsg("Login was cancelled. Please try again.");
                    _loadingScreen.SetActive(false);
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                    
                    // Xử lý lỗi Firebase cụ thể
                    if (task.Exception != null)
                    {
                        FirebaseException firebaseException = task.Exception.GetBaseException() as FirebaseException;
                        if (firebaseException != null)
                        {
                            AuthError error = (AuthError)firebaseException.ErrorCode;
                            string errorMessage = GetLoginErrorMessage(error);
                            ShowLogMsg(errorMessage);
                        }
                        else
                        {
                            ShowLogMsg("Login error: " + task.Exception.Message);
                        }
                    }
                    else
                    {
                        ShowLogMsg("An unexpected error occurred during login. Please try again.");
                    }
                    _loadingScreen.SetActive(false);
                    return;
                }

                AuthResult result = task.Result;
                Debug.Log($"User logged in successfully: {result.User.Email}");
                
                // Cập nhật thời gian đăng nhập cuối
                await UpdateLastLoginTime(result.User.UserId);
                
                if (result.User.IsEmailVerified)
                {
                    _loginUi.SetActive(false);
                    await LoadSceneAsync("MainMenuScene");
                }
                else {
                    _loadingScreen.SetActive(false);
                    ShowLogMsg("Please verify your email before logging in!");
                }
                
                
            });
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception during login: " + ex.Message);
            ShowLogMsg("Login failed. Please try again.");
            _loadingScreen.SetActive(false);
        }
    }

    private async Task LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            Debug.Log($"Loading progress: {progress * 100}%");
            await Task.Yield();
        }
        _loadingScreen.SetActive(false);
    }
    private async Task UpdateLastLoginTime(string userId)
    {
        try
        {
            if (FirebaseDataManager.Instance != null && FirebaseDataManager.Instance.IsInitialized())
            {
                PlayerData currentData = await FirebaseDataManager.Instance.LoadPlayerData();
                if (currentData != null)
                {
                    currentData.lastLoginTime = System.DateTime.Now;
                    await FirebaseDataManager.Instance.SavePlayerData(currentData);
                    Debug.Log("Last login time updated successfully");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to update last login time: {e.Message}");
        }
    }
    #endregion

    #region extra
    void ShowLogMsg(string msg)
    {
        _noticePopup.ShowNoticePopup(msg);
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private string GetLoginErrorMessage(AuthError error)
    {
        switch (error)
        {
            case AuthError.None:
                return "Login successful!";
            case AuthError.InvalidEmail:
                return "Invalid email address.";
            case AuthError.WrongPassword:
                return "Incorrect password. Please try again.";
            case AuthError.UserNotFound:
                return "No account found with this email address.";
            case AuthError.UserDisabled:
                return "This account has been disabled.";
            case AuthError.TooManyRequests:
                return "Too many login attempts. Please try again later.";
            case AuthError.NetworkRequestFailed:
                return "Network error. Please check your internet connection.";
            case AuthError.InvalidApiKey:
                return "Authentication service error. Please contact support.";
            case AuthError.AppNotAuthorized:
                return "App not authorized. Please contact support.";
            case AuthError.QuotaExceeded:
                return "Service temporarily unavailable. Please try again later.";
            case AuthError.UnverifiedEmail:
                return "Please verify your email before logging in.";
            case AuthError.RequiresRecentLogin:
                return "Please log in again for security reasons.";
            case AuthError.InvalidCredential:
                return "Invalid login credentials.";
            case AuthError.OperationNotAllowed:
                return "Email/password login is not enabled.";
            case AuthError.InvalidUserToken:
                return "Session expired. Please log in again.";
            case AuthError.UserTokenExpired:
                return "Session expired. Please log in again.";
            case AuthError.WebInternalError:
                return "Internal server error. Please try again later.";
            default:
                return "Login failed. Please try again.";
        }
    }

    private string GetSignUpErrorMessage(AuthError error)
    {
        switch (error)
        {
            case AuthError.None:
                return "Registration successful!";
            case AuthError.Unimplemented:
                return "Registration feature not implemented.";
            case AuthError.Failure:
                return "Registration failed. Please try again.";
            case AuthError.InvalidCustomToken:
                return "Invalid custom token.";
            case AuthError.CustomTokenMismatch:
                return "Custom token mismatch.";
            case AuthError.InvalidCredential:
                return "Invalid credential.";
            case AuthError.UserDisabled:
                return "Account disabled.";
            case AuthError.AccountExistsWithDifferentCredentials:
                return "Account already exists with different credentials.";
            case AuthError.OperationNotAllowed:
                return "Registration not allowed.";
            case AuthError.EmailAlreadyInUse:
                return "Email already in use.";
            case AuthError.RequiresRecentLogin:
                return "Requires recent login.";
            case AuthError.CredentialAlreadyInUse:
                return "Credential already in use.";
            case AuthError.InvalidEmail:
                return "Invalid email.";
            case AuthError.WrongPassword:
                return "Wrong password.";
            case AuthError.TooManyRequests:
                return "Too many registration requests. Please try again later.";
            case AuthError.UserNotFound:
                return "User not found.";
            case AuthError.ProviderAlreadyLinked:
                return "Account already linked with a different provider.";
            case AuthError.NoSuchProvider:
                return "Provider not found.";
            case AuthError.InvalidUserToken:
                return "Invalid user token.";
            case AuthError.UserTokenExpired:
                return "User token expired.";
            case AuthError.NetworkRequestFailed:
                return "Network request failed. Please try again.";
            case AuthError.InvalidApiKey:
                return "Invalid API key.";
            case AuthError.AppNotAuthorized:
                return "App not authorized.";
            case AuthError.UserMismatch:
                return "User mismatch.";
            case AuthError.WeakPassword:
                return "Weak password. Please choose a stronger password.";
            case AuthError.NoSignedInUser:
                return "No user signed in.";
            case AuthError.ApiNotAvailable:
                return "API not available.";
            case AuthError.ExpiredActionCode:
                return "Action code expired.";
            case AuthError.InvalidActionCode:
                return "Invalid action code.";
            case AuthError.InvalidMessagePayload:
                return "Invalid message payload.";
            case AuthError.InvalidPhoneNumber:
                return "Invalid phone number.";
            case AuthError.MissingPhoneNumber:
                return "Phone number missing.";
            case AuthError.InvalidRecipientEmail:
                return "Invalid recipient email.";
            case AuthError.InvalidSender:
                return "Invalid sender email.";
            case AuthError.InvalidVerificationCode:
                return "Invalid verification code.";
            case AuthError.InvalidVerificationId:
                return "Invalid verification ID.";
            case AuthError.MissingVerificationCode:
                return "Verification code missing.";
            case AuthError.MissingVerificationId:
                return "Verification ID missing.";
            case AuthError.MissingEmail:
                return "Email missing.";
            case AuthError.MissingPassword:
                return "Password missing.";
            case AuthError.QuotaExceeded:
                return "Quota exceeded. Please try again later.";
            case AuthError.RetryPhoneAuth:
                return "Phone authentication request failed. Please try again.";
            case AuthError.SessionExpired:
                return "Session expired.";
            case AuthError.AppNotVerified:
                return "App not verified.";
            case AuthError.AppVerificationFailed:
                return "App verification failed.";
            case AuthError.CaptchaCheckFailed:
                return "Captcha check failed.";
            case AuthError.InvalidAppCredential:
                return "Invalid app credential.";
            case AuthError.MissingAppCredential:
                return "App credential missing.";
            case AuthError.InvalidClientId:
                return "Invalid client ID.";
            case AuthError.InvalidContinueUri:
                return "Continue URI invalid.";
            case AuthError.MissingContinueUri:
                return "Continue URI missing.";
            case AuthError.KeychainError:
                return "Keychain error.";
            case AuthError.MissingAppToken:
                return "App token missing.";
            case AuthError.MissingIosBundleId:
                return "iOS Bundle ID missing.";
            case AuthError.NotificationNotForwarded:
                return "Notification not forwarded.";
            case AuthError.UnauthorizedDomain:
                return "Unauthorized domain.";
            case AuthError.WebContextAlreadyPresented:
                return "Web context already presented.";
            case AuthError.WebContextCancelled:
                return "Web context cancelled.";
            case AuthError.DynamicLinkNotActivated:
                return "Dynamic link not activated.";
            case AuthError.Cancelled:
                return "Registration cancelled.";
            case AuthError.InvalidProviderId:
                return "Invalid provider ID.";
            case AuthError.WebInternalError:
                return "Web internal error.";
            case AuthError.WebStorateUnsupported:
                return "Web storage unsupported.";
            case AuthError.TenantIdMismatch:
                return "Tenant ID mismatch.";
            case AuthError.UnsupportedTenantOperation:
                return "Unsupported tenant operation.";
            case AuthError.InvalidLinkDomain:
                return "Invalid link domain.";
            case AuthError.RejectedCredential:
                return "Credential rejected.";
            case AuthError.PhoneNumberNotFound:
                return "Phone number not found.";
            case AuthError.InvalidTenantId:
                return "Invalid tenant ID.";
            case AuthError.MissingClientIdentifier:
                return "Client identifier missing.";
            case AuthError.MissingMultiFactorSession:
                return "Multi-factor session missing.";
            case AuthError.MissingMultiFactorInfo:
                return "Multi-factor info missing.";
            case AuthError.InvalidMultiFactorSession:
                return "Invalid multi-factor session.";
            case AuthError.MultiFactorInfoNotFound:
                return "Multi-factor info not found.";
            case AuthError.AdminRestrictedOperation:
                return "Operation restricted by administrator.";
            case AuthError.UnverifiedEmail:
                return "Email not verified.";
            case AuthError.SecondFactorAlreadyEnrolled:
                return "Second factor already enrolled.";
            case AuthError.MaximumSecondFactorCountExceeded:
                return "Maximum second factor count exceeded.";
            case AuthError.UnsupportedFirstFactor:
                return "First factor not supported.";
            case AuthError.EmailChangeNeedsVerification:
                return "You need to verify your email to change it.";
            default:
                return "Unspecified registration error.";
        }
    }
    #endregion

}