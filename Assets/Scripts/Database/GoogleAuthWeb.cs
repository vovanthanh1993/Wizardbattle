using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Net;
using Firebase;
using Firebase.Auth;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class GoogleAuthWeb : MonoBehaviour
{
    [Header("Google OAuth Configuration")]
    [SerializeField] private string clientId = "332085799900-65iqjq46pmg2dnj3kce87kv30phtaakf.apps.googleusercontent.com";
    [SerializeField] private string clientSecret = "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";
    [SerializeField] private string redirectUri = "http://localhost:5000/callback/";
    
    [Header("Scene Management")]
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private bool autoLoadMenuOnSuccess = true;
    
    private HttpListener httpListener;
    private FirebaseAuth auth;
    private bool isInitialized = false;
    
    // Events
    public event Action<FirebaseUser> OnSignInSuccess;
    public event Action<string> OnSignInError;

    private void Start()
    {
        InitializeFirebase();
    }
    
    private void OnDestroy()
    {
        if (httpListener != null && httpListener.IsListening)
        {
            httpListener.Stop();
            httpListener.Close();
        }
    }
    
    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                isInitialized = true;
                Debug.Log("Firebase initialized successfully");
            }
            else
            {
                Debug.LogError("Firebase dependencies not available.");
                OnSignInError?.Invoke("Firebase dependencies not available.");
            }
        });
    }

    public async void SignInWithGoogle()
    {
        if (!isInitialized)
        {
            OnSignInError?.Invoke("Firebase not initialized yet. Please wait.");
            return;
        }
        
        try
        {
            string state = Guid.NewGuid().ToString();
            
            string authUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
                           $"?client_id={Uri.EscapeDataString(clientId)}" +
                           $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                           $"&response_type=code" +
                           $"&scope={Uri.EscapeDataString("openid email profile")}" +
                           $"&state={state}" +
                           $"&access_type=offline";

            Application.OpenURL(authUrl);
            await StartHttpListener(state);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error starting Google auth: {e.Message}");
            OnSignInError?.Invoke($"Error starting Google auth: {e.Message}");
        }
    }
    
    private async Task StartHttpListener(string expectedState)
    {
        try
        {
            httpListener = new HttpListener();
            
            // Ensure the prefix ends with '/'
            string prefix = redirectUri;
            if (!prefix.EndsWith("/"))
            {
                prefix += "/";
            }
            
            httpListener.Prefixes.Add(prefix);
            httpListener.Start();
            
            var context = await httpListener.GetContextAsync();
            await HandleAuthResponse(context, expectedState);
        }
        catch (Exception e)
        {
            Debug.LogError($"HTTP listener error: {e.Message}");
            OnSignInError?.Invoke($"HTTP listener error: {e.Message}");
        }
        finally
        {
            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.Stop();
                httpListener.Close();
            }
        }
    }

    private async Task HandleAuthResponse(HttpListenerContext context, string expectedState)
    {
        try
        {
            var query = context.Request.QueryString;
            string code = query["code"];
            string state = query["state"];
            string error = query["error"];
            
            string responseHtml = "<html><body><h1>Authentication Complete</h1><p>You can close this window now.</p></body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseHtml);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
            
            if (!string.IsNullOrEmpty(error))
            {
                OnSignInError?.Invoke($"OAuth error: {error}");
                return;
            }
            
            if (string.IsNullOrEmpty(code))
            {
                OnSignInError?.Invoke("No authorization code received");
                return;
            }
            
            if (state != expectedState)
            {
                OnSignInError?.Invoke("State mismatch");
                return;
            }
            
            await ExchangeCodeForToken(code);
        }
        catch (Exception e)
        {
            OnSignInError?.Invoke($"Error handling auth response: {e.Message}");
        }
    }

    private async Task ExchangeCodeForToken(string code)
    {
        try
        {
            string tokenUrl = "https://oauth2.googleapis.com/token";
            string postData = $"code={Uri.EscapeDataString(code)}" +
                            $"&client_id={Uri.EscapeDataString(clientId)}" +
                            $"&client_secret={Uri.EscapeDataString(clientSecret)}" +
                            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                            $"&grant_type=authorization_code";

            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string response = await client.UploadStringTaskAsync(tokenUrl, postData);
                
                Debug.Log($"Token response: {response}");
                
                var tokenResponse = JsonUtility.FromJson<TokenResponse>(response);
                
                if (string.IsNullOrEmpty(tokenResponse.access_token))
                {
                    OnSignInError?.Invoke("No access token in response");
                    return;
                }
                
                Debug.Log("Access token received, getting user info...");
                await GetUserInfoAndSignIn(tokenResponse.access_token);
            }
        }
        catch (Exception e)
        {
            OnSignInError?.Invoke($"Error exchanging code for token: {e.Message}");
        }
    }

    private async Task GetUserInfoAndSignIn(string accessToken)
    {
        try
        {
            // Get user info from Google
            string userInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";
            using (var client = new WebClient())
            {
                client.Headers.Add("Authorization", $"Bearer {accessToken}");
                string userInfoResponse = await client.DownloadStringTaskAsync(userInfoUrl);
                
                Debug.Log($"User info response: {userInfoResponse}");
                
                var userInfo = JsonUtility.FromJson<GoogleUserInfo>(userInfoResponse);
                
                // Create custom token or use email/password
                await SignInWithEmailPassword(userInfo.email, userInfo.name);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error getting user info: {e.Message}");
            OnSignInError?.Invoke($"Error getting user info: {e.Message}");
        }
    }

    private async Task SignInWithEmailPassword(string email, string displayName)
    {
        try
        {
            Debug.Log($"Signing in with email: {email}");
            
            // Try to sign in with email/password
            var userCredential = await auth.SignInWithEmailAndPasswordAsync(email, "temp_password");
            var user = userCredential.User;
            
            if (user != null)
            {
                Debug.LogFormat("User signed in successfully: {0} ({1})", displayName, email);
                OnSignInSuccess?.Invoke(user);
                
                if (autoLoadMenuOnSuccess)
                {
                    LoadMenuScene();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Email sign in failed: {e.Message}");
            
            // If email sign in fails, try to create account
            try
            {
                Debug.Log("Trying to create new account...");
                var userCredential = await auth.CreateUserWithEmailAndPasswordAsync(email, "temp_password");
                var newUser = userCredential.User;
                
                if (newUser != null)
                {
                    Debug.LogFormat("New user created successfully: {0} ({1})", displayName, email);
                    OnSignInSuccess?.Invoke(newUser);
                    
                    if (autoLoadMenuOnSuccess)
                    {
                        LoadMenuScene();
                    }
                }
            }
            catch (Exception createError)
            {
                Debug.LogError($"Account creation failed: {createError.Message}");
                OnSignInError?.Invoke($"Authentication failed: {createError.Message}");
            }
        }
    }
    
    public void LoadMenuScene()
    {
        try
        {
            Debug.Log($"Loading menu scene: {menuSceneName}");
            SceneManager.LoadScene(menuSceneName);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading menu scene: {e.Message}");
        }
    }
    
    public void LoadMenuSceneWithDelay(float delay = 1f)
    {
        StartCoroutine(LoadMenuSceneCoroutine(delay));
    }
    
    private System.Collections.IEnumerator LoadMenuSceneCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadMenuScene();
    }
    
    public void SignOut()
    {
        if (auth != null)
        {
            auth.SignOut();
        }
    }
    
    public FirebaseUser GetCurrentUser()
    {
        return auth?.CurrentUser;
    }

    [Serializable]
    private class TokenResponse
    {
        public string access_token;
        public string expires_in;
        public string token_type;
        public string refresh_token;
        public string id_token;
    }
    
    [Serializable]
    private class GoogleUserInfo
    {
        public string id;
        public string email;
        public string name;
        public string given_name;
        public string family_name;
        public string picture;
        public string locale;
    }
}
