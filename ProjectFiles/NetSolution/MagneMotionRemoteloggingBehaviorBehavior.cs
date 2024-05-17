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
using System.Net.Sockets;
using System.Text;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using RA.NodeControllerInterface;



[CustomBehavior]
public class MagneMotionRemoteloggingBehaviorBehavior : BaseNetBehavior
{



    RemoteLogHost rmtLog; // remote log host
    PeriodicTask _task; // task for update status

    Queue<LogModel> loggerBuffer; //log message buffer
    public override void Start()
    {

        rmtLog = new RemoteLogHost(Node.RemoteIP, Node.Port);
        rmtLog.OnLogMessage += RemoteLogHost_OnLogMessage;

        _task = new PeriodicTask(CycleScanStatusTaskHandle, 1000, Node);
        _task.Start();

        loggerBuffer = new Queue<LogModel>(Node.MessagePoolSize);
        
    }

  

    public override void Stop()
    {
        _task.Cancel();
        _task.Dispose();
        _task = null;

        rmtLog.OnLogMessage -= RemoteLogHost_OnLogMessage;
        rmtLog.Stop();
        rmtLog.Dispose();
        rmtLog = null;

    }


    [ExportMethod]
    public void Listen()
    {
        rmtLog.Start();
        
    }


    [ExportMethod]
    public void Close()
    {
       rmtLog.Stop();
    }

    [ExportMethod]
    public void ClearLog()
    {
        rmtLog.ClearLog();
        ClearLogBuffer();
        Node.RecvMsg = string.Empty;
    }



    private void CycleScanStatusTaskHandle()
    {
        Node.Running = rmtLog.IsRunning;
    }



    private void RemoteLogHost_OnLogMessage(object sender, RemoteLogRecvResult result)
    {
        //throw new NotImplementedException();


        Node.RecvMsg += result.MarkMessage;
        Node.RecvMsg.TakeLast(Node.RecvMsgSize);


        if(loggerBuffer.Count + result.Messages.Length > Node.MessagePoolSize)
        {
            var delete_size = loggerBuffer.Count + result.Messages.Length - Node.MessagePoolSize;
            for(var i = 0; i < delete_size; i++)
            {
                var mapper = loggerBuffer.Dequeue();
                mapper.node.Delete();

            }
        }

        foreach(var msg in result.Messages)
        {
            AddLogMessage(msg);
        }
        



    }


    private void ClearLogBuffer()
    {
        foreach(var item in Node.Loggers.Children)
        {
            item.Delete();
        } 

        loggerBuffer.Clear();
    }

    

    private void AddLogMessage(LogMessage log)
    {
        var obj = InformationModel.MakeObject<MagneMotionLogMessageType>(log.TimeStamp.ToString());
        obj.Timestamp = log.TimeStamp;
        obj.Source = log.Source;
        obj.SubSource = log.SubSource;
        obj.Message = log.Message;
        obj.TimestampString = $"{log.TimeStamp.Year}/{log.TimeStamp.Month.ToString("00")}/{log.TimeStamp.Day.ToString("00")} {log.TimeStamp.Hour.ToString("00")}:{log.TimeStamp.Minute.ToString("00")}:{log.TimeStamp.Second.ToString("00")}.{log.TimeStamp.Millisecond.ToString("000")}";


        Node.Loggers.Add(obj);
        loggerBuffer.Enqueue(new LogModel() { Log = log,node = obj});

    }


    struct LogModel
    {
        public LogMessage Log { get; set; }
        public IUANode node { get; set; }
    }

    #region Auto-generated code, do not edit!
    protected new MagneMotionRemoteloggingBehavior Node => (MagneMotionRemoteloggingBehavior)base.Node;
#endregion
}



