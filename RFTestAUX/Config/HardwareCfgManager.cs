using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFTestAUX.Communication
{
    public class HardwareCfgManager
    {
        public DP832Config[] DP832s { get; set; }
        public TC720Config[] TC720s { get; set; }

        public ComportCfg[] Comports { get; set; }
        public EtherNetCfg[] EtherNets { get; set; }
        public NIVasaCfg[] NIVisas { get; set; }
    }
}
