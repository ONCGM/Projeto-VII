using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Game;
using JetBrains.Annotations;
using UnityEngine;

namespace Utility {
    public static class SaveSystem {
        /// <summary>
        /// The active save data. Use this to modify the values you need, then save it.
        /// </summary>
        public static SaveData LoadedData { get; set; }
        /// <summary>
        /// Information of the current loaded save date file.
        /// </summary>
        public static FileInfo LoadedDataInfo { get; private set; }
        /// <summary>
        /// Holds information on all of the game saves.
        /// </summary>
        public static FileInfo GameSaveInfo { get; private set; }
        /// <summary>
        /// Holds information on all of the auto saves.
        /// </summary>
        public static FileInfo AutoSaveInfo { get; private set; }
        /// <summary>
        /// Holds data for all of the save files found.
        /// </summary>
        public static SaveData GameSave { get; private set; } = new SaveData();
        /// <summary>
        /// Holds data for all of the auto save files found.
        /// </summary>
        public static SaveData AutoSaveData { get; private set; } = new SaveData();
        
        /// <summary>
        /// The active directory.
        /// </summary>
        private static DirectoryInfo directory;

        /// <summary>
        /// The save settings to be used.
        /// </summary>
        private static SaveSystemSettings settings;
        
        /// <summary>
        /// Path to such settings.
        /// </summary>
        private static string saveSystemSettingsFilePath = "Scriptables/Settings/SaveSystemSettings";

        /// <summary>
        /// Constant file extensions.
        /// </summary>
        private const string JsonFileExtension = ".json";

        static SaveSystem() {
            // Load settings.
            settings = Resources.Load<SaveSystemSettings>(saveSystemSettingsFilePath);
            
            // Find directory.
            directory = new DirectoryInfo(GetPathBasedOnSettings());
            
            // If it does not exist, create it.
            CreateDirectory();
            
            // Look for save files.
            SearchForSaveFiles();
            
            // Load the most recent into the active slot.
            LoadedData = LoadGameFile();
        }
        
        #region File and Directory checks
        /// <summary>
        /// Checks if the game folder exists.
        /// </summary>
        public static bool CheckForDirectory() {
            return directory.Exists;
        }
        
        /// <summary>
        /// Checks if the game folder exists, if it doesn't, it will create it.
        /// </summary>
        public static void CreateDirectory() {
           directory = new DirectoryInfo(GetPathBasedOnSettings());

            if(!directory.Exists) directory.Create();
        }
        
        /// <summary>
        /// Updates the game folder to be the same of the one in the settings file.
        /// </summary>
        public static void UpdateDirectory() {
            directory = new DirectoryInfo(GetPathBasedOnSettings());
            CreateDirectory();
        }
        
        /// <summary>
        /// Checks if any save files exist. Will return true if any files
        /// in any of the save location have one of the file extensions used by the system.
        /// This doesn't mean the file is readable, valid or even a save,
        /// it may just share the extension. 
        /// </summary>
        public static bool CheckForSaveFile() {
            return (from SavePath path in Enum.GetValues(typeof(SavePath))
                select new DirectoryInfo(GetPathToSaveLocation(path))
                into dir
                select dir.EnumerateFiles()
                          .Where(file => file.Extension == string.Concat(".", settings.fileExtension) || file.Extension == JsonFileExtension)
                          .OrderBy(file => file.LastAccessTime)).Any(fileQuery => fileQuery.Any(file => file.Exists));
        }

        /// <summary>
        /// Checks if a save file exists.
        /// </summary>
        public static bool CheckForSaveFile(string path) {
            return File.Exists(path);
        }

