using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RFTestAUX.ValidateRules
{
    public class MustBeDouble : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value.ToString().Trim() == "")
                return new ValidationResult(false, "数值不能为空");
            else if (double.TryParse(value.ToString(), out double x))
                return new ValidationResult(true, null);
            else
                return new ValidationResult(false, "输入正确的数值");
        }
    }
}
