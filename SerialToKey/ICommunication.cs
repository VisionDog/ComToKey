using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialToKey
{
  interface ICommunication
  {
    /// <summary>
    /// 连接
    /// </summary>
    /// <returns></returns>
    bool Connection();

    /// <summary>
    /// 断开连接
    /// </summary>
    /// <returns></returns>
    void Close();

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="mes">要发送的字符串</param>
    /// <returns></returns>
    bool SendMessage(string mes);

    /// <summary>
    /// 接收数据
    /// </summary>
    /// <returns></returns>
    string ReadMessage();
    

    /// <summary>
    /// 连接标志位
    /// </summary>
    bool IsConnectionFalg
    {
      get;
    }

    /// <summary>
    /// 目标端口名
    /// </summary>
    string DestinationPortName
    {
      set;
    }

    /// <summary>
    /// 目标端口号
    /// </summary>
    int DestinationPort
    {
      set;
    }

  }
}
