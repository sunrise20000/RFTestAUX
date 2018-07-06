using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFTestAUX.Model
{
    public class ParaModel
    {
        public double Temperature
        {
            get;
            set;
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
    }
}
