using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;


namespace RFTestAUX.Communication
{
    public class EtherNet
    {
        public int m_nIndex;
        public string m_strName;
        public string m_strIP;
        public int m_nPort;
        public int m_nTime;
        public string m_strLineFlag;
        public string m_strAutoLine;



        public string m_strLine;
        public static int m_tcpipCount = 0;

        private bool isConnection = false;

        Thread checkStateThread;

        TcpClient m_client = null;


        private bool beginConnction = false;//
        private int BeginTime = 0;//
        private int EndTime = 0;//


        public EtherNet(int nIndex, string strName, string strIP, int nPort, int nTime, string strLine,string autoUPline)
        {
            m_nIndex = nIndex;
            m_strName = strName;
            m_strIP = strIP;
            m_nPort = nPort;
            m_nTime = nTime;

            m_strLineFlag = strLine;
            if (strLine == "CRLF")
            {
                m_strLine = "\r\n";
            }
            else if (strLine == "CR")
            {
                m_strLine = "\r";
            }
            else if (strLine == "LF")
            {
                m_strLine = "\n";
            }
            else if (strLine == "无")
            {
                m_strLine = "";
            }


            m_strAutoLine = autoUPline;
            m_tcpipCount++;
        }

        ~EtherNet()
        {
            m_tcpipCount--;
        }
        private bool Isonline(TcpClient asTcpClient, SelectMode asSelectMode)
        {
            bool value = false;
            string str = "";

            if (asTcpClient == null)
                return false;

            try
            {

                if(m_strAutoLine=="0")
                {
                        if(asTcpClient.Client.Connected )
                        {
                            value = true;
                        }
                        else
                        {
                            value = false;
                        }
                }
                else
                {
                    str = m_strName;///

                    if (asSelectMode == SelectMode.SelectRead)
                    {
                        if (asTcpClient.Client.Poll(100, asSelectMode) && (asTcpClient.Client.Available == 0) || !asTcpClient.Client.Connected)//网络是否有数据 True
                        {
                            value = false;
                        }
                        else
                        {
                            value = true;

                        }
                    }
                    else if (asSelectMode == SelectMode.SelectWrite)
                    {
                        if (asTcpClient.Client.Poll(100, asSelectMode) && asTcpClient.Client.Connected)//网络是否有数据 True
                        {
                            value = true;
                        }
                        else
                        {
                            value = false;

                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return value;

        }


        //打开网口
        public bool Open()
        {
            if (m_client == null)
            {
                m_client = new TcpClient();
            }
            if (m_client.Connected == false)
            {
                m_client.SendBufferSize = 4096;
                m_client.SendTimeout = m_nTime;
                m_client.ReceiveTimeout = m_nTime;
                m_client.ReceiveBufferSize = 4096;


                m_client.Connect(m_strIP, m_nPort);

                m_client.Client.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, 3000, 1000), null);

                m_client.GetStream().ReadTimeout = m_nTime;
                m_client.GetStream().WriteTimeout = m_nTime;


            }

            checkStateThread = new Thread(new ThreadStart(checkState));
            checkStateThread.IsBackground = true;
            checkStateThread.Start();

            return m_client.Connected;
        }

        private byte[] KeepAlive(int onOff, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }
        /// <summary>
        /// 获取或者设置网络连接状态
        /// </summary>
        public bool IsConnection
        {
            get { return isConnection; }
            set { isConnection = value; }
        }

        public bool AginConnection////
        {
            get { return beginConnction; }
            set { beginConnction = value; }
        }
        private void checkState()
        {
            bool up = false;
            string str = "";
            while (true)
            {

                if(!up)//
                {//
                    up = true;//
                    BeginTime  = Environment.TickCount;//
                }//
                if (((Environment.TickCount - BeginTime > 5000) && up) || beginConnction)//
                {//
                    beginConnction = false;//
                    up = false;//

                    //Thread.Sleep(3000);
                    if (Isonline(m_client, SelectMode.SelectRead) == false)
                    {
                        try
                        {
                            str = m_strName;

                            if (m_client == null)
                            {
                                //m_client.Close();
                                m_client = new TcpClient();
                            }
                            else
                            {
                                if (m_client.Connected == true)/////
                                {
                                    NetworkStream netStream = m_client.GetStream();
                                    netStream.Close();
                                }////

                                m_client.Close();
                                m_client = null;
                                m_client = new TcpClient();
                            }

                            m_client.SendBufferSize = 4096;
                            m_client.SendTimeout = m_nTime;
                            m_client.ReceiveTimeout = m_nTime;
                            m_client.ReceiveBufferSize = 4096;

                            m_client.Connect(m_strIP, m_nPort);

                            m_client.Client.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, 3000, 1000), null);//

                            m_client.GetStream().ReadTimeout = m_nTime;
                            m_client.GetStream().WriteTimeout = m_nTime;


                            isConnection = true;
                        }
                        catch
                        {
                            isConnection = false;
                        }
                    }
                    else
                    {
                        isConnection = true;
                    }
                }//
            }
        }
        //向网口写入数据
        public bool WriteData(Byte[] sendBytes, int nLen)
        {
            if (Isonline(m_client, SelectMode.SelectWrite) == true)
            {
                NetworkStream netStream = m_client.GetStream();
                if (netStream.CanWrite)
                {
                    netStream.Write(sendBytes, 0, nLen);
                }
                return true;
            }
            return false;
        }
        //向网口写入字符串
        public bool WriteString(string strData)
        {
            try
            {
                if (Isonline(m_client, SelectMode.SelectWrite) == true)
                {

                    NetworkStream netStream = m_client.GetStream();
                    if (netStream.CanWrite)
                    {

                        Byte[] sendBytes = Encoding.UTF8.GetBytes(strData);
                        netStream.Write(sendBytes, 0, sendBytes.Length);
                    }
                    else
                    {
                        return false;
                       
                    }
                    //        netStream.Close();
                    return true;
                }
            }
            catch(Exception ex)
            { 
                throw new Exception($"WriteString Error: {ex.Message}");
            }
            return false;
        }
        //向网口写入一行字符
        public bool WriteLine(string strData)
        {
            if (Isonline(m_client, SelectMode.SelectWrite) == true)
            {
                NetworkStream netStream = m_client.GetStream();
                if (netStream.CanWrite)
                {
                    Byte[] sendBytes = Encoding.UTF8.GetBytes(strData + m_strLine);
                    netStream.Write(sendBytes, 0, sendBytes.Length);
                }
                return true;
            }
            return false;
        }
        public int ReadData(byte[] bytes, int nLen)
        {
            if (Isonline(m_client, SelectMode.SelectRead) == true)
            {
                NetworkStream netStream = m_client.GetStream();
                if (netStream.CanRead && netStream.DataAvailable)
                {
                    netStream.Read(bytes, 0, nLen);
                }
                return nLen;
            }
            return 0;
        }
        public int ReadLine(out string strData)
        {
            if (Isonline(m_client, SelectMode.SelectRead) == true)
            {
                NetworkStream netStream = m_client.GetStream();

                if (netStream.CanRead && netStream.DataAvailable)
                {
                    try
                    {

                        byte[] bytes = new byte[m_client.ReceiveBufferSize];

                        int n = netStream.Read(bytes, 0, (int)m_client.ReceiveBufferSize);

                        strData = Encoding.UTF8.GetString(bytes, 0, n);
                    }
                    catch (IOException e)
                    {
                        strData = "";

                    }
                    return strData.Length;

                }
                else
                {
                    strData = "";
                    return 0;
                }

            }
            strData = "";
            return 0;
        }

        //关闭网口
        public void Close()
        {
            if (m_client != null)
            {

                if (m_client.Connected == true)
                {
                    NetworkStream netStream = m_client.GetStream();
                    netStream.Close();
                }
                m_client.Close();
                m_client = null;
                if (checkStateThread != null)
                {
                    checkStateThread.Abort();
                    checkStateThread = null;
                }
            }
        }
    }

}

