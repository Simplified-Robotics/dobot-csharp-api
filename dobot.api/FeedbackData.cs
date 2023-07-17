using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dobot.api
{
  public class FeedbackData
  {
    #region 机器人模式
    public const int NO_CONTROLLER = -1;
    public const int NO_CONNECTED = 0;
    public const int ROBOT_MODE_INIT = 1;
    public const int ROBOT_MODE_BRAKE_OPEN = 2;
    public const int ROBOT_RESERVED = 3;
    public const int ROBOT_MODE_DISABLED = 4;
    public const int ROBOT_MODE_ENABLE = 5;
    public const int ROBOT_MODE_BACKDRIVE = 6;
    public const int ROBOT_MODE_RUNNING = 7;
    public const int ROBOT_MODE_RECORDING = 8;
    public const int ROBOT_MODE_ERROR = 9;
    public const int ROBOT_MODE_PAUSE = 10;
    public const int ROBOT_MODE_JOG = 11;
    #endregion

    public short MessageSize { get; set; } = 0;//消息字节总长度

    public List<short> Reserved1 { get; set; } = new List<short>(new short[3]);//保留位

    public long DigitalInputs { get; set; } = 0;//数字输入
    public long DigitalOutputs { get; set; } = 0;//数字输出
    public long RobotMode { get; set; } = -1;//机器人模式
    public long TimeStamp { get; set; } = 0;//时间戳（单位ms）

    public long Reserved2 { get; set; } = 0;//保留位
    public long TestValue { get; set; } = 0;//内存结构测试标准值  0x0123 4567 89AB CDEF
    public double Reserved3 { get; set; } = 0;//保留位

    public double SpeedScaling { get; set; } = 0;//速度比例
    public double LinearMomentumNorm { get; set; } = 0; //机器人当前动量
    public double VMain { get; set; } = 0;//控制板电压
    public double VRobot { get; set; } = 0;//机器人电压
    public double IRobot { get; set; } = 0;//机器人电流

    public double Reserved4 { get; set; } = 0;//保留位
    public double Reserved5 { get; set; } = 0;//保留位

    public List<double> ToolAccelerometerValues { get; set; } = new List<double>(new double[3]);//TCP加速度
    public List<double> ElbowPosition { get; set; } = new List<double>(new double[3]);//肘位置
    public List<double> ElbowVelocity { get; set; } = new List<double>(new double[3]);//肘速度

    public List<double> QTarget { get; set; } = new List<double>(new double[6]);//目标关节位置
    public List<double> QdTarget { get; set; } = new List<double>(new double[6]);//目标关节速度
    public List<double> QddTarget { get; set; } = new List<double>(new double[6]);//目标关节加速度
    public List<double> ITarget { get; set; } = new List<double>(new double[6]);//目标关节加速度
    public List<double> MTarget { get; set; }= new List<double>(new double[6]);//目标关节电流
    public List<double> QActual { get; set; }= new List<double>(new double[6]);//实际关节位置
    public List<double> QdActual { get; set; } = new List<double>(new double[6]);//实际关节速度
    public List<double> IActual { get; set; } = new List<double>(new double[6]);//实际关节电流
    public List<double> IControl { get; set; } = new List<double>(new double[6]);//TCP传感器力值
    public List<double> ToolVectorActual { get; set; } = new List<double>(new double[6]);//TCP笛卡尔实际坐标值
    public List<double> TCPSpeedActual { get; set; } = new List<double>(new double[6]); //TCP笛卡尔实际速度值
    public List<double> TCPForce { get; set; } = new List<double>(new double[6]);//TCP力值
    public List<double> ToolVectorTarget { get; set; } = new List<double>(new double[6]);//TCP笛卡尔目标坐标值
    public List<double> TCPSpeedTarget { get; set; } = new List<double>(new double[6]);//TCP笛卡尔目标速度值
    public List<double> MotorTempetatures { get; set; } = new List<double>(new double[6]);//关节温度
    public List<double> JointModes { get; set; } = new List<double>(new double[6]);//关节控制模式
    public List<double> VActual { get; set; } = new List<double>(new double[6]);//关节电压

    public byte[] Handtype { get; set; } = new byte[4];//手系
    public byte User { get; set; } = 0;//用户坐标
    public byte Tool { get; set; } = 0;//工具坐标
    public byte RunQueuedCmd { get; set; } = 0;//算法队列运行标志
    public byte PauseCmdFlag { get; set; } = 0;//算法队列暂停标志
    public byte VelocityRatio { get; set; } = 0;//关节速度比例(0~100)
    public byte AccelerationRatio { get; set; } = 0;//关节加速度比例(0~100)
    public byte JerkRatio { get; set; } = 0;//关节加加速度比例(0~100)
    public byte XYZVelocityRatio { get; set; } = 0;//笛卡尔位置速度比例(0~100)
    public byte RVelocityRatio { get; set; } = 0;//笛卡尔姿态速度比例(0~100)
    public byte XYZAccelerationRatio { get; set; } = 0;//笛卡尔位置加速度比例(0~100)
    public byte RAccelerationRatio { get; set; } = 0;//笛卡尔姿态加速度比例(0~100)
    public byte XYZJerkRatio { get; set; } = 0;//笛卡尔位置加加速度比例(0~100)
    public byte RJerkRatio { get; set; } = 0;//笛卡尔姿态加加速度比例(0~100)

    public byte BrakeStatus { get; set; } = 0; //机器人抱闸状态
    public byte EnableStatus { get; set; } = 0;//机器人使能状态
    public byte DragStatus { get; set; } = 0;//机器人拖拽状态
    public byte RunningStatus { get; set; } = 0;//机器人运行状态
    public byte ErrorStatus { get; set; } = 0;//机器人报警状态
    public byte JogStatus { get; set; } = 0;//机器人点动状态
    public byte RobotType { get; set; }= 0; //机器类型
    public byte DragButtonSignal { get; set; } = 0; //按钮板拖拽信号
    public byte EnableButtonSignal { get; set; } = 0;//按钮板使能信号
    public byte RecordButtonSignal { get; set; }= 0;//按钮板录制信号
    public byte ReappearButtonSignal { get; set; } = 0;//按钮板复现信号
    public byte JawButtonSignal { get; set; } = 0; //按钮板夹爪控制信号
    public byte SixForceOnline { get; set; } = 0;//六维力在线状态

    public byte[] Reserved6 { get; set; } = new byte[82];//保留位

    public List<double> MActual { get; set; } = new List<double>(new double[6]);//实际扭矩
    public double Load { get; set; } = 0;//负载重量kg
    public double CenterX { get; set; } = 0;//X方向偏心距离mm
    public double CenterY { get; set; }= 0;//Y方向偏心距离mm
    public double CenterZ { get; set; }= 0;//Z方向偏心距离mm
    public List<double> UserValu { get; set; } = new List<double>(new double[6]);//用户坐标值
    public List<double> Tools { get; set; } = new List<double>(new double[6]);//工具坐标值
    public double TraceIndex { get; set; } = 0;//轨迹复现运行索引
    public List<double> SixForceValue { get; set; } = new List<double>(new double[6]);//当前六维力数据原始值
    public List<double> TargetQuaternion { get; set; } = new List<double>(new double[4]); //[qw,qx,qy,qz] 目标四元数
    public List<double> ActualQuaternion { get; set; } = new List<double>(new double[4]);//[qw,qx,qy,qz]  实际四元数

    public byte[] Reserved7 { get; set; } = new byte[24];//保留位
  }
}
