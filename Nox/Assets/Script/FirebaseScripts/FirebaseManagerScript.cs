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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loggin.AddListener(UserisLogIn);
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("Firebase App Name: " + FirebaseApp.DefaultInstance.Name);
        Debug.Log("Database URL: " + FirebaseApp.DefaultInstance.Options.DatabaseUrl);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void UserisLogIn()
    {
        Debug.Log("lobby opened");
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
        //if (string.IsNullOrEmpty(nicknameInput.text))
        //{
        //    statusText.text = "Please enter a nickname!";
        //    return;
        //}
        auth.SignInWithEmailAndPasswordAsync(emailInput.text, passwordInput.text).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
                statusText.text = "Login Failed!";
            else
            {
                statusText.text = "Login Successful!";
                FirebaseUser loggedInUser = auth.CurrentUser;
                Debug.Log("loggedInUser added ");
                GetNicknameFromFirebase(loggedInUser.UserId);
                Debug.Log("got nickname ");
                Debug.Log("lobby opened");
                loggin.Invoke();


            }
        }, TaskScheduler.FromCurrentSynchronizationContext());


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
            }
            else if (task.Result.Exists)
            {
                string nickname = task.Result.Value.ToString();
                PhotonConnect(nickname);
            }
        });
    }
    private void PhotonConnect(string nickname)
    {
        PhotonNetwork.NickName = nickname;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void SetNickname(GameObject player)
    {
        
        player.GetComponent<PlayerIGN>().SetNickname(PhotonNetwork.NickName);
    }
}
