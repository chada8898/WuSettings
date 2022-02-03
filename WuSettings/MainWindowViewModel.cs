using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using WuSettings.Properties;

namespace WuSettings
{
	public class MainWindowViewModel : NotifyPropertyChanged
	{
		private readonly LocalGroupPolicy localGroupPolicy = new LocalGroupPolicy();

		private int branchReadinessLevel;
		private int managePreviewBuilds;
		private int noAutoUpdate;
		private bool isWindows10HomeVersion;

		private string deferFeatureUpdates;
		public string DeferFeatureUpdates
		{
			get => deferFeatureUpdates;
			set
			{
				int.TryParse(value, out int ival);
				if (SaveDeferFeatureUpdates(ival))
				{
					SetProperty(ref deferFeatureUpdates, value);
				}
				else
				{
					SendPropertyChanged();
				}
			}
		}

		private string deferQualityUpdates;
		public string DeferQualityUpdates
		{
			get => deferQualityUpdates;
			set
			{
				int.TryParse(value, out int ival);
				if (SaveDeferQualityUpdates(ival))
				{
					SetProperty(ref deferQualityUpdates, value);
				}
				else
				{
					SendPropertyChanged();
				}
			}
		}

		private string activeHoursStart;
		public string ActiveHoursStart
		{
			get => activeHoursStart;
			set
			{
				int.TryParse(value, out int ival);
				if (SaveActiveHoursStart(ival))
				{
					SetProperty(ref activeHoursStart, value);
				}
				else
				{
					SendPropertyChanged();
				}
			}
		}

		private string activeHoursEnd;
		public string ActiveHoursEnd
		{
			get => activeHoursEnd;
			set
			{
				int.TryParse(value, out int ival);
				if (SaveActiveHoursEnd(ival))
				{
					SetProperty(ref activeHoursEnd, value);
				}
				else
				{
					SendPropertyChanged();
				}
			}
		}

		private bool downloadOnly;
		public bool DownloadOnly
		{
			get => downloadOnly;
			set
			{
				if (SaveDownloadOnly(value))
				{
					SetProperty(ref downloadOnly, value);
				}
				else
				{
					SendPropertyChanged();
				}
			}
		}

		private bool excludeDriverUpdates;
		public bool ExcludeDriverUpdates
		{
			get => excludeDriverUpdates;
			set
			{
				if (SaveExcludeDriverUpdates(value))
				{
					SetProperty(ref excludeDriverUpdates, value);
				}
				else
				{
					SendPropertyChanged();
				}
			}
		}

		private bool updateOtherMsProducts;
		public bool UpdateOtherMsProducts
		{
			get => updateOtherMsProducts;
			set
			{
				if (SaveUpdateOtherMsProducts(value))
				{
					SetProperty(ref updateOtherMsProducts, value);
				}
				else
				{
					SendPropertyChanged();
				}
			}
		}

		private string targetReleaseVersionInfo;
		public string TargetReleaseVersionInfo
		{
			get => targetReleaseVersionInfo;
			set
			{
				if (SaveTargetReleaseVersionInfo(value))
				{
					SetProperty(ref targetReleaseVersionInfo, value);
				}
				else
				{
					SendPropertyChanged();
				}
			}
		}

		private bool disableNewsFeeds;
		public bool DisableNewsFeeds
		{
			get => disableNewsFeeds;
			set
			{
				if (SaveDisableNewsFeeds(value))
				{
					SetProperty(ref disableNewsFeeds, value);
				}
				else
				{
					SendPropertyChanged();
				}
			}
		}

		private bool disableWindowsSuggestions;
		public bool DisableWindowsSuggestions
		{
			get => disableWindowsSuggestions;
			set
			{
				if (SaveDisableWindowsSuggestions(value))
				{
					SetProperty(ref disableWindowsSuggestions, value);
				}
				else
				{
					SendPropertyChanged();
				}
			}
		}

