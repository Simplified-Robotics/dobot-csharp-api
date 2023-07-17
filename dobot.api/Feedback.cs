using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dobot.api
{
  public class Feedback : DobotClient
  {
    private Thread mThread;

    public FeedbackData feedbackData { get; private set; }

    public bool DataHasRead { get; set; }

    public Feedback()
    {
      feedbackData = new FeedbackData();
    }

    public event EventHandler<OnDataReceivedEventArgs> DataReceived;

    protected virtual void OnDataReceived(FeedbackData fbd)
    {
      var e = new OnDataReceivedEventArgs
      {
        FeedbackData = fbd
      };
      DataReceived?.Invoke(this, e);
    }

    protected override void OnConnected(Socket sock)
    {
      sock.SendTimeout = 5000;
      //sock.ReceiveTimeout = 15000;

      mThread = new Thread(OnRecvData);
      mThread.IsBackground = true;
      mThread.Start();
    }

    protected override void OnDisconnected()
    {
      if (null != mThread && mThread.IsAlive)
      {
        try
        {
          mThread.Abort();
          mThread = null;
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.WriteLine("close thread:" + ex.ToString());
        }
      }
    }

    /// <summary>
    /// 接收返回的数据并解析处理
    /// </summary>
    private void OnRecvData()
    {
      byte[] buffer = new byte[4320];//1440*3
      int iHasRead = 0;
      while (IsConnected())
      {
        try
        {
          int iRet = Receive(buffer, iHasRead, buffer.Length - iHasRead, SocketFlags.None);
          if (iRet <= 0)
          {
            continue;
          }
          iHasRead += iRet;
          if (iHasRead < 1440)
          {
            continue;
          }

          bool bHasFound = false;//是否找到数据包头了
          for (int i = 0; i < iHasRead; ++i)
          {
            //找到消息头
            int iMsgSize = buffer[i + 1];
            iMsgSize <<= 8;
            iMsgSize |= buffer[i];
            iMsgSize &= 0x00FFFF;
            if (1440 != iMsgSize)
            {
              continue;
            }
            //校验
            ulong checkValue = BitConverter.ToUInt64(buffer, i + 48);
            if (0x0123456789ABCDEF == checkValue)
            {//找到了校验值
              bHasFound = true;
              if (i != 0)
              {//说明存在粘包，要把前面的数据清理掉
                iHasRead = iHasRead - i;
                Array.Copy(buffer, i, buffer, 0, buffer.Length - i);
              }
              break;
            }
          }
          if (!bHasFound)
          {//如果没找到头，判断数据长度是不是快超过了总长度，超过了，说明数据全都有问题，删掉
            if (iHasRead >= buffer.Length) iHasRead = 0;
            continue;
          }
          //再次判断字节数是否够
          if (iHasRead < 1440)
          {
            continue;
          }
          iHasRead = iHasRead - 1440;
          //按照协议的格式解析数据

          (feedbackData, DataHasRead) = ParseData(buffer);
          if (DataHasRead)
          {
            (var fb, _) = ParseData(buffer);
            OnDataReceived(fb);
          }

          Array.Copy(buffer, 1440, buffer, 0, buffer.Length - 1440);
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.WriteLine("recv thread:" + ex.ToString());
        }
      }
    }

    ///// <summary>
    ///// 解析数据
    ///// </summary>
    ///// <param name="buffer">一包完整的数据</param>
    //private void ParseData(byte[] buffer)
    //{
    //  int iStartIndex = 0;

    //  feedbackData.MessageSize = BitConverter.ToInt16(buffer, iStartIndex); //unsigned short
    //  iStartIndex += 2;

    //  for (int i = 0; i < feedbackData.Reserved1.Count; ++i)
    //  {
    //    feedbackData.Reserved1[i] = BitConverter.ToInt16(buffer, iStartIndex);
    //    iStartIndex += 2;
    //  }

    //  feedbackData.DigitalInputs = BitConverter.ToInt64(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.DigitalOutputs = BitConverter.ToInt64(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.RobotMode = BitConverter.ToInt64(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.TimeStamp = BitConverter.ToInt64(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.Reserved2 = BitConverter.ToInt64(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.TestValue = BitConverter.ToInt64(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.Reserved3 = BitConverter.ToInt64(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.SpeedScaling = BitConverter.ToDouble(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.LinearMomentumNorm = BitConverter.ToDouble(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.VMain = BitConverter.ToDouble(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.VRobot = BitConverter.ToDouble(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.IRobot = BitConverter.ToDouble(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.Reserved4 = BitConverter.ToDouble(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.Reserved5 = BitConverter.ToDouble(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  for (int i = 0; i < feedbackData.ToolAccelerometerValues.Count; ++i)
    //  {
    //    feedbackData.ToolAccelerometerValues[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.ElbowPosition.Count; ++i)
    //  {
    //    feedbackData.ElbowPosition[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.ElbowVelocity.Count; ++i)
    //  {
    //    feedbackData.ElbowVelocity[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.QTarget.Count; ++i)
    //  {
    //    feedbackData.QTarget[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.QdTarget.Count; ++i)
    //  {
    //    feedbackData.QdTarget[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.QddTarget.Count; ++i)
    //  {
    //    feedbackData.QddTarget[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.ITarget.Count; ++i)
    //  {
    //    feedbackData.ITarget[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.MTarget.Count; ++i)
    //  {
    //    feedbackData.MTarget[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.QActual.Count; ++i)
    //  {
    //    feedbackData.QActual[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.QdActual.Count; ++i)
    //  {
    //    feedbackData.QdActual[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.IActual.Count; ++i)
    //  {
    //    feedbackData.IActual[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.IControl.Count; ++i)
    //  {
    //    feedbackData.IControl[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.ToolVectorActual.Count; ++i)
    //  {
    //    feedbackData.ToolVectorActual[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.TCPSpeedActual.Count; ++i)
    //  {
    //    feedbackData.TCPSpeedActual[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.TCPForce.Count; ++i)
    //  {
    //    feedbackData.TCPForce[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.ToolVectorTarget.Count; ++i)
    //  {
    //    feedbackData.ToolVectorTarget[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.TCPSpeedTarget.Count; ++i)
    //  {
    //    feedbackData.TCPSpeedTarget[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.MotorTempetatures.Count; ++i)
    //  {
    //    feedbackData.MotorTempetatures[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.JointModes.Count; ++i)
    //  {
    //    feedbackData.JointModes[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.VActual.Count; ++i)
    //  {
    //    feedbackData.VActual[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.Handtype.Length; ++i)
    //  {
    //    feedbackData.Handtype[i] = buffer[iStartIndex];
    //    iStartIndex += 1;
    //  }

    //  feedbackData.User = buffer[iStartIndex];
    //  iStartIndex += 1;

    //  feedbackData.Tool = buffer[iStartIndex];
    //  iStartIndex += 1;

    //  feedbackData.RunQueuedCmd = buffer[iStartIndex];
    //  iStartIndex += 1;

    //  feedbackData.PauseCmdFlag = buffer[iStartIndex];
    //  iStartIndex += 1;

    //  feedbackData.VelocityRatio = buffer[iStartIndex];
    //  iStartIndex += 1;

    //  feedbackData.AccelerationRatio = buffer[iStartIndex];
    //  iStartIndex += 1;

    //  feedbackData.JerkRatio = buffer[iStartIndex];
    //  iStartIndex += 1;

    //  feedbackData.XYZVelocityRatio = buffer[iStartIndex];
    //  iStartIndex += 1;

    //  feedbackData.RVelocityRatio = buffer[iStartIndex];
    //  iStartIndex += 1;

    //  feedbackData.XYZAccelerationRatio = buffer[iStartIndex];
    //  iStartIndex += 1;

    //  feedbackData.RAccelerationRatio = buffer[iStartIndex];
    //  iStartIndex += 1;

    //  feedbackData.XYZJerkRatio = buffer[iStartIndex];
    //  iStartIndex += 1;

    //  feedbackData.RJerkRatio = buffer[iStartIndex];
    //  iStartIndex += 1;

    //  feedbackData.BrakeStatus = buffer[iStartIndex];
    //  iStartIndex += 1;
    //  feedbackData.EnableStatus = buffer[iStartIndex];
    //  iStartIndex += 1;
    //  feedbackData.DragStatus = buffer[iStartIndex];
    //  iStartIndex += 1;
    //  feedbackData.RunningStatus = buffer[iStartIndex];
    //  iStartIndex += 1;
    //  feedbackData.ErrorStatus = buffer[iStartIndex];
    //  iStartIndex += 1;
    //  feedbackData.JogStatus = buffer[iStartIndex];
    //  iStartIndex += 1;
    //  feedbackData.RobotType = buffer[iStartIndex];
    //  iStartIndex += 1;
    //  feedbackData.DragButtonSignal = buffer[iStartIndex];
    //  iStartIndex += 1;
    //  feedbackData.EnableButtonSignal = buffer[iStartIndex];
    //  iStartIndex += 1;
    //  feedbackData.RecordButtonSignal = buffer[iStartIndex];
    //  iStartIndex += 1;
    //  feedbackData.ReappearButtonSignal = buffer[iStartIndex];
    //  iStartIndex += 1;
    //  feedbackData.JawButtonSignal = buffer[iStartIndex];
    //  iStartIndex += 1;
    //  feedbackData.SixForceOnline = buffer[iStartIndex];
    //  iStartIndex += 1;

    //  for (int i = 0; i < feedbackData.Reserved6.Length; ++i)
    //  {
    //    feedbackData.Reserved6[i] = buffer[iStartIndex];
    //    iStartIndex += 1;
    //  }

    //  for (int i = 0; i < feedbackData.MActual.Count; ++i)
    //  {
    //    feedbackData.MActual[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  feedbackData.Load = BitConverter.ToDouble(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.CenterX = BitConverter.ToDouble(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.CenterY = BitConverter.ToDouble(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  feedbackData.CenterZ = BitConverter.ToDouble(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  for (int i = 0; i < feedbackData.UserValu.Count; ++i)
    //  {
    //    feedbackData.UserValu[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.Tools.Count; ++i)
    //  {
    //    feedbackData.Tools[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  feedbackData.TraceIndex = BitConverter.ToDouble(buffer, iStartIndex);
    //  iStartIndex += 8;

    //  for (int i = 0; i < feedbackData.SixForceValue.Count; ++i)
    //  {
    //    feedbackData.SixForceValue[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.TargetQuaternion.Count(); ++i)
    //  {
    //    feedbackData.TargetQuaternion[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.ActualQuaternion.Count; ++i)
    //  {
    //    feedbackData.ActualQuaternion[i] = BitConverter.ToDouble(buffer, iStartIndex);
    //    iStartIndex += 8;
    //  }

    //  for (int i = 0; i < feedbackData.Reserved7.Length; ++i)
    //  {
    //    feedbackData.Reserved7[i] = buffer[iStartIndex];
    //    iStartIndex += 1;
    //  }

    //  DataHasRead = true;
    //}

    private static (FeedbackData, bool) ParseData(byte[] buffer)
    {
      int iStartIndex = 0;
      var feedbackData = new FeedbackData();
      feedbackData.MessageSize = BitConverter.ToInt16(buffer, iStartIndex); //unsigned short
      iStartIndex += 2;

      for (int i = 0; i < feedbackData.Reserved1.Count; ++i)
      {
        feedbackData.Reserved1[i] = BitConverter.ToInt16(buffer, iStartIndex);
        iStartIndex += 2;
      }

      feedbackData.DigitalInputs = BitConverter.ToInt64(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.DigitalOutputs = BitConverter.ToInt64(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.RobotMode = BitConverter.ToInt64(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.TimeStamp = BitConverter.ToInt64(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.Reserved2 = BitConverter.ToInt64(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.TestValue = BitConverter.ToInt64(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.Reserved3 = BitConverter.ToInt64(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.SpeedScaling = BitConverter.ToDouble(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.LinearMomentumNorm = BitConverter.ToDouble(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.VMain = BitConverter.ToDouble(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.VRobot = BitConverter.ToDouble(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.IRobot = BitConverter.ToDouble(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.Reserved4 = BitConverter.ToDouble(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.Reserved5 = BitConverter.ToDouble(buffer, iStartIndex);
      iStartIndex += 8;

      for (int i = 0; i < feedbackData.ToolAccelerometerValues.Count; ++i)
      {
        feedbackData.ToolAccelerometerValues[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.ElbowPosition.Count; ++i)
      {
        feedbackData.ElbowPosition[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.ElbowVelocity.Count; ++i)
      {
        feedbackData.ElbowVelocity[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.QTarget.Count; ++i)
      {
        feedbackData.QTarget[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.QdTarget.Count; ++i)
      {
        feedbackData.QdTarget[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.QddTarget.Count; ++i)
      {
        feedbackData.QddTarget[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.ITarget.Count; ++i)
      {
        feedbackData.ITarget[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.MTarget.Count; ++i)
      {
        feedbackData.MTarget[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.QActual.Count; ++i)
      {
        feedbackData.QActual[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.QdActual.Count; ++i)
      {
        feedbackData.QdActual[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.IActual.Count; ++i)
      {
        feedbackData.IActual[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.IControl.Count; ++i)
      {
        feedbackData.IControl[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.ToolVectorActual.Count; ++i)
      {
        feedbackData.ToolVectorActual[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.TCPSpeedActual.Count; ++i)
      {
        feedbackData.TCPSpeedActual[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.TCPForce.Count; ++i)
      {
        feedbackData.TCPForce[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.ToolVectorTarget.Count; ++i)
      {
        feedbackData.ToolVectorTarget[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.TCPSpeedTarget.Count; ++i)
      {
        feedbackData.TCPSpeedTarget[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.MotorTempetatures.Count; ++i)
      {
        feedbackData.MotorTempetatures[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.JointModes.Count; ++i)
      {
        feedbackData.JointModes[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.VActual.Count; ++i)
      {
        feedbackData.VActual[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.Handtype.Length; ++i)
      {
        feedbackData.Handtype[i] = buffer[iStartIndex];
        iStartIndex += 1;
      }

      feedbackData.User = buffer[iStartIndex];
      iStartIndex += 1;

      feedbackData.Tool = buffer[iStartIndex];
      iStartIndex += 1;

      feedbackData.RunQueuedCmd = buffer[iStartIndex];
      iStartIndex += 1;

      feedbackData.PauseCmdFlag = buffer[iStartIndex];
      iStartIndex += 1;

      feedbackData.VelocityRatio = buffer[iStartIndex];
      iStartIndex += 1;

      feedbackData.AccelerationRatio = buffer[iStartIndex];
      iStartIndex += 1;

      feedbackData.JerkRatio = buffer[iStartIndex];
      iStartIndex += 1;

      feedbackData.XYZVelocityRatio = buffer[iStartIndex];
      iStartIndex += 1;

      feedbackData.RVelocityRatio = buffer[iStartIndex];
      iStartIndex += 1;

      feedbackData.XYZAccelerationRatio = buffer[iStartIndex];
      iStartIndex += 1;

      feedbackData.RAccelerationRatio = buffer[iStartIndex];
      iStartIndex += 1;

      feedbackData.XYZJerkRatio = buffer[iStartIndex];
      iStartIndex += 1;

      feedbackData.RJerkRatio = buffer[iStartIndex];
      iStartIndex += 1;

      feedbackData.BrakeStatus = buffer[iStartIndex];
      iStartIndex += 1;
      feedbackData.EnableStatus = buffer[iStartIndex];
      iStartIndex += 1;
      feedbackData.DragStatus = buffer[iStartIndex];
      iStartIndex += 1;
      feedbackData.RunningStatus = buffer[iStartIndex];
      iStartIndex += 1;
      feedbackData.ErrorStatus = buffer[iStartIndex];
      iStartIndex += 1;
      feedbackData.JogStatus = buffer[iStartIndex];
      iStartIndex += 1;
      feedbackData.RobotType = buffer[iStartIndex];
      iStartIndex += 1;
      feedbackData.DragButtonSignal = buffer[iStartIndex];
      iStartIndex += 1;
      feedbackData.EnableButtonSignal = buffer[iStartIndex];
      iStartIndex += 1;
      feedbackData.RecordButtonSignal = buffer[iStartIndex];
      iStartIndex += 1;
      feedbackData.ReappearButtonSignal = buffer[iStartIndex];
      iStartIndex += 1;
      feedbackData.JawButtonSignal = buffer[iStartIndex];
      iStartIndex += 1;
      feedbackData.SixForceOnline = buffer[iStartIndex];
      iStartIndex += 1;

      for (int i = 0; i < feedbackData.Reserved6.Length; ++i)
      {
        feedbackData.Reserved6[i] = buffer[iStartIndex];
        iStartIndex += 1;
      }

      for (int i = 0; i < feedbackData.MActual.Count; ++i)
      {
        feedbackData.MActual[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      feedbackData.Load = BitConverter.ToDouble(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.CenterX = BitConverter.ToDouble(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.CenterY = BitConverter.ToDouble(buffer, iStartIndex);
      iStartIndex += 8;

      feedbackData.CenterZ = BitConverter.ToDouble(buffer, iStartIndex);
      iStartIndex += 8;

      for (int i = 0; i < feedbackData.UserValu.Count; ++i)
      {
        feedbackData.UserValu[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.Tools.Count; ++i)
      {
        feedbackData.Tools[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      feedbackData.TraceIndex = BitConverter.ToDouble(buffer, iStartIndex);
      iStartIndex += 8;

      for (int i = 0; i < feedbackData.SixForceValue.Count; ++i)
      {
        feedbackData.SixForceValue[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.TargetQuaternion.Count(); ++i)
      {
        feedbackData.TargetQuaternion[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.ActualQuaternion.Count; ++i)
      {
        feedbackData.ActualQuaternion[i] = BitConverter.ToDouble(buffer, iStartIndex);
        iStartIndex += 8;
      }

      for (int i = 0; i < feedbackData.Reserved7.Length; ++i)
      {
        feedbackData.Reserved7[i] = buffer[iStartIndex];
        iStartIndex += 1;
      }

      return (feedbackData, true);
    }

    public string ConvertRobotMode()
    {
      switch (feedbackData.RobotMode)
      {
        case FeedbackData.NO_CONTROLLER:
          return "NO_CONTROLLER";
        case FeedbackData.NO_CONNECTED:
          return "NO_CONNECTED";
        case FeedbackData.ROBOT_MODE_INIT:
          return "ROBOT_MODE_INIT";
        case FeedbackData.ROBOT_MODE_BRAKE_OPEN:
          return "ROBOT_MODE_BRAKE_OPEN";
        case FeedbackData.ROBOT_RESERVED:
          return "ROBOT_RESERVED";
        case FeedbackData.ROBOT_MODE_DISABLED:
          return "ROBOT_MODE_DISABLED";
        case FeedbackData.ROBOT_MODE_ENABLE:
          return "ROBOT_MODE_ENABLE";
        case FeedbackData.ROBOT_MODE_BACKDRIVE:
          return "ROBOT_MODE_BACKDRIVE";
        case FeedbackData.ROBOT_MODE_RUNNING:
          return "ROBOT_MODE_RUNNING";
        case FeedbackData.ROBOT_MODE_RECORDING:
          return "ROBOT_MODE_RECORDING";
        case FeedbackData.ROBOT_MODE_ERROR:
          return "ROBOT_MODE_ERROR";
        case FeedbackData.ROBOT_MODE_PAUSE:
          return "ROBOT_MODE_PAUSE";
        case FeedbackData.ROBOT_MODE_JOG:
          return "ROBOT_MODE_JOG";
      }
      return string.Format("UNKNOW：RobotMode={0}", feedbackData.RobotMode);
    }

    public bool IsEnabled()
    {
      return FeedbackData.ROBOT_MODE_ENABLE == feedbackData.RobotMode;
    }
  }
  public class OnDataReceivedEventArgs : EventArgs
  {
    public FeedbackData FeedbackData { get; set; }
  }
}
