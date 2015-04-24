﻿using UnityEngine;
using System;
using System.Collections.Generic;
using SDK.Common;
using System.Diagnostics;

namespace SDK.Lib
{
    public class LogSys
    {
        public LockList<string> m_asyncLogList = new LockList<string>("Logger_asyncLogList");              // 这个是多线程访问的
        public LockList<string> m_asyncWarnList = new LockList<string>("Logger_asyncWarnList");            // 这个是多线程访问的
        public LockList<string> m_asyncErrorList = new LockList<string>("Logger_asyncErrorList");          // 这个是多线程访问的

        public string m_tmpStr;
        public bool m_bOutLog = true;          // 是否输出日志

        protected List<LogDeviceBase> m_logDeviceList = new List<LogDeviceBase>();

        public LogSys()
        {
#if UNITY_5
            Application.logMessageReceived += onDebugLogCallbackHandler;
            Application.logMessageReceivedThreaded += onDebugLogCallbackThreadHandler;
#elif UNITY_4_6
            Application.RegisterLogCallback(onDebugLogCallbackHandler);
            Application.RegisterLogCallbackThreaded(onDebugLogCallbackThreadHandler);
#endif

            registerDevice();
        }

        protected void registerDevice()
        {
            LogDeviceBase logDevice = null;

#if ENABLE_WINLOG
            logDevice = new WinLogDevice();
            logDevice.initDevice();
            m_logDeviceList.Add(logDevice);
#endif

#if ENABLE_NETLOG
            logDevice = new NetLogDevice();
            logDevice.initDevice();
            m_logDeviceList.Add(logDevice);
#endif
        }

        // 注册文件日志，因为需要账号，因此需要等待输入账号后才能注册，可能多次注册
        public void registerFileLogDevice()
        {
#if ENABLE_FILELOG
            unRegisterFileLogDevice();

            LogDeviceBase logDevice = null;
            logDevice = new FileLogDevice();
            (logDevice as FileLogDevice).fileSuffix = Ctx.m_instance.m_dataPlayer.m_accountData.m_account;
            logDevice.initDevice();
            m_logDeviceList.Add(logDevice);
#endif
        }

        protected void unRegisterFileLogDevice()
        {
            foreach(var item in m_logDeviceList)
            {
                if(typeof(FileLogDevice) == item.GetType())
                {
                    item.closeDevice();
                    m_logDeviceList.Remove(item);
                    break;
                }
            }
        }

        // 需要一个参数的
        public void debugLog_1(LangItemID idx, string str)
        {
            string textStr = Ctx.m_instance.m_langMgr.getText(LangTypeId.eDebug5, idx);
            m_tmpStr = string.Format(textStr, str);
            Ctx.m_instance.m_logSys.log(m_tmpStr);
        }

        public void formatLog(LangTypeId type, LangItemID item, params string[] param)
        {
            if (param.Length == 0)
            {
                m_tmpStr = Ctx.m_instance.m_langMgr.getText(type, item);
            }
            else if (param.Length == 1)
            {
                m_tmpStr = string.Format(Ctx.m_instance.m_langMgr.getText(type, item), param[0], param[1]);
            }
            Ctx.m_instance.m_logSys.log(m_tmpStr);
        }

        public void log(string message)
        {
            //StackTrace stackTrace = new StackTrace(true);
            //string traceStr = stackTrace.ToString();
            //message = string.Format("{0}\n{1}", message, traceStr);
            if (MThread.isMainThread())
            {
                logout(message, LogColor.LOG);
            }
            else
            {
                asyncLog(message);
            }
        }

        public void warn(string message)
        {
            StackTrace stackTrace = new StackTrace(true);
            string traceStr = stackTrace.ToString();
            message = string.Format("{0}\n{1}", message, traceStr);

            if (MThread.isMainThread())
            {
                logout(message, LogColor.WARN);
            }
            else
            {
                asyncWarn(message);
            }
        }
        
        public void error(string message)
        {
            StackTrace stackTrace = new StackTrace(true);
            string traceStr = stackTrace.ToString();
            message = string.Format("{0}\n{1}", message, traceStr);

            if (MThread.isMainThread())
            {
			    logout(message, LogColor.ERROR);
            }
            else
            {
                asyncError(message);
            }
        }

