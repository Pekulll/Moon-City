using System.IO;
using UnityEngine;

public static class JSONSaveSystem
{
    /**
     * <summary>Allow you to save a serializable object</summary>
     * <param name="filepath">The directory of the file</param>
     * <param name="filename">The name of the file which will contains the saved object</param>
     * <param name="obj">The object to save (need to be serializable)</param>
     * <returns>True if the save is a success</returns>
     */
    public static bool Save<T>(string filepath, string filename, T obj)
    {
        if (filename == "") {
            throw new System.Exception("Cannot save a file without a name.");
        } else if (obj == null) {
            throw new System.Exception("Cannot save \"null\" object.");
        } else if (!obj.GetType().IsSerializable)
        {
            throw new System.Exception(
                $"Cannot serialize non-serializable object. Please add \"[System.Serializable]\" before the signature of your class {obj.GetType()}");
        }

        if (!Directory.Exists(filepath))
        {
            Directory.CreateDirectory(filepath);
        }

        string path = filepath + "/" + filename;
        string jsonObj = JsonUtility.ToJson(obj, true);

        File.WriteAllText(path, jsonObj, System.Text.Encoding.UTF8);
        return true;
    } // Save<T>(...)

    /**
     * <summary>Allow you to load a file that contains a serializable object</summary>
     * <param name="filepath">The directory of the file</param>
     * <param name="filename">The name of the file which will contains the saved object</param>
     * <returns>The serializable object contains in the file specified</returns>
     */
    public static T Load<T>(string filepath, string filename)
    {
        if (!Directory.Exists(filepath))
        {
            Directory.CreateDirectory(filepath);
            throw new System.Exception($"The directory {filepath} doesn't exist. Can't load anything!");
        }

        string path = filepath + "/" + filename;

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        else
        {
            throw new System.Exception($"The file at: {path}, doesn't exist! Unable to load it.");
        }
    } // Load<T>(..)
}
