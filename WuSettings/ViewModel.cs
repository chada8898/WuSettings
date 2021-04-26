﻿using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using WuSettings.Properties;

namespace WuSettings
{
	public class ViewModel : NotifyPropertyChanged
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
					SendPropertyChanged(nameof(DeferQualityUpdates));
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
					SendPropertyChanged(nameof(ActiveHoursStart));
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
					SendPropertyChanged(nameof(ActiveHoursEnd));
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
					SendPropertyChanged(nameof(DownloadOnly));
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
					SendPropertyChanged(nameof(ExcludeDriverUpdates));
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
					SendPropertyChanged(nameof(UpdateOtherMsProducts));
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

		public void Initialize()
		{
			try
			{
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate"))
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

				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings"))
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

				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU"))
				{
					downloadOnly = (key?.GetValue("AUOptions") as int? == 2);
					noAutoUpdate = key?.GetValue("NoAutoUpdate") as int? ?? -1;
					updateOtherMsProducts = (key?.GetValue("AllowMUUpdateService") as int? > 0);
				}

				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
				{
					string productName = key?.GetValue("ProductName") as string;
					isWindows10HomeVersion = productName?.Contains(" Home") ?? false;
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

		private void SetRegistryValue(string subkey, string name, object value, RegistryValueKind valueKind)
		{
			try
			{
				using (RegistryKey key = Registry.LocalMachine.CreateSubKey(subkey))
				{
					key?.SetValue(name, value, valueKind);
				}
			}
			catch (Exception e)
			{
				//MessageBox.Show(e.GetType().Name + ": " + e.Message);
				throw new GPRegException(e.GetType().Name + ": " + e.Message);
			}
		}

		private void SetGroupPolicy(string key, string name, int dwordValue)
		{
			if (isWindows10HomeVersion)
			{
				SetRegistryValue(key, name, dwordValue, RegistryValueKind.DWord);
			}

			Task.Delay(10);
			bool success = localGroupPolicy.SetGroupPolicy(key, name, null, dwordValue);
			if (!success) throw new GPRegException(Resources.ErrorSettingGpoObject);
		}

		private void SetGroupPolicy(string key, string name, string stringValue)
		{
			if (isWindows10HomeVersion)
			{
				if (stringValue == null) stringValue = string.Empty;
				SetRegistryValue(key, name, stringValue, RegistryValueKind.String);
			}

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
