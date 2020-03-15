using System.IO;

namespace Compliance.Notifications.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class F
    {
        /// <summary>
        /// Append name to file name
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string AppendToFileName(this string fileName, string name)
        {
            return string.Concat(
                Path.GetFileNameWithoutExtension(fileName), 
                name, 
                Path.GetExtension(fileName));
        }
    }
}
