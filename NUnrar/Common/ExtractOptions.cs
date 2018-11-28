using System;

namespace NUnrar.Common
{
    [Flags]
    public enum ExtractOptions
    {
        None,
        /// <summary>
        /// overwrite target if it exists
        /// </summary>
        Overwrite,
        /// <summary>
        /// extract with internal directory structure
        /// </summary>
        ExtractFullPath,
    }
}
