using System;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class WrapKeybd_event
{
    const int INPUT_MOUSE = 0;
    const int INPUT_KEYBOARD = 1;
    const int INPUT_HARDWARE = 2;
    const uint KEYEVENTF_KEYDOWN = 0x0;
    const uint KEYEVENTF_EXTENDEDKEY = 0x1;
    const uint KEYEVENTF_KEYUP = 0x2;
    const uint KEYEVENTF_UNICODE = 0x4;
    const uint KEYEVENTF_SCANCODE = 0x8;

    private byte sendKey;
    private uint ex = 0x0;
    private UIntPtr dwExtraInfo; //通常0でいい

    public byte SendKey => sendKey;

    public WrapKeybd_event(byte sendKey,bool isExtend = false, uint dwExtraInfo = 0x0)
    {
        if (1 <= sendKey && sendKey <= 254)
        {
            this.sendKey = sendKey;
        }
        else
        {
            this.sendKey = 0x7; //Undefined
        }

        if (isExtend) ex = KEYEVENTF_EXTENDEDKEY;
        this.dwExtraInfo = (UIntPtr)dwExtraInfo;
    }
    public WrapKeybd_event(char sendKey, bool isExtend = false, uint dwExtraInfo = 0x0)
    {       
        this.sendKey = (byte)sendKey;
        if (isExtend) ex = KEYEVENTF_EXTENDEDKEY;
        this.dwExtraInfo = (UIntPtr)dwExtraInfo;
    }
    ~WrapKeybd_event()
    {
        //KeyUp();
    }
    

    public void KeyDown()
    {
        keybd_event(sendKey, 0, (ex) | KEYEVENTF_KEYDOWN, dwExtraInfo);
    }

    public void KeyUp()
    {
        keybd_event(sendKey, 0, (ex) | KEYEVENTF_KEYUP, dwExtraInfo);
    }


    #region Repeated hits

    private int sleeptime;
    private static bool allLoopFlag;
    private bool loopFlag, toggle;
    public bool Toggle { get; set; }

    public int SleepTime
    {
        get => sleeptime;
        set
        {
            if (value == 0)
            {
                sleeptime = 0;
                return;
            }
            if (value <= 15) value = 16;
            sleeptime = value;
        }
    }

    public void StartLoop()
    {
        if (sleeptime == 0) return;

        if (loopFlag) return;
        toggle = Toggle;
        allLoopFlag = true;
        loopFlag = true;
        Task.Run(() => Loop());
        
    }

    private async Task Loop()
    {
        await Task.Delay(sleeptime);

        while (loopFlag && allLoopFlag)
        {
            KeyDown();
            await Task.Delay(2);
            KeyUp();
            await Task.Delay(sleeptime);
        }
    }

    public void StopLoop()
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

    public static void StopAllLoop()
    {
        allLoopFlag = false;        
    }
    #endregion 


    [DllImport ("user32.dll")]
    public static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
}