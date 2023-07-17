using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;


namespace dobot.api
{
  public class ErrorInfoHelper
  {
    private static Dictionary<int, ErrorInfoBean> mControllerBeans = new Dictionary<int, ErrorInfoBean>();
    private static Dictionary<int, ErrorInfoBean> mServoBeans = new Dictionary<int, ErrorInfoBean>();
    public static void ParseControllerJsonFile(string strFullFile)
    {
      try
      {
        string strJson = File.ReadAllText(strFullFile);
        List<ErrorInfoBean> result = JsonSerializer.Deserialize<List<ErrorInfoBean>>(strJson);
        foreach (var bean in result)
        {
          bean.Type = "Controller";
          mControllerBeans.Add(bean.id, bean);
        }
      }
      catch (Exception )
      {
      }
    }
    public static void ParseServoJsonFile(string strFullFile)
    {
      try
      {
        string strJson = File.ReadAllText(strFullFile);
        List<ErrorInfoBean> result = JsonSerializer.Deserialize<List<ErrorInfoBean>>(strJson);
        foreach (var bean in result)
        {
          bean.Type = "Servo";
          mServoBeans.Add(bean.id, bean);
        }
      }
      catch (Exception)
      {
      }
    }

    public static ErrorInfoBean FindController(int id)
    {
      if (mControllerBeans.ContainsKey(id))
      {
        return mControllerBeans[id];
      }
      return null;
    }
    public static ErrorInfoBean FindServo(int id)
    {
      if (mServoBeans.ContainsKey(id))
      {
        return mServoBeans[id];
      }
      return null;
    }
  }
}
