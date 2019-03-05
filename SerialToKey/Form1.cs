using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SerialToKey
{
 
  public partial class Form1 : Form
  {


    #region SerialPort

    SerialPort Serial_Port = new System.IO.Ports.SerialPort();

    //串口标志位
    private bool isSerialPortFalg = false;
    public bool IsSerialPortFalg
    {
      get { return isSerialPortFalg; }
      set { isSerialPortFalg = value; }
    }
    //串口名
    private string serialPortName = "COM1";
    public string SerialPortNmae
    {
      get { return serialPortName; }
      set { serialPortName = value; }
    }
    //波特率
    private int serialPort = 9600;
    public int SerialPort
    {
      get { return serialPort; }
      set { serialPort = value; }
    }

    //发送数据Buffer
    byte[] Serial_Buffer = new byte[1024];
    /// <summary>
    /// 初始化打开COM口
    /// </summary>
    public void RS232_Init()
    {
      try
      {

        Serial_Port.PortName = serialPortName;                         //连接端口名
        Serial_Port.BaudRate = serialPort;                             //串口波特率
        Serial_Port.ReadTimeout = 10000000;
        Serial_Port.StopBits = System.IO.Ports.StopBits.One;            //停止位
        Serial_Port.Parity = System.IO.Ports.Parity.None;               //奇偶校验
        Serial_Port.DataBits = 8;                                       //数据位

        if (Serial_Port.IsOpen)
        {
          Serial_Port.Close();
        }

        Serial_Port.Open();
        Serial_Port.DiscardInBuffer();                                  //清空缓存
        Serial_Port.DiscardOutBuffer();                                 //清空通道
        isSerialPortFalg = true;                                        //位开启
        button1.Invoke((EventHandler)delegate { button1.Enabled = false; });
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
    }

    /// <summary>
    /// 发送字符串
    /// </summary>
    /// <param name="outStr"></param>
    private void RS232_WriteBuffer(string outStr)
    {
      try
      {
        if (isSerialPortFalg)
        {
          Application.DoEvents();
          Serial_Buffer = System.Text.Encoding.ASCII.GetBytes(outStr);            //将字符串转换为位
          Serial_Port.Write(Serial_Buffer, 0, 0);                                 //发送位
        }
        else
          MessageBox.Show("串口未连接");
      }
      catch (Exception ex) { MessageBox.Show(ex.ToString()); }
    }

    /// <summary>
    /// 接收RS232数据
    /// </summary>
    /// <param name="Receive"></param>x
    private void RS232_ReadBuffer(out string Receive)
    {

      try
      {
        if (isSerialPortFalg)
        {
          int count = Serial_Port.BytesToRead;
          Byte[] BufferData = new Byte[count];
          if (isSerialPortFalg)
          {
            Receive = null;
            Serial_Port.Read(BufferData, 0, count);
            Receive += Encoding.UTF8.GetString(BufferData);
          }
          else
            Receive = "串口未连接";
        }
        else
        {
          Receive = "串口未连接";
        }
      }
      catch (Exception ex) {  Receive = "Error"; }
    }

    #endregion

    #region InitSerialPortThread

    //原子锁
    private object threadInitSerialPortLock = new object();

    //线程运行完毕标志位
    private bool isInitSerialPortThreadFinsh = false;
    public bool IsInitSerialPortThreadFinsh
    {
      get { return isInitSerialPortThreadFinsh; }
      set { isInitSerialPortThreadFinsh = value; }
    }
    //工作线程
    private static Thread workThreadInitSerialPort;
    //线程触发信号
    private AutoResetEvent eventSignalInitSerialPort = new AutoResetEvent(false);
    //连接线程
    private void RunInitSerialPortThread()
    {
      try
      {
        do
        {
          lock (threadInitSerialPortLock)
          {
            isInitSerialPortThreadFinsh = true;
            RS232_Init();
            isInitSerialPortThreadFinsh = false;
          }
          eventSignalInitSerialPort.WaitOne();
        } while (true);

      }
      catch (Exception ex)
      {

        throw;
      }
    }
    #endregion

    #region SendMessageThread
    //原子锁
    private object threadSendMessageLock = new object();

    //线程运行完毕标志位
    private bool isSendMessageThreadFinsh = false;
    public bool IsSendMessageThreadFinsh
    {
      get { return isSendMessageThreadFinsh; }
      set { isSendMessageThreadFinsh = value; }
    }
    //工作线程
    private static Thread workThreadSendMessage;
    //线程触发信号
    private AutoResetEvent eventSignalSendMessage = new AutoResetEvent(false);
    //连接线程
    private void RunSendMessageThread()
    {
      try
      {
        do
        {
          //跨线程获取控件内容
          string serial = null;
          textBox5.Invoke(new MethodInvoker(() => { serial = textBox5.Text; }));
          //------------------
          lock (threadSendMessageLock)
          {
            isSendMessageThreadFinsh = true;
            RS232_WriteBuffer(serial);
            isSendMessageThreadFinsh = false;
          }
          eventSignalSendMessage.WaitOne();
        } while (true);

      }
      catch (Exception ex)
      {

        throw;
      }
    }
    #endregion

    #region ReceiveMessage
    //原子锁
    private object threadReceiveMessageLock = new object();
    //运行完毕标志位
    private bool isReceiveMessageThreadFinish = false;
    public bool IsReceiveMessageThreadFinish
    {
      get { return isReceiveMessageThreadFinish; }
      set { isReceiveMessageThreadFinish = value; }
    }
    //工作线程
    private static Thread workThreadReceiveMessage;
    //返回的字符串
    private string outMessage = null;
    //待处理字符串
    private string pendMessage = null;
    //激活信号
    private ManualResetEvent eventReceiveThreadSingle = new ManualResetEvent(false);
    private void RunReceiveThread()
    {
      try
      {

        do
        {
          lock (threadReceiveMessageLock)
          {
            if(isSerialPortFalg)
            {
              isReceiveMessageThreadFinish = true;
              RS232_ReadBuffer(out outMessage);
              outMessage = outMessage.Replace("\0", "");
              if (outMessage != null && outMessage != "")
              {
                textBox4.Invoke((EventHandler)delegate { textBox4.Text = outMessage; });
                pendMessage = outMessage;
                eventComToKey.Set();
              }
              isReceiveMessageThreadFinish = false;
            }
          }
          eventReceiveThreadSingle.WaitOne();
        } while (true);
      }
      catch (Exception ex)
      {
        
      }
    }
    #endregion

    #region ParameStream

    private int startBit = 0;
    public int StartBit
    {
      get { return startBit; }
      set { startBit = value; }
    }
    private int endBit = 0;
    public int EndBit
    {
      get { return endBit; }
      set { endBit = value; }
    }

   

    //Write Lock
    private object saveLock = new object();
    private object readLock = new object();
    private void SaveParame(string filename)
    {
      XmlTextWriter Save = new XmlTextWriter(filename, null);
      Save.Formatting = Formatting.Indented;
      Save.WriteStartDocument();
      try
      {
        lock (saveLock)
        {
          Save.WriteComment("ParameterRecord");
          Save.WriteStartElement("MachineParameter");
          Save.WriteElementString("波特率", serialPort.ToString());
          Save.WriteElementString("端口号", serialPortName);
          Save.WriteElementString("起始位", startBit.ToString());
          Save.WriteElementString("结束位", endBit.ToString());

        }
      }
      catch (Exception)
      {

        throw;
      }
      finally
      {
        Save.WriteEndElement();
        Save.WriteEndDocument();
        Save.Close();
        Save.Dispose();
      }
    }
    private void ReadParame(string filename)
    {
      try
      {
        XmlDocument Read = new XmlDocument();
        Read.Load(filename);
        try
        {
          lock (readLock)
          {
            if (Read.DocumentElement != null)
            {
              foreach (XmlNode Node in Read.DocumentElement.ChildNodes)
              {
                if (Node.LocalName == "波特率" && Node.HasChildNodes)
                  serialPort = Convert.ToInt32(Node.FirstChild.Value);
                if (Node.LocalName == "端口号" && Node.HasChildNodes)
                  serialPortName = Convert.ToString(Node.FirstChild.Value);
                if (Node.LocalName == "起始位" && Node.HasChildNodes)
                  startBit = Convert.ToInt32(Node.FirstChild.Value);
                if (Node.LocalName == "结束位" && Node.HasChildNodes)
                  endBit = Convert.ToInt32(Node.FirstChild.Value);
              }
            }
          }
        }
        catch (Exception)
        {

          throw;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }

    }
    #endregion

    #region ComToKey
    //激活信号
    private AutoResetEvent eventComToKey = new AutoResetEvent(false);
    //工作线程
    private Thread workComToKey;
    //原子锁
    private object lockComToKey = new object();
    //键盘发送类
    KeyBroke sendKey = new KeyBroke();
    //执行方法
    private void ComToKey(string message)
    {
      try
      {
        if (message!=null&&message!=""&&message!="串口未连接")
        {
          int count = message.Length;
          int temp = startBit;
          if (startBit==0)
            temp = 1;
          string key = "";
          if (count >= endBit)
          {
            for (int i = temp - 1; i < endBit; i++)
            {
              key += message[i];
            }
            sendKey.SendBarcode(key);
          }
        }
      }
      catch ( Exception ex)
      {
        //MessageBox.Show(ex.ToString());
      }
    }
    //工作线程
    private void RunComToKey()
    {
      try
      {
        do
        {
          lock (lockComToKey)
          {
            ComToKey(pendMessage);
          }
          eventComToKey.WaitOne();
        } while (true);

      }
      catch ( Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
    }
    #endregion

    public Form1()
    {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      try
      {
        ReadParame(Application.StartupPath + "\\系统配置.xml");
        comboBox1.SelectedItem = serialPortName;
        textBox1.Text = serialPort.ToString();
        textBox2.Text = startBit.ToString();
        textBox3.Text = endBit.ToString();
        //创建串口转键盘线程
        if (workComToKey == null)
        {
          workComToKey = new Thread(new ThreadStart(RunComToKey));
          workComToKey.IsBackground = true;
          workComToKey.Name = "RunComToKey";
          workComToKey.Priority = ThreadPriority.Normal;
          workComToKey.Start();
        }
        //创建连接线程
        if (workThreadInitSerialPort == null)
        {
          workThreadInitSerialPort = new Thread(new ThreadStart(RunInitSerialPortThread));
          workThreadInitSerialPort.IsBackground = true;
          workThreadInitSerialPort.Name = "InitSerialPort";
          workThreadInitSerialPort.Priority = ThreadPriority.Normal;
          workThreadInitSerialPort.Start();
        }
        else
          eventSignalInitSerialPort.Set();

        //创建接收线程
        if (workThreadReceiveMessage == null)
        {
          workThreadReceiveMessage = new Thread(new ThreadStart(RunReceiveThread));
          workThreadReceiveMessage.Name = "ReceiveMessage";
          workThreadReceiveMessage.IsBackground = true;
          workThreadReceiveMessage.Priority = ThreadPriority.Normal;
          workThreadReceiveMessage.Start();
          eventReceiveThreadSingle.Set();
        }
      }
      catch (Exception)
      {

        throw;
      }
    }
 
    private void button1_Click(object sender, EventArgs e)
    {
      try
      {
        if (workThreadReceiveMessage != null)
          eventReceiveThreadSingle.Set();
        
        if (workThreadInitSerialPort == null)
        {
          workThreadInitSerialPort = new Thread(new ThreadStart(RunInitSerialPortThread));
          workThreadInitSerialPort.IsBackground = true;
          workThreadInitSerialPort.Name = "InitSerialPort";
          workThreadInitSerialPort.Priority = ThreadPriority.Normal;
          workThreadInitSerialPort.Start();
        }
        else
          eventSignalInitSerialPort.Set();
        button4.Enabled = true;
      }
      catch (Exception)
      {

        throw;
      }
    }
    private void button2_Click(object sender, EventArgs e)
    {
      try
      {
        if (workThreadSendMessage == null)
        {
          workThreadSendMessage = new Thread(new ThreadStart(RunSendMessageThread));
          workThreadSendMessage.IsBackground = true;
          workThreadSendMessage.Name = "SendMessage";
          workThreadSendMessage.Priority = ThreadPriority.Normal;
          workThreadSendMessage.Start();
        }
        else
          eventSignalSendMessage.Set();
      }
      catch (Exception)
      {

        throw;
      }
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      try
      {
        serialPortName = Convert.ToString(comboBox1.SelectedItem);
       
      }
      catch (Exception)
      {

        throw;
      }
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
      serialPort = Convert.ToInt32(textBox1.Text);
    }

    private void textBox2_TextChanged(object sender, EventArgs e)
    {
      try
      {
        startBit = Convert.ToInt32(textBox2.Text);
      }
      catch (Exception)
      {
        
      }
    }

    private void textBox3_TextChanged(object sender, EventArgs e)
    {
      try
      {
        endBit = Convert.ToInt32(textBox3.Text);
      }
      catch (Exception)
      {
        
      }
    }
    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
      try
      {
        SaveParame(Application.StartupPath + "\\系统配置.xml");

        //KillAllProcess
        System.Diagnostics.Process[] objProcess = System.Diagnostics.Process.GetProcessesByName("EHolly");
        if (objProcess != null)
        {
          for (int i = 0; i < objProcess.Length; i++)
          {
            if (objProcess[i].Id != System.Diagnostics.Process.GetCurrentProcess().Id)
            {
              objProcess[i].Kill();
            }
          }
          System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        this.Dispose(true);
        this.Close();
        Application.Exit();
      }
      catch (Exception)
      {
      }
    }

    private void button4_Click(object sender, EventArgs e)
    {
      try
      {
        eventReceiveThreadSingle.Reset();
        Serial_Port.Close();
        isSerialPortFalg = false;
        button1.Enabled = true;
        button4.Enabled = false;
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
    }

    private void toolStripMenuItem1_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show("你确定要退出程序吗？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK)
      {
        notifyIcon1.Visible = false;
        this.Close();
        this.Dispose();
        Application.Exit();
      }
    }

    private void toolStripMenuItem2_Click(object sender, EventArgs e)
    {
      this.Hide();
    }

    private void toolStripMenuItem3_Click(object sender, EventArgs e)
    {
      this.Show();
      this.WindowState = FormWindowState.Normal;
      this.Activate();
    }

   
  }
}
