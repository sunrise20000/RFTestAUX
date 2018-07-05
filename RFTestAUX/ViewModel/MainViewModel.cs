using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using RFTestAUX.Config;
using RFTestAUX.Instrument;
using RFTestAUX.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace RFTestAUX.ViewModel
{
  
    public class MainViewModel : ViewModelBase
    {
        private object _msgLock = new object();
        private readonly IDataService _dataService;
        private string _totalError = "";
        private ParaModel _paraModelConfig = null;
        private int _errorCount = 0;
        private CancellationTokenSource cts = null;
        private Task MonitorTask = null;
        private double _realTimeTemperature = 0;
        private double _realTimeSourceLevel = 0;
        private double _realTimeCurrent = 0;
        private DP832 dp832 = null;
        private TC720 tc720=null;

        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                   
                });
            SystemMessageCollection = new ObservableCollection<MessageItem>();
            SystemMessageCollection.CollectionChanged += SystemMessageCollection_CollectionChanged;
            Messenger.Default.Register<string>(this, "ShowError", msg =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    lock (_msgLock)
                    {
                        ShowError(msg);
                    }
                });
            });
            Messenger.Default.Register<string>(this, "ShowInfo", msg =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    lock (_msgLock)
                    {
                        SystemMessageCollection.Add(new MessageItem()
                        {
                            MsgType = EnumMessageType.Info,
                            StrMsg = msg
                        });
                    }
                });
            });
            StrTotalError = "";
            try
            {
                ConfigMgr.Instance.LoadConfig();
                ParaModelConfig = ConfigMgr.Instance.ParaMgr.ParaCfgs[0];
                InitInstrument();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        #region private method
        private void ShowError(string strMsg)
        {
            SystemMessageCollection.Add(new MessageItem()
            {
                MsgType = EnumMessageType.Error,
                StrMsg = strMsg
            });
        }
        private void SystemMessageCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lock (_msgLock)
            {
                if (SystemMessageCollection.Count > 20)
                    SystemMessageCollection.RemoveAt(0);
                ErrorCount = SystemMessageCollection.Count();
                var collect = from msg in SystemMessageCollection where msg.MsgType == EnumMessageType.Error select msg;
                if (collect != null)
                {
                    StrTotalError = $"Infomation ({collect.Count()}) Error";
                }
            }
        }
        private bool InitInstrument()
        {
            dp832 = InstrumentMgr.Instance.FindInstrumentByName("DP832[0]") as DP832;
            tc720= InstrumentMgr.Instance.FindInstrumentByName("TC720[0]") as TC720;
            if (dp832 != null)
            {
                dp832.SetOutput(DP832.CHANNEL.CH3, false);
                Thread.Sleep(100);
                dp832.SetOutput(DP832.CHANNEL.CH2, false);
                Thread.Sleep(100);
                dp832.SetOutput(DP832.CHANNEL.CH1, false);
                Thread.Sleep(100);

                //设置过流保护
                dp832.SetProtection(DP832.OPMODE.OCP, DP832.CHANNEL.CH1, ParaModelConfig.CMPL);
                Thread.Sleep(50);
                dp832.SetProtection(DP832.OPMODE.OVP, DP832.CHANNEL.CH1, 3.4);
            }


            return dp832 != null && tc720 != null ;
        }
        
        #endregion

        #region Property
        public string StrTotalError
        {
            get { return _totalError; }

            set {
                if (_totalError != value)
                {
                    _totalError = value;
                    RaisePropertyChanged();
                }
            }

        }
        public ObservableCollection<MessageItem> SystemMessageCollection { get; set; }
        public ParaModel ParaModelConfig
        {
            get { return _paraModelConfig; }
            set
            {
                if (_paraModelConfig != value)
                {
                    _paraModelConfig = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int ErrorCount
        {
            get { return _errorCount; }
            set
            {
                if (_errorCount != value)
                {
                    _errorCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        public double RealTimeTemperature
        {
            get { return _realTimeTemperature; }
            set
            {
                if (_realTimeTemperature != value)
                {
                    _realTimeTemperature = value;
                    RaisePropertyChanged();
                }
            }
        }
        public double RealTimeSourceLevel
        {
            get { return _realTimeSourceLevel; }
            set
            {
                if (_realTimeSourceLevel != value)
                {
                    _realTimeSourceLevel = value;
                    RaisePropertyChanged();
                }
            }
        }
        public double RealTimeCurrent
        {
            get { return _realTimeCurrent; }
            set
            {
                if (_realTimeCurrent != value)
                {
                    _realTimeCurrent = value;
                    RaisePropertyChanged();
                }
            }
        }
       

        #endregion

        #region >>>>Command
        public RelayCommand<bool> OpenSourceCommand
        {
            get
            {
                return new RelayCommand<bool>(bChecked =>
                {
                    if (!(dp832 != null && dp832.SetOutput(DP832.CHANNEL.CH1, bChecked)))
                    {
                        ShowError(string.Format("{0}通道{1}失败", bChecked ? "打开" : "关闭", DP832.CHANNEL.CH1.ToString()));
                    }
                });
            }
        }
        public RelayCommand<string> ParaOperateCommand
        {
            get
            {
                return new RelayCommand<string>(cmd =>
                {
                    string[] para = cmd.Split('&');
                    try
                    {
                        switch (para[0])
                        {
                            case "Apply":
                                ParaModelConfig.Temperature = double.Parse(para[1]);
                                ParaModelConfig.SourceLevel = double.Parse(para[2]);
                                ParaModelConfig.CMPL = double.Parse(para[3]);
                                if (dp832 != null)
                                {
                                    dp832.SetVoltLevel(DP832.CHANNEL.CH1, ParaModelConfig.SourceLevel);
                                    dp832.SetProtection(DP832.OPMODE.OCP, DP832.CHANNEL.CH1, ParaModelConfig.CMPL);
                                }
                                if (tc720 != null)
                                    tc720.WriteTemperature(Channel.CH1, ParaModelConfig.Temperature);
                                break;
                            case "Save":
                                ConfigMgr.Instance.SaveConfig(EnumConfigType.ParaCfg, new ParaModel[] { ParaModelConfig });
                                break;
                            default:
                                break;
                        }
                    }
                    catch(Exception ex)
                    {
                        Messenger.Default.Send<string>($"{para[0]}的时候发生错误:{ex.Message}","ShowError");
                    }
                });
            }
        }
        public RelayCommand WindowLoadedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        if (MonitorTask == null || MonitorTask.IsCanceled || MonitorTask.IsCompleted)
                        {
                            cts = new CancellationTokenSource();
                            MonitorTask = new Task(() =>
                            {
                                while (!cts.Token.IsCancellationRequested)
                                {
                                    Thread.Sleep(100);
                                    if(tc720 != null)
                                        RealTimeTemperature = tc720.ReadTemperature(Channel.CH1);
                                    if (dp832 != null)
                                    {
                                        RealTimeSourceLevel = dp832.GetVoltLevel(DP832.CHANNEL.CH1);
                                        dp832.Fetch(DP832.CHANNEL.CH1);
                                        RealTimeCurrent = dp832.MeasureValue[1];
                                    }
                                }
                                    
                            }, cts.Token);
                        }
                        MonitorTask.Start();
                        
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                });
            }
        }
        public RelayCommand ClearMessageCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        lock (_msgLock)
                        {
                            SystemMessageCollection.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                });
            }
        }
        
        #endregion

    }
}