		public void Initialize()
		{
			try
			{
				using (var regLocalMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) ??
											 RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
				{
					using (RegistryKey key = regLocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate"))
					{
						if (key?.GetValue("DeferFeatureUpdates") as int? > 0)
						{
							deferFeatureUpdates = key.GetValue("DeferFeatureUpdatesPeriodInDays")?.ToString();
						}

						if (key?.GetValue("DeferQualityUpdates") as int? > 0)
						{
							deferQualityUpdates = key.GetValue("DeferQualityUpdatesPeriodInDays")?.ToString();
						}

						excludeDriverUpdates = key?.GetValue("ExcludeWUDriversInQualityUpdate") as int? > 0;
						branchReadinessLevel = key?.GetValue("BranchReadinessLevel") as int? ?? 0;
						managePreviewBuilds = key?.GetValue("ManagePreviewBuilds") as int? ?? -1;
						bool isTargetReleaseVersion = key?.GetValue("TargetReleaseVersion") as int? > 0;
						targetReleaseVersionInfo = isTargetReleaseVersion ? key?.GetValue("TargetReleaseVersionInfo") as string : null;
					}

					using (RegistryKey key = regLocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings"))
					{
						activeHoursStart = key?.GetValue("ActiveHoursStart")?.ToString();
						activeHoursEnd = key?.GetValue("ActiveHoursEnd")?.ToString();

						if (string.IsNullOrEmpty(deferFeatureUpdates))
						{
							deferFeatureUpdates = key?.GetValue("DeferFeatureUpdatesPeriodInDays")?.ToString();
						}

						if (string.IsNullOrEmpty(deferQualityUpdates))
						{
							deferQualityUpdates = key?.GetValue("DeferQualityUpdatesPeriodInDays")?.ToString();
						}
					}

					using (RegistryKey key = regLocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU"))
					{
						downloadOnly = (key?.GetValue("AUOptions") as int? == 2);
						noAutoUpdate = key?.GetValue("NoAutoUpdate") as int? ?? -1;
						updateOtherMsProducts = (key?.GetValue("AllowMUUpdateService") as int? > 0);
					}

					using (RegistryKey key = regLocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
					{
						string productName = key?.GetValue("ProductName") as string;
						isWindows10HomeVersion = productName?.Contains(" Home") ?? false;
					}

					using (RegistryKey key = regLocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Windows Feeds"))
					{
						bool enableFeeds = (key?.GetValue("EnableFeeds") as int? ?? 1) > 0;
						disableNewsFeeds = !enableFeeds;
					}
				}

				using (var regCurrentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64) ??
											RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
				{
					using (RegistryKey key = regCurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\UserProfileEngagement"))
					{
						bool enableSuggestions = (key?.GetValue("ScoobeSystemSettingEnabled") as int? ?? 0) > 0;
						disableWindowsSuggestions = !enableSuggestions;
					}
				}

				SetHiddenProperties();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.GetType().Name + ": " + e.Message);
			}
		}

		private bool SaveDeferFeatureUpdates(int deferFeatureUpdates)
		{
			if (deferFeatureUpdates < 0) deferFeatureUpdates = 0;

			try
			{
				SetRegistryValue(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "DeferFeatureUpdatesPeriodInDays", deferFeatureUpdates, RegistryValueKind.DWord);

				SetGroupPolicy(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "DeferFeatureUpdates", (deferFeatureUpdates > 0) ? 1 : 0);
				SetGroupPolicy(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "DeferFeatureUpdatesPeriodInDays", deferFeatureUpdates);
			}
			catch (GPRegException e)
			{
				MessageBox.Show(e.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return false;
			}

			return true;
		}

		private bool SaveDeferQualityUpdates(int deferQualityUpdates)
		{
			if (deferQualityUpdates < 0) deferQualityUpdates = 0;

			try
			{
				SetRegistryValue(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "DeferQualityUpdatesPeriodInDays", deferQualityUpdates, RegistryValueKind.DWord);

				SetGroupPolicy(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "DeferQualityUpdates", (deferQualityUpdates > 0) ? 1 : 0);
				SetGroupPolicy(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "DeferQualityUpdatesPeriodInDays", deferQualityUpdates);
			}
			catch (GPRegException e)
			{
				MessageBox.Show(e.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return false;
			}

			return true;
		}

		private bool SaveActiveHoursStart(int activeHoursStart)
		{
			if (activeHoursStart < 0) activeHoursStart = 0;

			try
			{
				SetRegistryValue(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "ActiveHoursStart", activeHoursStart, RegistryValueKind.DWord);
			}
			catch (GPRegException e)
			{
				MessageBox.Show(e.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return false;
			}

			return true;
		}

		private bool SaveActiveHoursEnd(int activeHoursEnd)
		{
			if (activeHoursEnd < 0) activeHoursEnd = 0;

			try
			{
				SetRegistryValue(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "ActiveHoursEnd", activeHoursEnd, RegistryValueKind.DWord);
			}
			catch (GPRegException e)
			{
				MessageBox.Show(e.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return false;
			}

			return true;
		}

		private bool SaveDownloadOnly(bool downloadOnly)
		{
			try
			{
				SetGroupPolicy(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions", downloadOnly ? 2 : 0);
			}
			catch (GPRegException e)
			{
				MessageBox.Show(e.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return false;
			}

			return true;
		}

		private bool SaveExcludeDriverUpdates(bool excludeDriverUpdates)
		{
			int exDriverUpdates = Convert.ToInt32(excludeDriverUpdates);

			try
			{
				SetRegistryValue(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "ExcludeWUDriversInQualityUpdate", exDriverUpdates, RegistryValueKind.DWord);
				SetGroupPolicy(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", exDriverUpdates);
			}
			catch (GPRegException e)
			{
				MessageBox.Show(e.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return false;
			}

			return true;
		}

		private bool SaveUpdateOtherMsProducts(bool updateOtherMsProducts)
		{
			try
			{
				SetGroupPolicy(@"Software\Policies\Microsoft\Windows\WindowsUpdate\AU", "AllowMUUpdateService", Convert.ToInt32(updateOtherMsProducts));
			}
			catch (GPRegException e)
			{
				MessageBox.Show(e.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return false;
			}

			return true;
		}

		private bool SaveTargetReleaseVersionInfo(string targetReleaseVersionInfo)
		{
			bool isVersionTargeted = !string.IsNullOrEmpty(targetReleaseVersionInfo);

			try
			{
				SetGroupPolicy(@"Software\Policies\Microsoft\Windows\WindowsUpdate", "TargetReleaseVersion", Convert.ToInt32(isVersionTargeted));
				SetGroupPolicy(@"Software\Policies\Microsoft\Windows\WindowsUpdate", "TargetReleaseVersionInfo", targetReleaseVersionInfo);
			}
			catch (GPRegException e)
			{
				MessageBox.Show(e.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return false;
			}

			return true;
		}

		private bool SaveDisableNewsFeeds(bool disableNewsFeeds)
		{
			try
			{
				SetGroupPolicy(@"Software\Policies\Microsoft\Windows\Windows Feeds", "EnableFeeds", Convert.ToInt32(!disableNewsFeeds));
			}
			catch (GPRegException e)
			{
				MessageBox.Show(e.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return false;
			}

			return true;
		}

		private bool SaveDisableWindowsSuggestions(bool disableSuggestions)
		{
			int enableSuggestions = Convert.ToInt32(!disableSuggestions);

			try
			{
				SetRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement", "ScoobeSystemSettingEnabled",
								 enableSuggestions, RegistryValueKind.DWord, RegistryHive.CurrentUser);
				SetRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled",
								 enableSuggestions, RegistryValueKind.DWord, RegistryHive.CurrentUser);
				SetRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled",
								 enableSuggestions, RegistryValueKind.DWord, RegistryHive.CurrentUser);
				SetRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled",
								 enableSuggestions, RegistryValueKind.DWord, RegistryHive.CurrentUser);
			}
			catch (GPRegException e)
			{
				MessageBox.Show(e.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return false;
			}

            return true;
		}

		private void SetRegistryValue(string subkey, string name, object value, RegistryValueKind valueKind, RegistryHive hive = RegistryHive.LocalMachine)
		{
			try
			{
				using (var regBaseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64) ??
										RegistryKey.OpenBaseKey(hive, RegistryView.Registry32))
				{
					using (RegistryKey key = regBaseKey.CreateSubKey(subkey))
					{
						key?.SetValue(name, value, valueKind);
					}
				}
			}
			catch (Exception e)
			{
				throw new GPRegException(e.GetType().Name + ": " + e.Message);
			}
		}

		private void SetGroupPolicy(string key, string name, int dwordValue)
		{
			SetRegistryValue(key, name, dwordValue, RegistryValueKind.DWord);
			if (isWindows10HomeVersion) return;

			Task.Delay(10);
			bool success = localGroupPolicy.SetGroupPolicy(key, name, null, dwordValue);
			if (!success) throw new GPRegException(Resources.ErrorSettingGpoObject);
		}

		private void SetGroupPolicy(string key, string name, string stringValue)
		{
			if (stringValue == null) stringValue = string.Empty;
			SetRegistryValue(key, name, stringValue, RegistryValueKind.String);
			if (isWindows10HomeVersion) return;

			Task.Delay(10);
			bool success = localGroupPolicy.SetGroupPolicy(key, name, stringValue, null);
			if (!success) throw new GPRegException(Resources.ErrorSettingGpoObject);
		}

		private void SetHiddenProperties()
		{
			try
			{
				if (noAutoUpdate != 0)
				{
					noAutoUpdate = 0;
					SetGroupPolicy(@"Software\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", noAutoUpdate);
				}

				if (branchReadinessLevel != 0x10)
				{
					branchReadinessLevel = 0x10;
					SetGroupPolicy(@"Software\Policies\Microsoft\Windows\WindowsUpdate", "BranchReadinessLevel", branchReadinessLevel);
				}

				if (managePreviewBuilds != 0)
				{
					managePreviewBuilds = 0;
					SetGroupPolicy(@"Software\Policies\Microsoft\Windows\WindowsUpdate", "ManagePreviewBuilds", managePreviewBuilds);
				}

				// Do not allow Microsoft PC Health Check to install
				SetRegistryValue(@"SOFTWARE\Microsoft\PCHC", "PreviousUninstall", 1, RegistryValueKind.DWord);
			}
			catch (GPRegException e)
			{
				MessageBox.Show(e.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
			}
		}
	}

	public class GPRegException : Exception
	{
		public GPRegException(string message) : base(message)
		{
		}
	}


}
