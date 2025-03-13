using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using Photon.Pun;
using Firebase.Database;
using System.Collections;
using UnityEngine.Events;
using System.Threading.Tasks;

public class FirebaseManagerScript : MonoBehaviour
{
    public PlayerData playerData;
    private FirebaseAuth auth;
    private DatabaseReference dbReference;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField emailSignupInput;
    public TMP_InputField passwordSignupInput;
    public TMP_InputField nicknameInput;
    public TextMeshProUGUI statusText;

    private UnityEvent loggin = new UnityEvent();

    [SerializeField]
    private GameObject lobbyGameObject;
    [SerializeField]
    private GameObject LoginGameObject;
    private bool isLoggin = false;
    private string currentUserId;

    void Start()
    {
        loggin.AddListener(UserisLogIn);
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        ResetUserStatusOnAppStart(currentUserId);
    }

    private void UserisLogIn()
    {
        Debug.Log("Lobby opened");
        lobbyGameObject.SetActive(true);
        LoginGameObject.SetActive(false);
        isLoggin = false;
    }

    public void Register()
    {
        auth.CreateUserWithEmailAndPasswordAsync(emailSignupInput.text, passwordSignupInput.text).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
                statusText.text = "Registration Failed!";
            else
            {
                FirebaseUser newUser = auth.CurrentUser;
                SaveNicknameToFirebase(newUser.UserId, nicknameInput.text);
                statusText.text = "Registration Successful!";
            }
        });
    }

    public void Login()
    {
        auth.SignInWithEmailAndPasswordAsync(emailInput.text, passwordInput.text).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                statusText.text = "Login Failed!";
                return;
            }

            FirebaseUser loggedInUser = auth.CurrentUser;
            CheckIfUserAlreadyLoggedIn(loggedInUser.UserId);
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void CheckIfUserAlreadyLoggedIn(string userId)
    {
        dbReference.Child("users").Child(userId).Child("isLoggedIn").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                statusText.text = "Error checking login status!";
                return;
            }

            if (task.Result.Exists && (bool)task.Result.Value)
            {
                statusText.text = "User already logged in!";
            }
            else
            {
                SetUserLoggedIn(userId);
            }
        });
    }

    private void SetUserLoggedIn(string userId)
    {
        dbReference.Child("users").Child(userId).Child("isLoggedIn").SetValueAsync(true).ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                statusText.text = "Error setting login status!";
                return;
            }

            FirebaseUser loggedInUser = auth.CurrentUser;
            statusText.text = "Login Successful!";
            GetNicknameFromFirebase(loggedInUser.UserId);
            playerData.userId = loggedInUser.UserId;
            SetupAutoLogoutOnDisconnect(userId);
            loggin.Invoke();
        });
    }

    private void SaveNicknameToFirebase(string userId, string nickname)
    {
        dbReference.Child("users").Child(userId).Child("nickname").SetValueAsync(nickname);
    }

    private void GetNicknameFromFirebase(string userId)
    {
        dbReference.Child("users").Child(userId).Child("nickname").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                statusText.text = "Failed to get nickname!";
                return;
            }

            if (task.Result.Exists)
            {
                string nickname = task.Result.Value.ToString();
                PhotonConnect(nickname);
            }
        });
    }

    private void PhotonConnect(string nickname)
    {
        playerData.nickname = nickname;
        PhotonNetwork.NickName = playerData.nickname;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Logout()
    {
        if (auth.CurrentUser != null)
        {
            string userId = auth.CurrentUser.UserId;
            dbReference.Child("users").Child(userId).Child("isLoggedIn").SetValueAsync(false).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    auth.SignOut();
                    lobbyGameObject.SetActive(false);
                    LoginGameObject.SetActive(true);
                    statusText.text = "Logged out!";
                }
            });
        }
    }
    private void SetupAutoLogoutOnDisconnect(string userId)
    {
        // If player disconnects (crash, close game), set `isLoggedIn` to false
        dbReference.Child("users").Child(userId).Child("isLoggedIn").OnDisconnect().SetValue(false);
    }
    private void ResetUserStatusOnAppStart(string userId)
    {
        dbReference.Child("users").Child(userId).Child("isLoggedIn").SetValueAsync(false);
    }
    void OnApplicationQuit()
    {
        Logout();
        PhotonNetwork.Disconnect(); 

    }
}
