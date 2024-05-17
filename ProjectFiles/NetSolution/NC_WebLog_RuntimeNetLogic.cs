#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.Retentivity;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
#endregion
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RA.NodeControllerInterface;
public class NC_WebLog_RuntimeNetLogic : BaseNetLogic
{
    IUANode nLoggers;
    //WebApiClient client;
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        nLoggers = LogicObject.Get("Loggers");
        ClearLogger();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        //client = null;
    }

    [ExportMethod]
    public void GetNCLog(string ip,string user,string pwd)
    {

        if (GetNCLog(ip,user, pwd, "whole",100,out var result))
        {
            var ls = LogMessageTroubleshooting.ParseResponse(result);
            BuildUIObject(ls);
        }
        else
        {
            ClearLogger();
            Log.Error(result);
        }
    }


    [ExportMethod]
    public void GetNCLogWithTimeFilter(string ip, string user, string pwd,DateTime st,DateTime et)
    {

        if (GetNCLog(ip,user,pwd, "whole", 100, out var result))
        {
            var ls = LogMessageTroubleshooting.ParseResponse(result);
            var nls = ls.Where(l=>l.TimeStamp >=  st && l.TimeStamp <= et).ToList();
            BuildUIObject(nls);
        }
        else
        {
            ClearLogger();
            Log.Error(result);
        }
    }





    private bool GetNCLog(string ip,string user,string pwd,string portion,int numlines ,out string result)
    {
        try
        {

            return LogMessageTroubleshooting.GetLog(30000, ip, user, pwd, portion, numlines, out result);
        }catch (Exception ex)
        {
            result = ex.ToString();
            return false;
        }

    
    }



    private void BuildUIObject(List<LogMessage> ls)
    {
        ClearLogger();
        var i = 0;
        foreach (var log in ls)
        {
            var obj = InformationModel.MakeObject<MagneMotionLogMessageType>(i.ToString());
            obj.Timestamp = log.TimeStamp;
            obj.Source = log.Source;
            obj.SubSource = log.SubSource;
            obj.Message = log.Message;
            obj.TimestampString = $"{log.TimeStamp.Year}/{log.TimeStamp.Month.ToString("00")}/{log.TimeStamp.Day.ToString("00")} {log.TimeStamp.Hour.ToString("00")}:{log.TimeStamp.Minute.ToString("00")}:{log.TimeStamp.Second.ToString("00")}.{log.TimeStamp.Millisecond.ToString("000")}";

            nLoggers.Add(obj);
            i++;
        }
    }
    
    
    private void ClearLogger()
    {
        foreach(var item in nLoggers.Children)
        {
            item.Delete();
        }
    }





}
