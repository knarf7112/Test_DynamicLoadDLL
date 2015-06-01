using System;
using System.ComponentModel;
//
using System.Runtime.InteropServices;

namespace Test_Native_And_COM_Interoperability
{
    public sealed class ShareMem : IDisposable
    {
        // 檔案保護 Here we're using enums because they're safer than constants
        enum FileProtection : uint // constants from winnt.h
        {
            ReadOnly = 2,
            ReadWrite = 4
        }
        // 檔案權限
        enum FileRights : uint // constants from WinBASE.h
        {
            Read = 4,
            Write = 2,
            ReadWrite = Read + Write
        }

        static readonly IntPtr NoFileHandle = new IntPtr(-1);

        [DllImport("kernel32.dll",SetLastError = true)]
        static extern IntPtr CreateFileMapping(IntPtr hFile,
                                               int lpAttributes,
                                               FileProtection flProtec,
                                               uint dwMaximumSizeHigh,
                                               uint dwMaximumSizeLow,
                                               string lpName);

        [DllImport("kernel32.dll",SetLastError=true)]
        static extern IntPtr OpenFileMapping(FileRights dwDesiredAccess, bool bInheritHandle, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, FileRights dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);
        
        [DllImport("kernel32.dll")]
        static extern bool UnmapViewOfFile(IntPtr map);

        [DllImport("kernel32.dll")]
        static extern int CloseHandle(IntPtr hObject);

        IntPtr fileHandle, fileMap;

        public IntPtr Root { get { return fileMap; } }

        public ShareMem(string name, bool existing, uint sizeInBytes)
        {
            if (existing)
            {
                this.fileHandle = OpenFileMapping(FileRights.ReadWrite, false, name);
            }
            else
            {
                this.fileHandle = CreateFileMapping(NoFileHandle, 0, FileProtection.ReadWrite, 0, sizeInBytes, name);
            }

            if (this.fileHandle == IntPtr.Zero)
            {
                int i = Marshal.GetLastWin32Error();//當SetLastError=true有設定,則可取得DLL拋出的錯誤代碼
                throw new Win32Exception(i);
            }

            this.fileMap = MapViewOfFile(this.fileHandle, FileRights.ReadWrite, 0, 0, 0);
            if (fileMap == IntPtr.Zero)
            {
                int i = Marshal.GetLastWin32Error();
                throw new Win32Exception(i);
            }
        }

        public void Dispose()
        {
            if (this.fileMap != IntPtr.Zero)
                UnmapViewOfFile(fileMap);
            if (this.fileHandle != IntPtr.Zero)
                CloseHandle(fileHandle);

            this.fileMap = this.fileHandle = IntPtr.Zero;
        }
    }
}
