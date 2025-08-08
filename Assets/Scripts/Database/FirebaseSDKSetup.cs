using UnityEngine;

public class FirebaseSDKSetup : MonoBehaviour
{
    [Header("Firebase Configuration")]
    [SerializeField] private string _projectId = "wizardbattle-b3901";
    [SerializeField] private string _apiKey = "1:381819246771:android:9826aaba8a056bb83e78c6";
    [SerializeField] private string _authDomain = "wizardbattle-b3901.firebaseapp.com";
    [SerializeField] private string _databaseURL = "https://wizardbattle-b3901-default-rtdb.firebaseio.com/";
    
    [Header("How to Update Configuration")]
    [TextArea(10, 15)]
    [SerializeField] private string _updateInstructions = "HOW TO UPDATE FIREBASE CONFIG:\n\n" +
        "1. Go to Firebase Console: https://console.firebase.google.com/\n" +
        "2. Select your project\n" +
        "3. Get Project ID: Project Settings > General\n" +
        "4. Get API Key: Project Settings > General > Your apps\n" +
        "5. Get Auth Domain: Authentication > Settings\n" +
        "6. Get Database URL: Realtime Database\n" +
        "7. Update the values above in this component\n" +
        "8. Click 'Validate Configuration' to check\n\n" +
        "OR use the Context Menu (right-click this component):\n" +
        "- Show Setup Instructions\n" +
        "- Validate Configuration";
    
    [Header("Setup Instructions")]
    [TextArea(15, 25)]
    [SerializeField] private string _setupInstructions = "FIREBASE SDK SETUP INSTRUCTIONS:\n\n" +
        "1. CREATE FIREBASE PROJECT:\n" +
        "   - Go to https://console.firebase.google.com/\n" +
        "   - Click 'Create a project' or select existing project\n" +
        "   - Follow the setup wizard\n\n" +
        "2. ADD UNITY APP TO FIREBASE:\n" +
        "   - In Firebase Console, click 'Add app'\n" +
        "   - Select Unity icon\n" +
        "   - Enter your app nickname\n" +
        "   - Download google-services.json (Android) or GoogleService-Info.plist (iOS)\n" +
        "   - Place the config file in Assets/Plugins/\n\n" +
        "3. INSTALL FIREBASE SDK:\n" +
        "   Method 1 - Package Manager:\n" +
        "   - Window > Package Manager\n" +
        "   - Click '+' > Add package from git URL\n" +
        "   - Add these packages one by one:\n" +
        "     * com.google.firebase.app\n" +
        "     * com.google.firebase.auth\n" +
        "     * com.google.firebase.database\n\n" +
        "   Method 2 - Asset Store:\n" +
        "   - Search for 'Firebase Unity SDK'\n" +
        "   - Download and import the package\n\n" +
        "4. UPDATE FIREBASE CONFIG:\n" +
        "   - Replace the values above with your project settings\n" +
        "   - Project ID: Found in Project Settings\n" +
        "   - API Key: Found in Project Settings > General\n" +
        "   - Auth Domain: Found in Authentication > Settings\n" +
        "   - Database URL: Found in Realtime Database\n\n" +
        "5. ENABLE AUTHENTICATION:\n" +
        "   - In Firebase Console, go to Authentication\n" +
        "   - Click 'Get started'\n" +
        "   - Enable 'Email/Password' provider\n" +
        "   - Add your test users or allow registration\n\n" +
        "6. SETUP REALTIME DATABASE:\n" +
        "   - In Firebase Console, go to Realtime Database\n" +
        "   - Click 'Create database'\n" +
        "   - Choose location and start in test mode\n" +
        "   - Set up security rules for your data\n\n" +
        "7. UPDATE SECURITY RULES:\n" +
        "   {\n" +
        "     \"rules\": {\n" +
        "       \"users\": {\n" +
        "         \"$uid\": {\n" +
        "           \".read\": \"$uid === auth.uid\",\n" +
        "           \".write\": \"$uid === auth.uid\"\n" +
        "         }\n" +
        "       }\n" +
        "     }\n" +
        "   }\n\n" +
        "8. TEST THE SYSTEM:\n" +
        "   - Run the game\n" +
        "   - Try registering a new account\n" +
        "   - Try logging in with existing account\n" +
        "   - Check Firebase Console for data\n\n" +
        "TROUBLESHOOTING:\n" +
        "- Make sure google-services.json is in Assets/Plugins/\n" +
        "- Check that all Firebase packages are installed\n" +
        "- Verify your project settings are correct\n" +
        "- Check Unity Console for error messages\n" +
        "- Ensure Authentication is enabled in Firebase Console";

    public string SetupInstructions => _setupInstructions;
    public string UpdateInstructions => _updateInstructions;
    
    private void OnValidate()
    {
        // Validate configuration
        if (string.IsNullOrEmpty(_projectId) || _projectId == "your-project-id")
        {
            Debug.LogWarning("Firebase Project ID not configured! Please update FirebaseSDKSetup.");
        }
        
        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "your-api-key")
        {
            Debug.LogWarning("Firebase API Key not configured! Please update FirebaseSDKSetup.");
        }
    }
    
    [ContextMenu("Show Setup Instructions")]
    private void ShowSetupInstructions()
    {
        Debug.Log(_setupInstructions);
    }
    
    [ContextMenu("Show Update Instructions")]
    private void ShowUpdateInstructions()
    {
        Debug.Log(_updateInstructions);
    }
    
    [ContextMenu("Validate Configuration")]
    private void ValidateConfiguration()
    {
        bool isValid = true;
        
        if (string.IsNullOrEmpty(_projectId) || _projectId == "your-project-id")
        {
            Debug.LogError("Project ID not configured!");
            isValid = false;
        }
        
        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "your-api-key")
        {
            Debug.LogError("API Key not configured!");
            isValid = false;
        }
        
        if (string.IsNullOrEmpty(_authDomain) || _authDomain == "your-project.firebaseapp.com")
        {
            Debug.LogError("Auth Domain not configured!");
            isValid = false;
        }
        
        if (string.IsNullOrEmpty(_databaseURL) || _databaseURL == "https://your-project.firebaseio.com")
        {
            Debug.LogError("Database URL not configured!");
            isValid = false;
        }
        
        if (isValid)
        {
            Debug.Log("Firebase configuration is valid!");
        }
    }
} 