using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using CPAS.Config.HardwareManager;
using CPAS.Config;
using NationalInstruments.VisaNS;

namespace CPAS.Instrument
{
    public enum EnumUnit
    {
        W,
        mW,
        μW
    }
    public class PowerMeter : InstrumentBase
    {
        private byte[] byteRecv = new byte[64];
        ComportCfg comportCfg = null;
        NIVasaCfg nivisaCfg = null;
        public double[] MeasureValue=new double[4] { 0.0f,0.0f,0.0f,0.0f};
        MessageBasedSession session = null;
 
        public PowerMeter(HardwareCfgLevelManager1 cfg) : base(cfg)
        {
        }
        public override bool Init()
        {
            try
            {
                HardwareCfgManager hardwareCfg = ConfigMgr.HardwareCfgMgr;
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
                else if (Config.ConnectMode.ToUpper() == @"NIVISA")
                {
                    foreach (var it in hardwareCfg.NIVisas)
                    {
                        if (it.PortName == Config.PortName)
                            nivisaCfg = it;
                    }
                    if (nivisaCfg != null)
                    {
                        string[] resources = ResourceManager.GetLocalManager().FindResources(nivisaCfg.KeyWord1);
                        foreach (var res in resources)
                        {
                            if (res.Contains(nivisaCfg.KeyWord2))
                            {
                                session = ResourceManager.GetLocalManager().Open(resources[0].ToString()) as MessageBasedSession;
                            }
                        }
                        string str = session.Query("READ?");
                        return session != null;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public override bool DeInit()
        {
            if (comPort != null)
            {
                comPort.Close();
                comPort.Dispose();
            }
            if (session != null)
            {
                session.Clear();
            }
            return true;
        }
        public  object Excute(object objCmd)
        {
            try
            {
                lock (_lock)
                {
                    session.Write(objCmd.ToString());
                    return true;
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
                lock (_lock)
                {
                    return session.Query(objCmd.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public double GetPowerValue(EnumUnit unit)
        {

            string strValue=Query("READ?").ToString();
            if (double.TryParse(strValue, out double value))
            {
                int n = 1;
                switch (unit)
                {
                    case EnumUnit.mW:
                        n = 1000;
                        break;
                    case EnumUnit.μW:
                        n = 1000000;
                            break;
                }
                
                return value*n;
            }
            return 0.0f;
        }
        public void Abort()
        {
            Excute(":ABOR");
        }
    }
}
