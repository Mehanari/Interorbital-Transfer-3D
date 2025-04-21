using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Src
{
public class JsonIO<T> where T : new()
    {
        public JsonConverter[] Converters { get; set; }
        public string FileName { get; set; } = "data.json";

        private string DefaultPath()
        {
            return Path.Combine(
                Application.persistentDataPath,
                FileName
            );
        }
        
        public void Save(T data)
        {
            Save(data, DefaultPath());
        }
        
        public void Save(T data, string path)
        {
            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                string jsonString = JsonConvert.SerializeObject(data, Converters);
                
                File.WriteAllText(path, jsonString);
                Debug.Log("Saved data to: " + path);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving inventory: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Checks if a save file exists for a default path. 
        /// </summary>
        /// <returns></returns>
        public bool Exists()
        {
            return File.Exists(DefaultPath());
        }
        
        public T Load()
        {
            return Load(DefaultPath());
        }
        
        public T Load(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return new T();
                }
                
                string jsonString = File.ReadAllText(path);
                
                var construction = JsonConvert.DeserializeObject<T>(jsonString, Converters);
                
                return construction;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading inventory: {ex.Message}");
                throw;
            }
        }
	}
}