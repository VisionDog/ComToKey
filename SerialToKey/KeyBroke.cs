using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SerialToKey
{
  class KeyBroke
  {
    #region SendKeyToText
    [System.Runtime.InteropServices.DllImport("user32")]
    static extern void keybd_event(
          byte bVk,
          byte bScan,
          uint dwFlags,
          uint dwExtraInfo
          );
    const uint KEYEVENTF_EXTENDEDKEY = 0x1;
    const uint KEYEVENTF_KEYUP = 0x2;
    private static Dictionary<string, byte> keycode = new Dictionary<string, byte>();

    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    [DllImport("User32.DLL")]
    public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, string lParam);
    [DllImport("User32.DLL")]
    public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

    [DllImport("User32.DLL")]
    public static extern IntPtr FindWindowEx(IntPtr hwndParent,
        IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
    public const uint WM_SETTEXT = 0x000C;
    public const uint WM_CHAR = 0x0102;
    public static int WM_KEYDOWN = 0x0100;
    //释放一个键  
    public static int WM_KEYUP = 0x0101;

    public KeyBroke()
    {
      InitKey();
    }

    public void InitKey()
    {
      keycode = new Dictionary<string, byte>();
      keycode.Add("A", 65);
      keycode.Add("B", 66);
      keycode.Add("C", 67);
      keycode.Add("D", 68);
      keycode.Add("E", 69);
      keycode.Add("F", 70);
      keycode.Add("G", 71);
      keycode.Add("H", 72);
      keycode.Add("I", 73);
      keycode.Add("J", 74);
      keycode.Add("K", 75);
      keycode.Add("L", 76);
      keycode.Add("M", 77);
      keycode.Add("N", 78);
      keycode.Add("O", 79);
      keycode.Add("P", 80);
      keycode.Add("Q", 81);
      keycode.Add("R", 82);
      keycode.Add("S", 83);
      keycode.Add("T", 84);
      keycode.Add("U", 85);
      keycode.Add("V", 86);
      keycode.Add("W", 87);
      keycode.Add("X", 88);
      keycode.Add("Y", 89);
      keycode.Add("Z", 90);
      keycode.Add("0", 48);
      keycode.Add("1", 49);
      keycode.Add("2", 50);
      keycode.Add("3", 51);
      keycode.Add("4", 52);
      keycode.Add("5", 53);
      keycode.Add("6", 54);
      keycode.Add("7", 55);
      keycode.Add("8", 56);
      keycode.Add("9", 57);
      keycode.Add(".", 0x6E);
      keycode.Add("LEFT", 0x25);
      keycode.Add("UP", 0x26);
      keycode.Add("RIGHT", 0x27);
      keycode.Add("DOWN", 0x28);
      keycode.Add("-", 0x6D);
      keycode.Add("{ENTER}", 13);
    }

    public static void KeyBoardDo(string key)
    {
      keybd_event(keycode[key], 0x45, KEYEVENTF_EXTENDEDKEY | 0, 0);
      keybd_event(keycode[key], 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
    }


    /// <summary>  
    /// 使用keybd_event发送。 缺点是不能指定接受的窗口名称；接受窗口必须为当前活动窗口。  
    /// </summary>  
    /// <param name="barcode"></param>  
    public void SendBarcode(string barcode)
    {
      for (int k = 0; k < barcode.Length; k++)
      {
        string ak = barcode.Substring(k, 1);
        KeyBoardDo(ak);
      }
      //KeyBoardDo("{ENTER}");
    }


    /// <summary>  
    ///   
    /// </summary>  
    /// <param name="barcode"></param>  
    public void SendKeys(string barcode)
    {
      System.Windows.Forms.SendKeys.Send(barcode);
      System.Windows.Forms.SendKeys.Send("{ENTER}");
    }


    /// <summary>  
    ///可以指定窗口，并且窗口可以为不活动状态； 即应用程序不在显示器最上层。  
    /// </summary>  
    /// <param name="hello"></param>  
    public void SendKeyByMessage(string hello)
    {
      System.Diagnostics.Process[] GamesProcess = System.Diagnostics.Process.GetProcessesByName("notepad");
      if (GamesProcess.Length == 0) return;

      IntPtr hWnd = FindWindowEx(GamesProcess[0].MainWindowHandle,
      IntPtr.Zero, "Edit", null);

      //SendMessage(hWnd, WM_SETTEXT, 0, hello); //清空记事本中内容；写入字符；并且光标回到第一行第一个位置。  
      int key = 0;
      for (int k = 0; k < hello.Length; k++)
      {
        string ak = hello.Substring(k, 1);
        key = keycode[ak];
        SendMessage(hWnd, WM_CHAR, key, 0);
      }

      key = keycode["{ENTER}"];
      SendMessage(hWnd, WM_CHAR, key, 0);      //往记事本中添加内容。       
    }

  }
  #endregion
  
}
