using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using RFTestAUX.Communication;
using RFTestAUX.Config;

namespace RFTestAUX.Instrument
{
    class DP832 : InstrumentBase
    {
        private int DevHandle = -1;
        public enum OUTPUTMODE { CV,CC,UR}
        public enum CHANNEL:int { CH1=1,CH2,CH3}
        public enum OPMODE : int {OCP,OVP}
        public double[] MeasureValue = new double[3] { 0.0f, 0.0f, 0.0f };
        private ComportCfg comportCfg = null;
        public DP832(HardwareCfgLevelManager1 cfg) : base(cfg) { }
        public override bool DeInit()
        {
            if (comPort != null)
            {
                comPort.Close();
                comPort.Dispose();
            }
            return true;
        }
        public void Fetch(object o)
        {
            int nCh = (int)o;
            if (nCh > (int)CHANNEL.CH3 || nCh < (int)CHANNEL.CH1)
                return;
            string cmd = string.Format(":MEASure:ALL? CH{0}",nCh.ToString());
            for (int nLoops = 0; nLoops < 10; nLoops++)
            {
                string strRet = Query(cmd).ToString().Replace("\r", "").Replace("\n", "");
                string[] strValues = strRet.Split(',');
                if (strValues.Length == 3)
                {
                    for (int i = 0; i < 3; i++)
                        if (!double.TryParse(strValues[i], out MeasureValue[i]))
                        {
                            MeasureValue[i] = 0.0f;
                            throw new InvalidCastException("DP832 feth value is invalid");
                        }
                    break;
                }
            }
        }

