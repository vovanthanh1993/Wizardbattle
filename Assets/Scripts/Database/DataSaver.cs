using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Database;

[Serializable]
public class PlayerDataSaver {
    public string userName;
    public int totalCoins;
    public int currentLevel;
    public int xp;
    public int highScore;

}

public class DataSaver : MonoBehaviour
{
    [SerializeField] private PlayerDataSaver _playerData;
    [SerializeField] private string _userId;
    private DatabaseReference _dbRef;

    private void Awake()
    {
        _dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void SaveDataFn() 
    {
        string json = JsonUtility.ToJson(_playerData);
        _dbRef.Child("users").Child(_userId).SetRawJsonValueAsync(json);
    }

    public void LoadDataFn()
    {
        StartCoroutine(LoadDataEnum());
    }

    IEnumerator LoadDataEnum() 
    {
        var serverData = _dbRef.Child("users").Child(_userId).GetValueAsync();
        yield return new WaitUntil(predicate: () => serverData.IsCompleted);

        print("process is complete");

        DataSnapshot snapshot = serverData.Result;
        string jsonData = snapshot.GetRawJsonValue();

        if (jsonData != null)
        {
            print("server data found");

            _playerData = JsonUtility.FromJson<PlayerDataSaver>(jsonData);
        }
        else {
            print("no data found");
        }
    
    }
}