using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using RFTestAUX.Communication;
using RFTestAUX.Instrument;
using RFTestAUX.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RFTestAUX.Config
{
    public enum EnumConfigType
    {
        ParaCfg
    }
    public class ConfigMgr
    {
        private ConfigMgr()
        {

        }
        private static readonly Lazy<ConfigMgr> _instance = new Lazy<ConfigMgr>(() => new ConfigMgr());
        public static ConfigMgr Instance
        {
            get { return _instance.Value; }
        }


        private readonly string File_HardwareCfg = Directory.GetCurrentDirectory()+ "\\Config\\HardwareCfg.json";
        private readonly string File_ParaCfg = Directory.GetCurrentDirectory() + "\\Config\\ParaCfg.json";
        public HardwareCfgManager HardwareCfgMgr = null;
        public ParaManager ParaMgr = null;
        public void LoadConfig()
        {
            #region >>>>Hardware init
            try
            {
                var json_string = File.ReadAllText(File_HardwareCfg);
                HardwareCfgMgr = JsonConvert.DeserializeObject<HardwareCfgManager>(json_string);
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>($"Unable to load config file { File_HardwareCfg }:{ ex.Message}", "ShowError");
            }
            InstrumentBase inst = null;
            HardwareCfgLevelManager1[] instCfgs = null;

            string strClassName = "";
            Type t = HardwareCfgMgr.GetType();
            PropertyInfo[] PropertyInfos = t.GetProperties();
            for (int i = 0; i < PropertyInfos.Length; i++)
            {
                if (PropertyInfos[i].Name.ToUpper().Contains("COMPORT") || PropertyInfos[i].Name.ToUpper().Contains("ETHERNET") ||
                    PropertyInfos[i].Name.ToUpper().Contains("GPIB") || PropertyInfos[i].Name.ToUpper().Contains("NIVISA") ||
                     PropertyInfos[i].Name.ToUpper().Contains("CAMERACFG"))
                    continue;
                PropertyInfo pi = PropertyInfos[i];
                instCfgs = pi.GetValue(HardwareCfgMgr) as HardwareCfgLevelManager1[];
                strClassName = pi.Name.Substring(0, pi.Name.Length - 1);
               
                foreach (var it in instCfgs)
                {
                    if (!it.Enabled)
                        continue;
                    inst = t.Assembly.CreateInstance("RFTestAUX.Instrument." + strClassName, true, BindingFlags.CreateInstance, null, new object[] { it }, null, null) as InstrumentBase;
                    if (inst != null && it.Enabled)
                    {
                        if (inst.Init())
                            InstrumentMgr.Instance.AddInstrument(it.InstrumentName, inst);
                        else
                        {
                            Messenger.Default.Send<string>($"{it.InstrumentName} init Error","ShowError");
                            
                        } 
                    }
                } 
            }
            //if (sbError.ToString().Length > 5)
            //    throw new Exception($"Instrument :{sbError.ToString()} init failed");
            #endregion

            #region >>>> Software Init
            try
            {
                var json_string = File.ReadAllText(File_ParaCfg);
                ParaMgr = JsonConvert.DeserializeObject<ParaManager>(json_string);
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>($"Unable to load config file { File_ParaCfg }:{ ex.Message}", "ShowError");
            }
            #endregion


        }
        public void SaveConfig(EnumConfigType cfgType, object[] listObj)
        {
            if (listObj == null)
                throw new Exception(string.Format("保存的{0}数据为空", cfgType.ToString()));
            string fileSaved = null;
            object objSaved = null;
            switch (cfgType)
            {
                case EnumConfigType.ParaCfg:
                    fileSaved = File_ParaCfg;
                    objSaved = new ParaManager();
                    (objSaved as ParaManager).ParaCfgs = listObj as ParaModel[];
                    break;
                default:
                    break;
            }
            string json_str = JsonConvert.SerializeObject(objSaved);
            File.WriteAllText(fileSaved, json_str);
        }
    }
}
