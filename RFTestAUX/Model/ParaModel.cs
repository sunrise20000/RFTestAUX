using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFTestAUX.Model
{
    public class ParaModel : INotifyPropertyChanged
    {
        private double _temperature = -5;
        private double _sourceLevel = 3.3;
        private double _cmpl = 0.3;
        public double Temperature
        {
            set
            {
                if (_temperature != value)
                {
                    _temperature = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Temperature"));
                }
            }
            get
            {
                return _temperature;
            }
        }
        public double SourceLevel
        {
            set
            {
                if (_sourceLevel != value)
                {
                    _sourceLevel = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SourceLevel"));
                }
            }
            get
            {
                return _sourceLevel;
            }
        }
        public double CMPL
        {
            set
            {
                if (_cmpl != value)
                {
                    _cmpl = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CMPL"));
                }
            }
            get
            {
                return _cmpl;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
