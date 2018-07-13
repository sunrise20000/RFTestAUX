using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFTestAUX.Model
{
    public class ParaModel  : INotifyPropertyChanged
    {
        private double _temperature = 0.0;
        public double Temperature
        {
            get { return _temperature; }
            set
            {
                if (_temperature != value)
                {
                    _temperature = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Temperature"));
                }
            }
        }
        public double SourceLevel
        {
            get;
            set;
        }
        public double CMPL
        {
            set;
            get;
        }
        public double TemperatureBand
        {
            get;
            set;      
        }
        public int StabilizationTime
        {
            get;
            set;   
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
