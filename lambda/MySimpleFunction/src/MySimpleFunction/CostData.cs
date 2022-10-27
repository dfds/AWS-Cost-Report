using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySimpleFunction
{
    public class CostData
    {
        public CostTypes Type;
        public double Value;
        public string Currency;

        public CostData(CostTypes type, double value, string currency)
        {
            this.Type = type;
            Value = value;
            Currency = currency;
        }

    }

}
