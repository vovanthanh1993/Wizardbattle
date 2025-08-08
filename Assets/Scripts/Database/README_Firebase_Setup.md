# Firebase Authentication Setup Guide

## Tổng quan
Hệ thống login bằng email với Firebase đã được tạo sẵn. Hiện tại đang sử dụng mock data để test, khi cần tích hợp Firebase thực tế, chỉ cần uncomment các dòng code trong `FirebaseRealImplementation.cs`.

## Các file đã tạo:

### 1. FirebaseAuthSystem.cs
- Hệ thống authentication chính
- Quản lý đăng ký, đăng nhập, logout
- Lưu trữ thông tin user

### 2. EmailLoginUI.cs
- UI cho hệ thống login
- Giao diện đăng ký và đăng nhập
- Hiển thị thông tin user

### 3. FirebaseRealImplementation.cs
- Implementation thực tế cho Firebase SDK
- Các method đã được comment sẵn
- Chỉ cần uncomment khi cài đặt Firebase SDK

### 4. FirebaseSDKSetup.cs
- Hướng dẫn setup Firebase SDK
- Validation configuration
- Troubleshooting

## Cách sử dụng:

### Bước 1: Setup Firebase Project
1. Truy cập https://console.firebase.google.com/
2. Tạo project mới hoặc chọn project có sẵn
3. Thêm Unity app vào project
4. Download `google-services.json` (Android) hoặc `GoogleService-Info.plist` (iOS)
5. Đặt file config vào `Assets/Plugins/`

### Bước 2: Cài đặt Firebase SDK
**Method 1 - Package Manager:**
```
Window > Package Manager > + > Add package from git URL
Thêm lần lượt:
- com.google.firebase.app
- com.google.firebase.auth
- com.google.firebase.database
```

**Method 2 - Asset Store:**
- Tìm "Firebase Unity SDK"
- Download và import package

### Bước 3: Cấu hình Firebase
1. Mở `FirebaseSDKSetup.cs`
2. Cập nhật các thông tin project:
   - Project ID
   - API Key
   - Auth Domain
   - Database URL

### Bước 4: Enable Authentication
1. Trong Firebase Console, vào Authentication
2. Click "Get started"
3. Enable "Email/Password" provider
4. Thêm test users hoặc cho phép đăng ký

### Bước 5: Setup Database
1. Trong Firebase Console, vào Realtime Database
2. Click "Create database"
3. Chọn location và start in test mode
4. Cập nhật security rules:

```json
{
  "rules": {
    "users": {
      "$uid": {
        ".read": "$uid === auth.uid",
        ".write": "$uid === auth.uid"
      }
    }
  }
}
```

### Bước 6: Tích hợp vào game
1. Tạo GameObject với `FirebaseAuthSystem` component
2. Tạo GameObject với `EmailLoginUI` component
3. Setup UI elements trong Inspector
4. Test hệ thống

## Chuyển từ Mock sang Real Firebase:

### 1. Uncomment Firebase imports
Trong `FirebaseRealImplementation.cs`, uncomment:
```csharp
using Firebase;
using Firebase.Auth;
using Firebase.Database;
```

### 2. Uncomment Firebase initialization
```csharp
// Check Firebase dependencies
var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
if (dependencyStatus == DependencyStatus.Available)
{
    // Initialize Firebase Auth
    _auth = FirebaseAuth.DefaultInstance;
    
    // Initialize Firebase Database
    _database = FirebaseDatabase.DefaultInstance.RootReference;
    
    _isInitialized = true;
    Debug.Log("Firebase initialized successfully");
    OnInitializationComplete?.Invoke(true);
}
```

### 3. Uncomment các method thực tế
- `RegisterUserWithFirebase()`
- `LoginUserWithFirebase()`
- `LoadUserDataFromFirebase()`
- `SaveUserDataToFirebase()`

## Testing:

### Test đăng ký:
1. Chạy game
2. Click "Register"
3. Nhập email, password, display name
4. Click "Register"
5. Kiểm tra Firebase Console

### Test đăng nhập:
1. Sử dụng email/password đã đăng ký
2. Click "Login"
3. Kiểm tra thông tin user hiển thị

### Test logout:
1. Click "Logout"
2. Kiểm tra quay về màn hình login

## Troubleshooting:

### Lỗi thường gặp:
1. **"Firebase not initialized"**
   - Kiểm tra Firebase SDK đã cài đặt chưa
   - Kiểm tra `google-services.json` đã đặt đúng vị trí

2. **"Registration failed"**
   - Kiểm tra Authentication đã enable Email/Password chưa
   - Kiểm tra security rules của Database

3. **"Login failed"**
   - Kiểm tra email/password đúng không
   - Kiểm tra user đã tồn tại trong Firebase Console

4. **"Failed to load user data"**
   - Kiểm tra Database rules
   - Kiểm tra user data có tồn tại trong Database

### Debug:
- Mở Unity Console để xem log
- Kiểm tra Firebase Console để xem data
- Sử dụng Firebase Debug View để debug

## Security Rules mẫu:

```json
{
  "rules": {
    "users": {
      "$uid": {
        ".read": "$uid === auth.uid",
        ".write": "$uid === auth.uid"
      }
    },
    "public": {
      ".read": true,
      ".write": "auth != null"
    }
  }
}
```

## Next Steps:
1. Thêm password reset functionality
2. Thêm email verification
3. Thêm social login (Google, Facebook)
4. Thêm user profile management
5. Thêm data synchronization
6. Thêm offline support 