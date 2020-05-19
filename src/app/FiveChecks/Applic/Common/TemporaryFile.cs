using System;
using LanguageExt;
using Pri.LongPath;

namespace FiveChecks.Applic.Common
{
    public sealed class TemporaryFile : IDisposable
    {
        public FileInfo File { get; }

        public TemporaryFile(Some<string> fileExtension)
        {
            var tempFileName = System.IO.Path.GetTempFileName();
            var tempFileNameWithExtension = $"{tempFileName}.{fileExtension.Value.Trim('.')}";
            System.IO.File.Move(tempFileName,tempFileNameWithExtension);
            File = new FileInfo(tempFileNameWithExtension);
        }

        private void ReleaseUnmanagedResources()
        {
            if (File != null && System.IO.File.Exists(File.FullName))
            {
                System.IO.File.Delete(File.FullName);
            }
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~TemporaryFile()
        {
            ReleaseUnmanagedResources();
        }
    }
}