        // 多线程日志
        protected void asyncLog(string message)
        {
            m_asyncLogList.Add(message);

            //ThreadLogMR threadLog = new ThreadLogMR();
            //threadLog.m_logSys = message;
            //Ctx.m_instance.m_sysMsgRoute.push(threadLog);
        }

        // 多线程日志
        protected void asyncWarn(string message)
        {
            StackTrace stackTrace = new StackTrace(true);        // 这个在 new 的地方生成当时堆栈数据，需要的时候再 new ，否则是旧的堆栈数据
            string traceStr = stackTrace.ToString();
            message = string.Format("{0}\n{1}", message, traceStr);

            m_asyncWarnList.Add(message);

            //ThreadLogMR threadLog = new ThreadLogMR();
            //threadLog.m_logSys = message;
            //Ctx.m_instance.m_sysMsgRoute.push(threadLog);
        }

        // 多线程日志
        protected void asyncError(string message)
        {
            StackTrace stackTrace = new StackTrace(true);        // 这个在 new 的地方生成当时堆栈数据，需要的时候再 new ，否则是旧的堆栈数据
            string traceStr = stackTrace.ToString();
            message = string.Format("{0}\n{1}", message, traceStr);

            m_asyncErrorList.Add(message);

            //ThreadLogMR threadLog = new ThreadLogMR();
            //threadLog.m_logSys = message;
            //Ctx.m_instance.m_sysMsgRoute.push(threadLog);
        }

        public void logout(string message, LogColor type = LogColor.LOG)
        {
        #if THREAD_CALLCHECK
            MThread.needMainThread();
        #endif

            if (m_bOutLog)
            {
                foreach (LogDeviceBase logDevice in m_logDeviceList)
                {
                    logDevice.logout(message, type);
                }
            }
        }

        public void updateLog()
        {
        #if THREAD_CALLCHECK
            MThread.needMainThread();
        #endif

            while ((m_tmpStr = m_asyncLogList.RemoveAt(0)) != default(string))
            {
                logout(m_tmpStr, LogColor.LOG);
            }

            while ((m_tmpStr = m_asyncWarnList.RemoveAt(0)) != default(string))
            {
                logout(m_tmpStr, LogColor.LOG);
            }

            while ((m_tmpStr = m_asyncErrorList.RemoveAt(0)) != default(string))
            {
                logout(m_tmpStr, LogColor.LOG);
            }
        }

        static private void onDebugLogCallbackHandler(string name, string stack, LogType type) 
        { 
            // LogType.Log 日志直接自己输出
            if (LogType.Error == type || LogType.Exception == type)
            {
                Ctx.m_instance.m_logSys.error("onDebugLogCallbackHandler ---- Error");
                Ctx.m_instance.m_logSys.error(name);
                Ctx.m_instance.m_logSys.error(stack);
            }
            else if(LogType.Assert == type || LogType.Warning == type)
            {
                Ctx.m_instance.m_logSys.warn("onDebugLogCallbackHandler ---- Warning");
                Ctx.m_instance.m_logSys.warn(name);
                Ctx.m_instance.m_logSys.warn(stack);
            }
        }

        static private void onDebugLogCallbackThreadHandler(string name, string stack, LogType type)
        {
            if (LogType.Error == type || LogType.Exception == type)
            {
                Ctx.m_instance.m_logSys.asyncError("onDebugLogCallbackThreadHandler ---- Error");
                Ctx.m_instance.m_logSys.asyncError(name);
                Ctx.m_instance.m_logSys.asyncError(stack);
            }
            else if (LogType.Assert == type || LogType.Warning == type)
            {
                Ctx.m_instance.m_logSys.asyncWarn("onDebugLogCallbackThreadHandler ---- Warning");
                Ctx.m_instance.m_logSys.asyncWarn(name);
                Ctx.m_instance.m_logSys.asyncWarn(stack);
            }
        }

        public void closeDevice()
        {
            foreach (LogDeviceBase logDevice in m_logDeviceList)
            {
                logDevice.closeDevice();
            }
        }
    }
}