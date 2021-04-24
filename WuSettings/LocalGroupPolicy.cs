using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Text;

// This source came from the following web example:
// https://bitbandit.org/20190622/configure-wsus-gpo-programmatically/
//
// and then was modified to allow adding DWORD registry values
// and to allow creating registry keys instead of just opening them.
//

namespace WuSettings
{
	public class LocalGroupPolicy
	{
		public bool SetGroupPolicy(string subKey, string keyName, string stringValue, int? dwordValue)
		{
			var gpo = new GroupPolicyClass();
			IGroupPolicyObject igpo = (IGroupPolicyObject)gpo;

			try
			{
				var hr = igpo.OpenLocalMachineGPO(GPO_OPEN_LOAD_REGISTRY);
				if (hr != 0) return false;

				hr = igpo.GetRegistryKey(GPO_SECTION_MACHINE, out UIntPtr keyComputer);
				if (hr != 0) return false;

				if (RegCreateKeyExW(keyComputer, subKey, 0, null, RegOption.NonVolatile, RegSAM.SetValue, IntPtr.Zero, out UIntPtr keyptr, IntPtr.Zero) == 0)
				{
					if (stringValue != null)
					{
						IntPtr bstr = Marshal.StringToBSTR(stringValue);
						int cbData = Encoding.Unicode.GetByteCount(stringValue) + 2;
						if (RegSetValueExW(keyptr, keyName, 0, RegistryValueKind.String, bstr, cbData) != 0) return false;
						Marshal.FreeBSTR(bstr);
					}
					else
					{
						if (dwordValue != null)
						{
							IntPtr intPtr = Marshal.AllocHGlobal(sizeof(int));
							Marshal.WriteInt32(intPtr, (int)dwordValue);
							if (RegSetValueExW(keyptr, keyName, 0, RegistryValueKind.DWord, intPtr, sizeof(int)) != 0) return false;
							Marshal.FreeHGlobal(intPtr);
						}
						else
						{
							RegDeleteValueW(keyptr, keyName);
						}
					}

					RegCloseKey(keyptr);
				}

				RegCloseKey(keyComputer);

				hr = igpo.Save(true, true, REGISTRY_EXTENSION_GUID, CLSID_PolicySnapinUser);
				return (hr == 0);
			}
			catch
			{
				return false;
			}
		}

		static readonly Guid REGISTRY_EXTENSION_GUID = new Guid("35378EAC-683F-11D2-A89A-00C04FBBCFA2");
		static readonly Guid CLSID_PolicySnapinUser = new Guid("0F6B957E-509E-11D1-A7CC-0000F87571E3");
		public const uint GPO_OPEN_LOAD_REGISTRY = 0x00000001;
		public const uint GPO_OPEN_READ_ONLY = 0x00000002;
		public const uint GPO_SECTION_ROOT = 0;
		public const uint GPO_SECTION_USER = 1;
		public const uint GPO_SECTION_MACHINE = 2;

		[ComImport, Guid("EA502722-A23D-11d1-A7D3-0000F87571E3")]
		public class GroupPolicyClass
		{
		}

		[ComImport, Guid("EA502723-A23D-11d1-A7D3-0000F87571E3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IGroupPolicyObject
		{
			uint New(
			  [MarshalAs(UnmanagedType.LPWStr)] string domainName,
			  [MarshalAs(UnmanagedType.LPWStr)] string displayName,
			  uint flags);

			uint OpenDSGPO(
			  [MarshalAs(UnmanagedType.LPWStr)] string path,
			  uint flags);

			uint OpenLocalMachineGPO(
			  uint flags);

			uint OpenRemoteMachineGPO(
			  [MarshalAs(UnmanagedType.LPWStr)] string computerName,
			  uint flags);

			uint Save(
			  [MarshalAs(UnmanagedType.Bool)] bool machine,
			  [MarshalAs(UnmanagedType.Bool)] bool add,
			  [MarshalAs(UnmanagedType.LPStruct)] Guid extension,
			  [MarshalAs(UnmanagedType.LPStruct)] Guid app);

			uint Delete();

			uint GetName(
			  [MarshalAs(UnmanagedType.LPWStr)] StringBuilder name,
			  int maxLength);

			uint GetDisplayName(
			  [MarshalAs(UnmanagedType.LPWStr)] StringBuilder name,
			  int maxLength);

			uint SetDisplayName(
			  [MarshalAs(UnmanagedType.LPWStr)] string name);

			uint GetPath(
			  [MarshalAs(UnmanagedType.LPWStr)] StringBuilder path,
			  int maxPath);

			uint GetDSPath(
			  uint section,
			  [MarshalAs(UnmanagedType.LPWStr)] StringBuilder path,
			  int maxPath);

			uint GetFileSysPath(
			  uint section,
			  [MarshalAs(UnmanagedType.LPWStr)] StringBuilder path,
			  int maxPath);

			uint GetRegistryKey(
			  uint section,
			  out UIntPtr key);

			uint GetOptions();

			uint SetOptions(
			  uint options,
			  uint mask);

			uint GetType(
			  out IntPtr gpoType);

			uint GetMachineName(
			  [MarshalAs(UnmanagedType.LPWStr)] StringBuilder name,
			  int maxLength);

			uint GetPropertySheetPages(
			  out IntPtr pages);
		}

		[DllImport("advapi32.dll", SetLastError = true)]
		static extern int RegCloseKey(
		  UIntPtr hKey);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern int RegOpenKeyExW(
		  UIntPtr hKey,
		  [MarshalAs(UnmanagedType.LPWStr)] string subKey,
		  int ulOptions,
		  RegSAM samDesired,
		  out UIntPtr hkResult);

		/// <summary>
		/// Creates the specified registry key. If the key already exists, the function opens it. Note that key names are not case sensitive.
		/// </summary>
		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern int RegCreateKeyExW(
		  UIntPtr hKey,
		  [MarshalAs(UnmanagedType.LPWStr)] string subKey,
		  UInt32 Reserved,
		  String lpClass,
		  RegOption regOption,
		  RegSAM samDesired,
		  IntPtr lpSecurityAttributes,
		  out UIntPtr hkResult,
		  IntPtr lpdwDisposition);

		[DllImport("advapi32.dll", SetLastError = true)]
		static extern int RegSetValueExW(
		  UIntPtr hKey,
		  [MarshalAs(UnmanagedType.LPWStr)] string lpValueName,
		  int Reserved,
		  RegistryValueKind dwType,
		  IntPtr lpData,
		  int cbData);

		[DllImport("advapi32.dll", SetLastError = true)]
		static extern int RegDeleteValueW(
		  UIntPtr hKey,
		  [MarshalAs(UnmanagedType.LPWStr)] string lpValueName);

		[Flags]
		public enum RegSAM
		{
			QueryValue = 0x00000001,
			SetValue = 0x00000002,
			CreateSubKey = 0x00000004,
			EnumerateSubKeys = 0x00000008,
			Notify = 0x00000010,
			CreateLink = 0x00000020,
			WOW64_32Key = 0x00000200,
			WOW64_64Key = 0x00000100,
			WOW64_Res = 0x00000300,
			Read = 0x00020019,
			Write = 0x00020006,
			Execute = 0x00020019,
			AllAccess = 0x000F003F
		}

		[Flags]
		public enum RegOption
		{
			NonVolatile = 0x0,
			Volatile = 0x1,
			CreateLink = 0x2,
			BackupRestore = 0x4,
			OpenLink = 0x8
		}

		[Flags]
		public enum RegResult
		{
			CreatedNewKey = 0x00000001,
			OpenedExistingKey = 0x00000002
		}
	}
}
