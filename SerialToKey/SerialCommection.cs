using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Logger;

namespace SerialToKey
{
  class SerialCommection : ICommunication
  {
    //串口
    SerialPort Serial ;

    //串口连接成功标志位
    private bool isSerialPortFalg = false;
    //目标连接端口名
    private string destinationPortName = "";
    //目标连接端口
    private int destinationPort = 0;

    public SerialCommection()
    {
      Serial = new SerialPort();
    }

    public bool IsConnectionFalg
    {
      get { return isSerialPortFalg; }
    }

    public string DestinationPortName
    {
      get { return destinationPortName; }
      set { destinationPortName = value; }
    }

    public int DestinationPort
    {
      get { return destinationPort; }
      set { destinationPort = value; }
    }


    /// <summary>
    /// 实现ICommunication接口方法：连接
    /// </summary>
    /// <returns></returns>
    public bool Connection()
    {
      try
      {
        Serial.PortName = destinationPortName;                      //连接端口名
        Serial.BaudRate = destinationPort;                          //串口波特率
        Serial.ReadTimeout = 10000000;
        Serial.StopBits = System.IO.Ports.StopBits.One;            //停止位
        Serial.Parity = System.IO.Ports.Parity.None;               //奇偶校验
        Serial.DataBits = 8;                                       //数据位

        if (Serial.IsOpen)
        {
          Serial.Close();
        }

        Serial.Open();
        Serial.DiscardInBuffer();                                  //清空缓存
        Serial.DiscardOutBuffer();                                 //清空通道
        isSerialPortFalg = true;                                   //连接成功标志位
        return isSerialPortFalg;
      }
      catch (Exception ex)
      {
        Logger4net.Entrance.Debug(ex);
        return false;
      }
     

      
    }



    /// <summary>
    /// 实现ICommunication接口方法：断开连接
    /// </summary>
    /// <returns></returns>
    public void Close()
    {
      try
      {
        isSerialPortFalg = false;
        Serial.Close();
      }
      catch (Exception ex)
      {
        
      }
    }


    /// <summary>
    /// 实现ICommunication接口方法：发送数据
    /// </summary>
    /// <param name="mes"></param>
    /// <returns></returns>
    public bool SendMessage(string mes)
    {
      try
      {
        if (isSerialPortFalg)
        {
          byte[] buffer = new byte[1024];
          buffer = System.Text.Encoding.ASCII.GetBytes(mes);            //将字符串转换为位
          Serial.Write(buffer, 0, buffer.Length);                                 //发送位
          return true;
        }
        else
          return false;
      }
      catch (Exception ex) { return false; }
    }


    /// <summary>
    /// 实现ICommunication接口方法：接收数据
    /// </summary>
    /// <returns></returns>
    public string ReadMessage()
    {
      string receive;
      try
      {
        if (isSerialPortFalg)
        {
          int count = Serial.BytesToRead;
          Byte[] BufferData = new Byte[count];
          receive = null;
          Serial.Read(BufferData, 0, count);
          receive += Encoding.UTF8.GetString(BufferData);
          return receive;
        }
        else
        {
          return receive = "串口未连接";
        }
      }
      catch (Exception ex) { return receive = "Error"; }
    }

  }
}
