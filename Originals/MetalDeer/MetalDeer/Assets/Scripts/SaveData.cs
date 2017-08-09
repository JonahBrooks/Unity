using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

[Serializable]
public class SaveData {

    public int EasyLevel;
    public int NormalLevel;
    public int HardLevel;

	public SaveData()
    {
        EasyLevel = 0;
        NormalLevel = 0;
        HardLevel = 0;
    }

    public static void Save(SaveData saveData)
    {
        string dataPath = string.Format("{0}/SaveGame.dat", Application.persistentDataPath);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream;
        try
        {
            if (File.Exists(dataPath))
            {
                File.WriteAllText(dataPath, string.Empty);
                fileStream = File.Open(dataPath, FileMode.Open);
            }
            else
            {
                fileStream = File.Create(dataPath);
            }

            binaryFormatter.Serialize(fileStream, saveData);
            fileStream.Close();

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Application.ExternalEval("_JS_FileSystem_Sync();");
            }
        }
        catch (Exception e)
        {
            Debug.Log("Failed to Save: " + e.Message);
        }
    }

    public static SaveData Load()
    {
        SaveData saveData = null;
        string dataPath = string.Format("{0}/SaveGame.dat", Application.persistentDataPath);

        try
        {
            if (File.Exists(dataPath))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream fileStream = File.Open(dataPath, FileMode.Open);

                saveData = (SaveData)binaryFormatter.Deserialize(fileStream);
                fileStream.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log("Failed to Load: " + e.Message);
        }

        return saveData;
    }
}
