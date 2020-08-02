using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public class WindowSwitchingHook
{

    #region DLLs

    private const uint WINEVENT_OUTOFCONTEXT = 0;
    private const uint EVENT_SYSTEM_FOREGROUND = 3;

    [DllImport("user32.dll")]
    static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool UnhookWinEvent(IntPtr hhk);
    #endregion

    /// <summary>
    /// Internal callback processing function
    /// </summary>
    private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
    private WinEventDelegate dele;

    /// <summary>
    /// Hook ID
    /// </summary>
    private IntPtr hookID = IntPtr.Zero;


    /// <summary>
    /// Function that will be called when defined events occur
    /// </summary>
    public delegate void Callback(WindowSwitchEventArgs e);

    public event Callback Window;

    /// <summary>
    /// フィールドにキャッシュしておく
    /// </summary>
    static readonly WindowSwitchEventArgs windowArgs = new WindowSwitchEventArgs();

    /// <summary>
    /// Default hook call, Pass a class to analyze the foreground window
    /// <br></br>
    /// デフォルトのフックコール、WindowSwitchEventArgsを渡したいだけです
    /// </summary>
    private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        Window(windowArgs);
    }

    /// <summary>
    /// Install hook
    /// <br></br>
    /// フック開始
    /// </summary>
    public void Install()
    {
        dele = new WinEventDelegate(WinEventProc);
        hookID = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);
    }

    /// <summary>
    /// Remove hook
    /// <br></br>
    /// フック解除
    /// </summary>
    public void Uninstall()
    {
        UnhookWinEvent(hookID);
        //hookID = IntPtr.Zero;
    }

    /// <summary>
    /// Destructor. Unhook current hook
    /// <br></br>
    /// デストラクターされたらフック解除
    /// </summary>
    ~WindowSwitchingHook()
    {
        Uninstall();
    }

}

/// <summary>
/// It will help the acquisition of the property of the foreground window
/// </summary>

public class WindowSwitchEventArgs : EventArgs
{
    #region DLLs

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
    #endregion

    /// <summary>
    /// Returns the process name of the foreground window
    /// <br></br>
    /// アクティブウィンドウのプロセスネームを返します
    /// </summary>
    /// <returns></returns>
    public string GetProcessName()
    {
        int x = GetProcessID();
        Process process = Process.GetProcessById(x);
        return process.ProcessName;
    }

    /// <summary>
    /// Returns the title of the foreground window
    /// <br></br>
    /// アクティブウィンドウのタイトル返します
    /// </summary>
    /// <returns></returns>
    public string GetTitle()
    {
        const int nChars = 256;
        StringBuilder Buff = new StringBuilder(nChars);
        IntPtr hWnd = GetForegroundWindow();

        if (GetWindowText(hWnd, Buff, nChars) > 0)
        {
            return Buff.ToString();
        }
        return null;
    }

    /// <summary>
    /// Returns the process ID of the foreground window
    /// <br></br>
    /// アクティブウィンドウのプロセスIDを返します
    /// </summary>
    /// <returns></returns>
    public int GetProcessID()
    {
        int i;
        IntPtr hWnd = GetForegroundWindow();
        GetWindowThreadProcessId(hWnd, out i);
        return i;
    }
}
