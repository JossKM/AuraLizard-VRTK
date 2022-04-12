//This code and dll were found thanks to https://forum.unity.com/threads/openfiledialog-in-runtime.459474/

using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]

public class OpenFileName
{
    public OpenFileName()
    {
        structSize = Marshal.SizeOf(this);
        filter = "All Files\0*.*\0\0";
        file = new string(new char[256]);
        maxFile = file.Length;
        fileTitle = new string(new char[64]);
        maxFileTitle = fileTitle.Length;
        initialDir = UnityEngine.Application.dataPath;
        title = "Open File";
        defExt = "";
        flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
    }

    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public String file = null;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}

public static class OpenFileDialog
{
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
    
    public static bool OpenFile([In, Out] OpenFileName ofn)
    {
        return GetOpenFileName(ofn);
    }
}