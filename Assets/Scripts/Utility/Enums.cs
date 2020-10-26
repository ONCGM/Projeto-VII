using System;

namespace Utility {
    /// <summary>
    /// Defines the paths that the system can serialize files to.
    /// </summary>
    public enum SavePath {
        PersistentPath,
        ApplicationPath,
        Documents
    }

    /// <summary>
    /// Defines the format in which to save files as.
    /// </summary>
    public enum SaveFormat {
        Binary,
        Json
    }
}