        /// <summary>
        /// Checks if a save file exists in the specified path,
        /// sing the save files directory in the search.
        /// </summary>
        /// <param name="path"> What path to search for the file? </param>
        /// <param name="name"> What is the name of the file to check for? </param>
        /// <returns></returns>
        public static bool CheckForSaveFile(SavePath path, string name) {
            switch(path) {
                case SavePath.PersistentPath:
                    return CheckForSaveFile(Path.Combine(Application.persistentDataPath, settings.directoryName, name));
                case SavePath.ApplicationPath:
                    return CheckForSaveFile(Path.Combine(Application.dataPath, settings.directoryName, name));
                case SavePath.Documents:
                    return CheckForSaveFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Application.companyName, Application.productName, settings.directoryName, name));
                default:
                    Debug.LogWarning("Couldn't find the specified file.");
                    return false;
            }
        }

        /// <summary>
        /// Deletes the currently loaded file.
        /// </summary>
        public static void DeleteLoadedFile() {
            try {
                File.Delete(LoadedDataInfo.FullName);
                LoadedData = null;
                LoadedDataInfo = null;
            } catch(Exception e) {
                Debug.LogError($"Something went wrong: {e}");
            }
        }
        
        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        public static void DeleteFile(string path) {
            try {
                File.Delete(path);
            } catch(Exception e) {
                Debug.LogError($"Something went wrong: {e}");
            }
        }
        
        #endregion

        #region File loading
        
        /// <summary>
        /// Searches for any save file in the known directories.
        /// </summary>
        public static void SearchForSaveFiles() {
            // Clear save lists.
            GameSaveInfo = null;
            GameSave = null;
            AutoSaveData = null;

            // If the selected directory does not exist, create it.
            CreateDirectory();
            
            // Query files by extension.
            // IEnumerable<FileInfo> fileQueryBinary = directory
            //                                   .EnumerateFiles()
            //                                   .Where(file => file.Extension == GetFileExtension())
            //                                   .OrderByDescending(file => file.LastWriteTime);
            IEnumerable<FileInfo> fileQueryJson = directory
                                              .EnumerateFiles()
                                              .Where(file => file.Extension == JsonFileExtension )
                                              .OrderByDescending(file => file.LastWriteTime);
            
            // Check if the files are readable, and if so, separate manual saves from auto saves.
            // foreach(var file in fileQueryBinary) {
            //     if(ReferenceEquals(DeserializeBinaryGameFile(file.Open(FileMode.Open)), null)) continue;
            //
            //     if(file.Name.Contains(settings.autoSavePrefix)) {
            //         AutoSaveInfo = file;
            //         AutoSaveData = DeserializeBinaryGameFile(file.Open(FileMode.Open));
            //         break;
            //     } else {
            //         GameSaveInfo = file;
            //         GameSave = DeserializeBinaryGameFile(file.Open(FileMode.Open));
            //         break;
            //     }
            // }
            
            // Check if the files are readable, and if so, separate manual saves from auto saves.
            foreach(var file in fileQueryJson) {
                if(ReferenceEquals(DeserializeJsonGameFile(File.ReadAllText(file.FullName)), null)) continue;
                
                if(file.Name.Contains(settings.autoSavePrefix)) {
                    AutoSaveInfo = file;
                    AutoSaveData = DeserializeJsonGameFile(File.ReadAllText(file.FullName));
                    break;
                } else {
                    GameSaveInfo = file;
                    GameSave = DeserializeJsonGameFile(File.ReadAllText(file.FullName));
                    break;
                }
            }
        }
        

        #endregion
        
        #region Serialization
        
        /// <summary>
        /// Saves the loaded data using the set methods in the save settings. (See Save System window to change the settings). 
        /// This method will overwrite the save file in which the loaded data was read from.
        /// <para>
        /// If loaded data is a null it will ignore the request,
        /// to create a new save use one of the overloads and pass a 'new SaveData()' as a parameter.
        /// </para>
        /// </summary>
        public static void SerializeToFile() {
            // Check if the file is valid.
            if(ReferenceEquals(LoadedData, null)) {
                Debug.LogWarning("The loaded file was a null or wasn't loaded. The file was not serialized");
                return;
            }

            // Tries to save the file based on the settings.
            try { 
                switch(settings.saveFormat) {
                    case SaveFormat.Binary:
                        OverwriteBinaryFile(LoadedData, LoadedDataInfo?? new FileInfo(Path.Combine(directory.FullName, 
                                                                                                    $"{settings.saveFileDefaultName}{GetFileExtension()}")));
                        break;
                    case SaveFormat.Json:
                        OverwriteJsonFile(LoadedData, LoadedDataInfo?? new FileInfo(Path.Combine(directory.FullName, 
                                                                                                 $"{settings.saveFileDefaultName}{JsonFileExtension}")));
                        break;
                    default:
                        Debug.LogWarning("Something went wrong. Probably the save settings were null or they weren't loaded.");
                        break;
                }
            } catch(Exception e) {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Save the specified data, if it receives a null it will ignore the request.
        /// </summary>
        public static void SerializeToFile(string name, [NotNull] SaveData data, SaveFormat format, string path) {
            // Check if the path is valid.
            if(!Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute)) {
                Debug.LogWarning("The path was a invalid. The file was not serialized");
                return;
            }
            
            // Tries to save the file.
            try { 
                switch(format) {
                    case SaveFormat.Binary:
                        SerializeBinaryFile(data, path, false, name);
                        break;
                    case SaveFormat.Json:
                        SerializeJsonFile(data, path, false, name);
                        break;
                    default:
                        Debug.LogWarning("Something went wrong. Probably the save settings were null or they weren't loaded.");
                        break;
                }
            } catch(Exception e) {
                Debug.LogError(e);
            }
        }

        #region Binary

        /// <summary>
        /// Creates a new file in the binary format in the specified path using the default settings in the save system.
        /// </summary>
        private static void SerializeBinaryFile([NotNull] SaveData data, string path, bool isAutoSave, string fileName = "") {
            // Checks for the directory.
            CreateDirectory();
            // Creates a formatter.
            var formatter = new BinaryFormatter();
            FileStream stream;
            
            // Checks if it is an auto save.
            if(!isAutoSave) {
                // If not, save it based on the parameters.
                stream = new FileStream(Path.Combine(path, string.Concat(string.IsNullOrEmpty(fileName) ? settings.saveFileDefaultName : fileName,
                                                                         GetFileExtension())), FileMode.Create);
            } else {
                // If it is, use auto save naming.
                var autoSavePath = Path.Combine(path, string.Concat(settings.autoSavePrefix, fileName, GetFileExtension()));
                
                stream = new FileStream(autoSavePath, FileMode.Create);
                
                // Checks if the file should be hidden.
                if(settings.hideAutoSave) File.SetAttributes(autoSavePath, FileAttributes.Hidden);
            }
            
            // Serializes the data and closes the stream.
            formatter.Serialize(stream, data);
            stream.Seek(0, SeekOrigin.Begin);
            stream.Close();
            
            SearchForSaveFiles();
        }
        
        /// <summary>
        /// Overwrites the save data on the specified file as a binary file.
        /// </summary>
        /// <param name="data"> The save data to be serialized. </param>
        /// <param name="info"> The information of the file to be overwritten. </param>
        private static void OverwriteBinaryFile([NotNull] SaveData data, FileSystemInfo info) {
            // Checks for the directory.
            CreateDirectory();
            
            // Creates the formatter.
            var formatter = new BinaryFormatter();
            var stream = new FileStream(info.FullName, FileMode.Create);
            
            // Serializes the file and closes the stream.
            formatter.Serialize(stream, data);
            stream.Seek(0, SeekOrigin.Begin);
            stream.Close();
            
            SearchForSaveFiles();
        }
        
        #endregion

        #region JSON

        /// <summary>
        /// Creates a new file in the JSON format in the specified path using the default settings in the save system.
        /// </summary>
        private static void SerializeJsonFile([NotNull] SaveData data, string path, bool isAutoSave, string fileName = "") {
            // Checks for the directory.
            CreateDirectory();
            
            // Converts the data to .Json.
            string jsonConversion = JsonUtility.ToJson(data, true);

            // Check if it is an auto save.
            if(!isAutoSave) {
                // Writes the data.
                File.WriteAllText(Path.Combine(path, string.Concat(string.IsNullOrEmpty(fileName) ? settings.saveFileDefaultName : fileName,
                                                                    JsonFileExtension)), jsonConversion);
                
            } else {
                var autoSavePath = Path.Combine(path, string.Concat(settings.autoSavePrefix, fileName, JsonFileExtension));
                
                // Writes the data.
                File.WriteAllText(autoSavePath, jsonConversion);
                
                // Hides the file if the user desires to.
                if(settings.hideAutoSave) File.SetAttributes(autoSavePath, FileAttributes.Hidden);
            }
            
            SearchForSaveFiles();
        }
        
        /// <summary>
        /// Overwrites the save data on the specified file as a JSON file.
        /// </summary>
        /// <param name="data"> The save data to be serialized. </param>
        /// <param name="info"> The information of the file to be overwritten. </param>
        private static void OverwriteJsonFile([NotNull] SaveData data, FileSystemInfo info) {
            // Checks for the directory.
            CreateDirectory();
            
            // Converts the date to .Json.
            string jsonConversion = JsonUtility.ToJson(data, true);
            
            // Writes the data.
            File.WriteAllText(info.FullName, jsonConversion);
            
            
            SearchForSaveFiles();
        }

        #endregion

        #endregion
        
        #region Deserialization

        /// <summary>
        /// Checks if there is a file to load, if it does not find one, it wil create a new one.
        /// <para>
        /// If it can't create a new one, it will return a null.
        /// </para>
        /// </summary>
        /// <returns></returns>
        public static SaveData LoadGameFile() {
            SearchForSaveFiles();
            GameMaster.OnSaveDataUpdated?.Invoke();
            // Checks if there are any known saves.
            if(GameSaveInfo == null && AutoSaveInfo == null) {
                Debug.Log("No saves or auto saves were found. Creating a new save file template.");
                LoadedData = new SaveData();
                SerializeToFile();
                return LoadedData;
            }
        
            // Tries to get a game and auto save file.
            // Loads the most recent.
            FileInfo saveFile;
            
            if(GameSaveInfo == null) {
                saveFile = AutoSaveInfo;
            } else if(AutoSaveInfo == null) {
                saveFile = GameSaveInfo;
            } else {
                saveFile = GameSaveInfo?.LastWriteTime > AutoSaveInfo?.LastWriteTime ? GameSaveInfo : AutoSaveInfo;
            }
            
            // Checks if it is valid.
            if(saveFile == null) {
                Debug.Log("No save was found. Creating a new save file template.");
                LoadedData = new SaveData();
                SerializeToFile();
                return LoadedData;
            }
            
            // Deserializes the data.
            SaveData data;
            LoadedDataInfo = saveFile;

            switch(settings.saveFormat) {
                case SaveFormat.Binary:
                    data = DeserializeBinaryGameFile(new FileStream(saveFile.FullName, FileMode.Open));
                    break;
                case SaveFormat.Json:
                    data = DeserializeJsonGameFile(File.ReadAllText(saveFile.FullName));
                    break;
                default:
                    data = null;
                    break;
            }
            
            // Checks if it is null.
            if(data != null) return data;
             
            // Return a new save data if it is a null.
            Debug.LogWarning("File found but couldn't be read as SaveData. Creating a new save file template.");
            LoadedData = new SaveData();
            SerializeToFile();
            return LoadedData;
        }
        
        /// <summary>
        /// Tries to load the specified save file. If it fails it will return a new SaveData.
        /// </summary>
        /// <param name="path"> Path to the file directory. </param>
        public static SaveData LoadGameFile(string path) {
            GameMaster.OnSaveDataUpdated?.Invoke();
            if(CheckForSaveFile(path)) {
                LoadedDataInfo = new FileInfo(path);
            } else {
                Debug.LogWarning("The file could not be found. creating a new save file.");
                LoadedData = new SaveData();
                SerializeToFile();
                return LoadedData;
            }

            if(LoadedDataInfo.Extension.Contains(settings.fileExtension)) {
                return DeserializeBinaryGameFile(new FileStream(path, FileMode.Open));
            }

            if(LoadedDataInfo.Extension.Contains(JsonFileExtension)) {
                return DeserializeJsonGameFile(File.ReadAllText(path));
            }
            
            Debug.LogWarning("Couldn't load the file. Returning new save data.");
            LoadedData = new SaveData();
            SerializeToFile();
            return LoadedData;
        }
        
        #region Binary

        /// <summary>
        /// Deserializes the specified binary file as save data.
        /// </summary>
        /// <param name="file"> The file to be deserialized. </param>
        private static SaveData DeserializeBinaryGameFile(FileStream file) {
            try {
                BinaryFormatter bf = new BinaryFormatter();
                SaveData data = bf.Deserialize(file) as SaveData;
                file.Seek(0, SeekOrigin.Begin);
                file.Close();
                return data;
            } catch(Exception e) {
                Debug.LogError($"Something went wrong: {e}");
                return null;
            }
        }
        
        #endregion

        #region JSON
        
        /// <summary>
        /// Deserializes the specified JSON string as save data.
        /// </summary>
        /// <param name="data"> The string to be deserialized. </param>
        private static SaveData DeserializeJsonGameFile(string data) {
            try {
                SaveData saveData = JsonUtility.FromJson<SaveData>(data);
                return saveData;
            } catch(Exception e) {
                Debug.LogError($"Something went wrong: {e}");
                return null;
            }
            
        }

        #endregion

        #endregion
        
        #region Auto Save
        
        /// <summary>
        /// Auto saves the current loaded data.
        /// </summary>
        public static void AutoSave() {
            if(ReferenceEquals(LoadedData, null)) {
                Debug.LogWarning("The loaded file was a null or wasn't loaded. The file was not serialized");
                return;
            }
            
            AutoSave(LoadedData);
        }

        /// <summary>
        /// Save the specified data as an auto save.
        /// </summary>
        /// <param name="data"> Data to be saved. </param>
        private static void AutoSave([NotNull] SaveData data) {
            try { 
                switch(settings.saveFormat) {
                    case SaveFormat.Binary:
                        SerializeBinaryFile(data, GetPathToSaveLocation(settings.saveLocation), true, settings.saveFileDefaultName);
                        break;
                    case SaveFormat.Json:
                        SerializeJsonFile(data, GetPathToSaveLocation(settings.saveLocation), true, settings.saveFileDefaultName);
                        break;
                    default:
                        Debug.LogWarning("Something went wrong. Probably the save settings were null or they weren't loaded.");
                        break;
                }
            } catch(Exception e) {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Loads the most recent auto save.
        /// </summary>
        /// <returns> Return the save data. Will return a new file if it can't load or find an auto save.</returns>
        public static SaveData LoadAutoSave() {
            var lastAutoSave = AutoSaveInfo;

            if(lastAutoSave.Extension.Contains(settings.fileExtension)) {
                if(!ReferenceEquals(DeserializeBinaryGameFile(new FileStream(lastAutoSave.FullName, FileMode.Open)), null)) {
                    return DeserializeBinaryGameFile(new FileStream(lastAutoSave.FullName, FileMode.Open));
                }

                Debug.LogWarning("Something went wrong, retuning new save data.");
                LoadedData = new SaveData();
                SerializeToFile();
                return LoadedData;
            } 
            
            if(lastAutoSave.Extension.Contains(JsonFileExtension)) {
                if(!ReferenceEquals(DeserializeJsonGameFile(File.ReadAllText(lastAutoSave.FullName)), null)) {
                    return DeserializeJsonGameFile(File.ReadAllText(lastAutoSave.FullName));
                }

                Debug.LogWarning("Something went wrong, retuning new save data.");
                LoadedData = new SaveData();
                SerializeToFile();
                return LoadedData;
            }

            Debug.LogWarning("Something went wrong, retuning new save data.");
            LoadedData = new SaveData();
            SerializeToFile();
            return LoadedData;
        }
        
        #endregion
        
        #region Save System Settings
        
        /// <summary>
        /// Toggles the auto save settings.
        /// </summary>
        /// <param name="hideAutoSaveFile"> Should the auto save be hidden.</param>
        public static void SetAutoSave(bool hideAutoSaveFile = false) {
            settings.hideAutoSave = hideAutoSaveFile;
        }

        /// <summary>
        /// Changes the save system settings.
        /// </summary>
        /// <param name="updatedSettings"> New settings to be used by the system. </param>
        public static void SetSaveSystemSettings(SaveSystemSettings updatedSettings) {
            settings = updatedSettings;
        }

        #endregion

        #region Miscellaneous
        
        /// <summary>
        /// Gets the current time and formats it into a savable format.
        /// </summary>
        private static string GetCurrentDateFormatted() {
            return $" {DateTime.Now:HH-mm-ss_MM-dd-yyyy}";
        }

        /// <summary>
        /// Adds the dot on the file extension if the user hasn't done so.
        /// </summary>
        private static string GetFileExtension() {
            return settings.fileExtension.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) == 0
                ? settings.fileExtension
                : string.Concat(".", settings.fileExtension);
        }
        
        /// <summary>
        /// Returns the path towards the save location selected in the settings.
        /// If for some reason it can't do so, it will return the Application.persistentDataPath instead.
        /// </summary>
        private static string GetPathBasedOnSettings() {
            switch(settings.saveLocation) {
                case SavePath.PersistentPath:
                    return Path.Combine(Application.persistentDataPath, settings.directoryName);
                case SavePath.ApplicationPath:
                    return Path.Combine(Application.dataPath, settings.directoryName);
                case SavePath.Documents:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), settings.directoryName);
                default:
                    Debug.LogWarning("Couldn't get the desired path to save location, using Application.persistentDataPath instead.");
                    return Path.Combine(Application.persistentDataPath, directory.Name);
            }
        } 
        
        /// <summary>
        /// Returns the path towards the save location selected in the settings.
        /// If for some reason it can't do so, it will return the Application.persistentDataPath instead.
        /// </summary>
        private static string GetPathToSaveLocation(SavePath location) {
            switch(location) {
                case SavePath.PersistentPath:
                    return Path.Combine(Application.persistentDataPath, settings.directoryName);
                case SavePath.ApplicationPath:
                    return Path.Combine(Application.dataPath, settings.directoryName);
                case SavePath.Documents:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), settings.directoryName);
                default:
                    Debug.LogWarning("Couldn't get the desired path to save location, using Application.persistentDataPath instead.");
                    return Path.Combine(Application.persistentDataPath, directory.Name);
            }
        } 
        
        #endregion
    }
}
