﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dobot.api
{
  public class JointPoint
  {
    /// <summary>
    /// J1 轴位置，单位：度
    /// </summary>
    public double j1 { get; set; }

    /// <summary>
    /// J2 轴位置，单位：度
    /// </summary>
    public double j2 { get; set; }

    /// <summary>
    /// J3 轴位置，单位：度
    /// </summary>
    public double j3 { get; set; }

    /// <summary>
    /// J4 轴位置，单位：度
    /// </summary>
    public double j4 { get; set; }

    /// <summary>
    /// J5 轴位置，单位：度
    /// </summary>
    public double j5 { get; set; }

    /// <summary>
    /// J6 轴位置，单位：度
    /// </summary>
    public double j6 { get; set; }

    public JointPoint()
    {
      j1 = j2 = j3 = j4 = j5 = j6 = 0.0;
    }

    override public string ToString()
    {
      string str = string.Format("{0},{1},{2},{3},{4},{5}",
          j1, j2, j3, j4, j5, j6);
      return str;
    }
  }
}
