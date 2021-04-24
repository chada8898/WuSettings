using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace WuSettings
{
	public class DeferFeatureUpdatesValidator : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			int ival;
			int.TryParse(value as string, out ival);
			return (ival > 365) ?
					new ValidationResult(false, "Defer Feature Updates range is 0 - 365 days") :
					ValidationResult.ValidResult;
		}
	}

	public class DeferQualityUpdatesValidator : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			int ival;
			int.TryParse(value as string, out ival);
			return (ival > 30) ?
					new ValidationResult(false, "Defer Quality Updates range is 0 - 30 days") :
					ValidationResult.ValidResult;
		}
	}

	public class ActiveHoursValidator : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			int ival;
			int.TryParse(value as string, out ival);
			return (ival > 23) ?
					new ValidationResult(false, "Active Hours range is 0 - 23 hours") :
					ValidationResult.ValidResult;
		}
	}

	public class TargetReleaseVersionInfoValidator : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			string str = value as string;
			var result = ValidationResult.ValidResult;
			Regex regex = new Regex("^[A-Z0-9]*$");

			if (!string.IsNullOrEmpty(str) && !regex.IsMatch(str))
			{
				result = new ValidationResult(false, "Target version info must contain letters and numbers only");
			}

			return result;
		}
	}

}
