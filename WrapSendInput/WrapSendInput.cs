using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class WrapSendInput
    {
    #region const
    const int INPUT_MOUSE = 0;
    const int INPUT_KEYBOARD = 1;
    const int INPUT_HARDWARE = 2;
    const uint KEYEVENTF_KEYDOWN = 0x0000;
    const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    const uint KEYEVENTF_KEYUP = 0x0002;
    const uint KEYEVENTF_UNICODE = 0x0004;
    const uint KEYEVENTF_SCANCODE = 0x0008;
    #endregion
    private class INPUT
    {
        public int type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private class InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;
        [FieldOffset(0)]
        public KEYBDINPUT ki;
        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private class MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private class KEYBDINPUT
    {
        /*Virtual Key code.  Must be from 1-254.  If the dwFlags member specifies KEYEVENTF_UNICODE, wVk must be 0.*/
        public ushort wVk;
        /*A hardware scan code for the key. If dwFlags specifies KEYEVENTF_UNICODE, wScan specifies a Unicode character which is to be sent to the foreground application.*/
        public ushort wScan;
        /*Specifies various aspects of a keystroke.  See the KEYEVENTF_ constants for more information.*/
        public uint dwFlags;
        /*The time stamp for the event, in milliseconds. If this parameter is zero, the system will provide its own time stamp.*/
        public uint time;
        /*An additional value associated with the keystroke. Use the GetMessageExtraInfo function to obtain this information.*/
        public UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private class HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    #region Win32API
    [DllImport("user32.dll")]
    static extern IntPtr GetMessageExtraInfo();

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    #endregion

    INPUT[] keydown = null;
    INPUT[] keyup = null;
    INPUT[] rabit = null;

    public WrapSendInput(byte[] vBk,bool isExtend =false,uint dwExtraInfo = 0x0)
    {
        UIntPtr extraInfo = (UIntPtr)dwExtraInfo;
        int i = vBk.Length;
        keydown = new INPUT[i];
        for (int l = 0; l < i; l++)
        {
            keydown[l] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vBk[l],
                        wScan = 0,
                        dwFlags = ((isExtend) ? (KEYEVENTF_EXTENDEDKEY) : 0x0 | KEYEVENTF_KEYDOWN),
                        dwExtraInfo = extraInfo,
                    }
                }
            };
        }
        keyup = new INPUT[i];
        for (int l = 0; l < i; l++)
        {
            keyup[l] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vBk[l],
                        wScan = 0,
                        dwFlags = ((isExtend) ? (KEYEVENTF_EXTENDEDKEY) : 0x0 | KEYEVENTF_KEYUP),
                        dwExtraInfo = extraInfo,
                    }
                }
            };
        }
        rabit = new INPUT[i * 2];
        for (int l = 0; l < i * 2; l++)
        {
            rabit[l] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vBk[l],
                        wScan = 0,
                        dwFlags = ((isExtend) ? (KEYEVENTF_EXTENDEDKEY) : 0x0 | KEYEVENTF_KEYDOWN),
                        dwExtraInfo = extraInfo,
                    }
                }
            };
            l++;
            rabit[l] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vBk[l],
                        wScan = 0,
                        dwFlags = ((isExtend) ? (KEYEVENTF_EXTENDEDKEY) : 0x0 | KEYEVENTF_KEYUP),
                        dwExtraInfo = extraInfo,
                    }
                }
            };
        }
    }

    public void KeyDown()
    {
        SendInput((uint)keydown.Length, keydown, Marshal.SizeOf(typeof(INPUT))); ;
    }
    public void KeyUp()
    {
        SendInput((uint)keyup.Length, keyup, Marshal.SizeOf(typeof(INPUT))); ;
    }

    public int sleepTime;
    static bool allLoopFlag = true;
    bool loopFlag = false;
    bool toggle = false;

    public void RabitFile(int sleeptime, bool toggle = false)
    {
        if (loopFlag) return;
        if (sleeptime <= 15) sleeptime = 16;
        this.sleepTime = sleeptime;

        allLoopFlag = true;
        this.toggle = toggle;
        using Task<int> _ = Loop();
    }

    private async Task<int> Loop()
    {
        loopFlag = true;
        while (loopFlag && allLoopFlag)
        {
            SendInput((uint)rabit.Length, rabit, Marshal.SizeOf(typeof(INPUT))); ;
            await Task.Delay(sleepTime);
        }
        return 0;
    }

    public void LoopStop()
    {
        if (toggle)
        {
            toggle = false;

        }
        else
        {
            loopFlag = false;

        }
    }

    public static void AllLoopStop()
    {
        allLoopFlag = false;

    }
}