        public override bool Init()
        {
            try
            {
                HardwareCfgManager hardwareCfg = ConfigMgr.Instance.HardwareCfgMgr;
                if (Config.ConnectMode.ToUpper() == @"COMPORT")
                {
                    foreach (var it in hardwareCfg.Comports)
                    {
                        if (it.PortName == Config.PortName)
                            comportCfg = it;
                    }
                    comPort = new System.IO.Ports.SerialPort();
                    if (comPort != null && comportCfg != null)
                    {
                        GetPortProfileData(comportCfg);
                        comPort.PortName = comportData.Port;
                        comPort.BaudRate = comportData.BaudRate;
                        comPort.Parity = comportData.parity;
                        comPort.StopBits = comportData.stopbits;
                        comPort.DataBits = comportData.DataBits;
                        comPort.ReadTimeout = comportData.Timeout;
                        comPort.WriteTimeout = comportData.Timeout;
                        if (comPort.IsOpen)
                            comPort.Close();
                        comPort.Open();
                        return comPort.IsOpen;
                    }
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        private object Excute(object objCmd)
        {
            try
            {
                lock (comPort)
                {
                    comPort.WriteLine(objCmd.ToString());
                    Thread.Sleep(50);
                    comPort.WriteLine(":SYSTem:ERR?");
                    Thread.Sleep(50);
                    string strErr = comPort.ReadLine();
                    return strErr != "";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public  object Query(object objCmd)
        {
            try
            {
                lock (comPort)
                {
                    Thread.Sleep(50);
                    comPort.WriteLine(objCmd.ToString());
                    Thread.Sleep(50);
                    return comPort.ReadLine().Replace("\r", "").Replace("\n", "");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #region >>>output
        /// <summary>
        /// 设置输出模式
        /// OUTP: CVCC? CH1
        /// </summary>
        /// <param name="nChannel"></param>
        /// <param name="Mode"></param>
        /// <returns></returns>
        public OUTPUTMODE GetOutputMode(CHANNEL nChannel)
        {
            string strCmd = "";
            string strRet = "";
            strCmd = string.Format("OUTP:MODE? {0}", nChannel.ToString());
            strRet = Query(strCmd).ToString();
            if (strRet.ToUpper().Contains("CV"))
                return OUTPUTMODE.CV;
            else if (strRet.ToUpper().Contains("CC"))
                return OUTPUTMODE.CC;
            else
                return OUTPUTMODE.UR;
        }
        /// <summary>
        /// 设置通道电流值
        /// </summary>
        /// <param name="nChannel"></param>
        /// <param name="fValue"></param>
        /// <returns></returns>
        public bool SetCurrentLevel(CHANNEL nChannel, double fValue)
        {
            //[:SOURce[<n>]]:CURRent[:LEVel]
            if (fValue < 0.0f)
                fValue = 0.0f;
            if (fValue > 3.0f)
                fValue = 3.0f;
            string strCmd = "";
            strCmd = string.Format(":SOUR{0}:CURR {1}", nChannel.ToString(), fValue.ToString("F6"));
            return (bool)Excute(strCmd);
        }
        /// <summary>
        /// 获取通道电流值
        /// </summary>
        /// <param name="nChannel"></param>
        /// <returns></returns>
        public double GetCurrentLevel(CHANNEL nChannel)
        {
            //[:SOURce[<n>]]:CURRent[:LEVel][:IMMediate][:AMPLitude]? 
            string strCmd = "";
            strCmd = string.Format(":SOUR{0}:CURR?", nChannel.ToString());
            string strRet= Query(strCmd).ToString();
            double fValue = 0.0f;
            if (double.TryParse(strRet, out fValue))
                return fValue;
            return 0.0f;
        }
        /// <summary>
        /// 设置通道电压值
        /// </summary>
        /// <param name="nChannel"></param>
        /// <param name="fValue"></param>
        /// <returns></returns>
        public bool SetVoltLevel(CHANNEL nChannel, double fValue)
        {
            //[:SOURce[<n>]]:VOLTage[:LEVel]
            if (fValue < 0.0f)
                fValue = 0.0f;
            if (fValue > 30.0f)
                fValue = 30.0f;
            string strCmd = "";
            strCmd = string.Format(":SOUR{0}:VOLT {1}", ((int)nChannel).ToString(), fValue.ToString("F6"));
            return (bool)Excute(strCmd);
        }
        /// <summary>
        /// 取得设置的通道电压值
        /// </summary>
        /// <param name="nChannel"></param>
        /// <returns></returns>
        public double GetVoltLevel(CHANNEL nChannel)
        {
            //[:SOURce[<n>]]:VOLTage[:LEVel][:IMMediate][:AMPLitude]? 
            string strCmd = "";
            strCmd = string.Format(":SOUR{0}:VOLT?", nChannel.ToString());
            string strRet = Query(strCmd).ToString();
            double fValue = 0.0f;
            if (double.TryParse(strRet, out fValue))
                return fValue;
            return 0.0f;
        }
        /// <summary>
        /// 设置过保电压和电流
        /// </summary>
        /// <param name="opMode"></param>
        /// <param name="channel"></param>
        /// <param name="fProtValue"></param>
        /// <returns></returns>
        public bool SetProtection(OPMODE opMode,CHANNEL channel, double fProtValue)
        {
            //[:SOURce[<n>]]:CURRent:PROTection[:LEVel]
            string strCmd = "";
            bool bRet = false;
            switch (opMode)
            {
                case OPMODE.OCP:
                    strCmd = string.Format("SOUR{0}:CURR:PROT {1}", (int)channel, fProtValue.ToString("F6"));
                    bRet=(bool)Excute(strCmd);
                    break;
                case OPMODE.OVP:
                    strCmd = string.Format("SOUR{0}:VOLT:PROT {1}", (int)channel, fProtValue.ToString("F6"));
                    bRet = (bool)Excute(strCmd);
                    break;
                default:
                    break;
            }
            return bRet;
        }
        public bool SetOutput(CHANNEL channel, bool bEnable)
        {
            string strCmd = "";
            bool bRet = false;
            strCmd = string.Format(":OUTP {0},{1}", channel.ToString(), bEnable?"ON":"OFF");
            bRet = (bool)Excute(strCmd);
            return bRet;
        }
        public bool GetOutputState(CHANNEL channel)
        {
            string strCmd = "";
           
            strCmd = string.Format(":OUTP? {0}", channel.ToString());
            string strRet = Query(strCmd).ToString();
            return strRet.Contains("ON");
        }
        #endregion
    }
}
