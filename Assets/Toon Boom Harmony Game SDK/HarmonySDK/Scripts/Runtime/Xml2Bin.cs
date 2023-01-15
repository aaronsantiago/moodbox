using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace ToonBoom.Harmony
{
    public static class Xml2Bin
    {
        [DllImport("Xml2Bin")]
        internal static extern int ConvertToFile(string projectPath, string bytesFilePath, IntPtr stdprint, IntPtr errprint);
        [DllImport("Xml2Bin")]
        internal static extern int ConvertToMemory(string projectPath, out IntPtr bytesBuffer, out IntPtr bytesBufferSize, IntPtr stdprint, IntPtr errprint);
        [DllImport("Xml2Bin")]
        internal static extern int FreeMemoryBuffer(IntPtr bytesBuffer, IntPtr stdprint, IntPtr errprint);

        public delegate int PrintDelegate(string message);

        public static byte[] ConvertToMemory(string projectPath)
        {
            StringBuilder stdOutputMerger = new StringBuilder();
            try
            {
                return ConvertToMemory(projectPath,
                    stdOutMsg => { stdOutputMerger.Append(stdOutMsg); return stdOutMsg.Length; },
                    stdErrMsg => { stdOutputMerger.Append(stdErrMsg); Debug.LogError(stdErrMsg); return stdErrMsg.Length; }
                    );
            }
            finally
            {
                Debug.Log("XML2Bin Output:\n" + stdOutputMerger.ToString());
            }
        }

        public static byte[] ConvertToMemory(string projectPath, PrintDelegate stdOut, PrintDelegate stdErr)
        {
            byte[] bytes = null;
            IntPtr bytesBuffer = IntPtr.Zero;
            IntPtr std = Marshal.GetFunctionPointerForDelegate(stdOut);
            IntPtr err = Marshal.GetFunctionPointerForDelegate(stdErr);
            try
            {
                ConvertToMemory(projectPath, out bytesBuffer, out IntPtr bytesSize, std, err);
                bytes = new byte[(int)bytesSize];
                Marshal.Copy(bytesBuffer, bytes, 0, (int)bytesSize);
            }
            finally
            {
                if (bytesBuffer != IntPtr.Zero)
                {
                    FreeMemoryBuffer(bytesBuffer, std, err);
                }
            }
            return bytes;
        }
    }
}