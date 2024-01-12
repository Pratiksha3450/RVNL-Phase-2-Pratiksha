using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace RVNLMIS.Common
{
    public class ApiLogger
    {
        #region --Class Variables--
        private static DirectoryInfo _directoryInfo;
        private static FileStream _fileStream;
        private static StreamWriter _streamWriter;
        private static StackTrace _stackTrace;
        private static MethodBase _methodBase;
        #endregion

        #region --Private methods--
        /// <summary>
        /// Logs the error information.
        /// </summary>
        /// <param name="info">The error information.</param>
        private static void Info(Object info)
        {
            LogErrorToLogFile(info);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        private static void Dispose()
        {
            if (_directoryInfo != null)
            {
                _directoryInfo = null;
            }

            if (_streamWriter != null)
            {
                _streamWriter.Close();
                _streamWriter.Dispose();
                _streamWriter = null;
            }

            if (_fileStream != null)
            {
                _fileStream.Dispose();
                _fileStream = null;
            }

            if (_stackTrace != null)
            {
                _stackTrace = null;
            }

            if (_methodBase != null)
            {
                _methodBase = null;
            }
        }

        /// <summary>
        /// Logs the error to log file.
        /// </summary>
        /// <param name="info">The info.</param>
        private static void LogErrorToLogFile(Object info)
        {
            //Gets folder & file information of the log file
            //GlobalVariables objGlobalVariables = new GlobalVariables();

            string logFile = HttpContext.Current.Server.MapPath(GlobalVariables.ApiLog);

            string[] fileLocation = logFile.Split('\\');
            string folderName = fileLocation[fileLocation.Length - 2];
            string fileName = logFile;
            string user = string.Empty;
            _directoryInfo = new DirectoryInfo(HttpContext.Current.Server.MapPath(folderName));

            //Check for existence of logger file
            if (File.Exists(fileName))
            {
                try
                {
                    //_fileStream = new FileStream(fileName, FileMode.Append, FileAccess.Write);
                    //_streamWriter = new StreamWriter(_fileStream);
                    string val = info.ToString();
                    //_streamWriter.WriteLine(val);
                    File.AppendAllText(logFile, DateTime.Now.ToString() + Environment.NewLine + val + Environment.NewLine + Environment.NewLine);
                }
                catch (UnauthorizedAccessException ex)
                {

                }
                catch (Exception Ex)
                {
                    LogInfo(Ex);
                }
                finally
                {
                    //_fileStream.Dispose();
                }
            }
            else
            {
                //If file doesn't exist create one
                try
                {
                    _directoryInfo = Directory.CreateDirectory(_directoryInfo.FullName);

                    _fileStream = File.Create(fileName);
                    _streamWriter = new StreamWriter(_fileStream);
                    String val1 = info.ToString();

                    _streamWriter.WriteLine(val1);

                    _streamWriter.Close();
                    _fileStream.Close();
                }
                catch (UnauthorizedAccessException ex)
                {

                }
                catch (Exception ex)
                {
                    LogInfo(ex);
                }
                finally
                {
                    // _fileStream.Dispose();
                }
            }
        }

        #endregion

        #region --Public methods--
        /// <summary>
        /// Logs the error information.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public static void LogInfo(Exception ex)
        {
            try
            {
                //  MessageBox.Show(ex.Message);
                //Writes error information to the log file including name of the file, line number & error message description
                _stackTrace = new StackTrace(ex, true);
                String methodName;
                Int32 lineNumber;
                string fileNames = _stackTrace.GetFrame((_stackTrace.FrameCount - 1)).GetFileName();

                if (fileNames != null)
                {
                    //For handled exceptions using try-catch
                    lineNumber = _stackTrace.GetFrame((_stackTrace.FrameCount - 1)).GetFileLineNumber();
                    _methodBase = _stackTrace.GetFrame((_stackTrace.FrameCount - 1)).GetMethod();    //These two lines are respnsible to find out name of the method
                    methodName = _methodBase.Name;
                }
                else
                {
                    //For all unhandled exceptions
                    fileNames = _stackTrace.GetFrame(0).GetFileName();
                    lineNumber = _stackTrace.GetFrame(0).GetFileLineNumber();
                    _methodBase = _stackTrace.GetFrame(0).GetMethod();    //These two lines are respnsible to find out name of the method
                    methodName = _methodBase.Name;
                }

                StringBuilder errorString = new StringBuilder();

                errorString.Append("---------");
                errorString.Append(string.Concat("\r\n", "Error Date   : ", DateTime.Now.ToString(), "\r\n"));
                errorString.Append(string.Concat("File Name    : ", fileNames, "\r\n"));
                errorString.Append(string.Concat("Method Name  : ", methodName, "\r\n"));
                errorString.Append(string.Concat("Line Number  : ", lineNumber.ToString(), "\r\n"));
                errorString.Append(string.Concat("Error Message: ", ex.Message, "\r\n"));
                errorString.Append("---------");

                // errorString.Append(string.Concat(DateTime.Now.ToString(), " | ", fileNames, " | ", methodName, " | ", lineNumber.ToString(), " | ", ex.Message, " | "));//, "\r\n"));

                Info(errorString);
            }
            catch (UnauthorizedAccessException e)
            {

            }
            catch (Exception genEx)
            {
                Info(ex.Message);
            }
            finally
            {
                Dispose();
            }
        }

        /// <summary>
        /// Logs the error information.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogInfo(string message)
        {
            try
            {
                //Write general message to the log file
                Info(string.Concat("Message-----", message));
            }
            catch (Exception genEx)
            {
                Info(genEx.Message);
            }
        }

        #endregion
    }

}