using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class GameReference
{
    int id;
    public DateTime startTime;
    public int moveCount;
    public float duration;
    public bool isWon;

    public GameReference(int id)
    {
        this.id = id;
        this.startTime = DateTime.Now;
    }

    private string FilePath
    {
        get { return Application.persistentDataPath + $"/game-{id}.save"; }
    }

    public Solitaire Load()
    {
        Debug.Log("Loading solitaire game from " + FilePath);
        Solitaire result = null;
        if (File.Exists(FilePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(FilePath, FileMode.Open);
            result = (Solitaire)bf.Deserialize(file);
            file.Close();
        }
        return result;
    }

    public void Save(Solitaire solitaire)
    {
        Debug.Log("Saving solitaire game to " + FilePath);
        FileStream file = File.Create(FilePath);
        new BinaryFormatter().Serialize(file, solitaire);
        file.Close();
    }
}

[Serializable]
public class GameHistoryManager
{
    private const string _filePath = "/history.save";
    private static GameHistoryManager _instance;
    public static GameHistoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                if (File.Exists(Application.persistentDataPath + _filePath))
                {
                    Debug.Log("Loading GameHistoryManager from " + Application.persistentDataPath + _filePath);
                    FileStream file = File.Open(Application.persistentDataPath + _filePath, FileMode.Open);
                    _instance = (GameHistoryManager)new BinaryFormatter().Deserialize(file);
                    file.Close();
                }
                else
                {
                    _instance = new GameHistoryManager();
                }
            }
            return _instance;
        }
    }

    public List<GameReference> gameHistoryIndex = new List<GameReference>();

    private void SaveToFile()
    {
        Debug.Log("Saving GameHistoryManager to " + Application.persistentDataPath + _filePath);
        FileStream file = File.Create(Application.persistentDataPath + _filePath);
        new BinaryFormatter().Serialize(file, this);
        file.Close();
    }

    public GameReference CreateGame()
    {
        var gameRef = new GameReference(gameHistoryIndex.Count);
        gameHistoryIndex.Add(gameRef);
        SaveToFile();
        return gameRef;
    }

    public GameReference GetMostRecentGame()
    {
        if (gameHistoryIndex.Count > 0)
        {
            return gameHistoryIndex[gameHistoryIndex.Count - 1];
        }
        return null;
    }

}
