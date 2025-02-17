using System;
using System.Runtime.InteropServices;
using System.Text;

namespace XGRAB.Util.Memory.Structures;

public class Imps
{
	public struct SYSTEM_INFO
	{
		public ushort processorArchitecture;

		private ushort reserved;

		public uint pageSize;

		public UIntPtr minimumApplicationAddress;

		public UIntPtr maximumApplicationAddress;

		public IntPtr activeProcessorMask;

		public uint numberOfProcessors;

		public uint processorType;

		public uint allocationGranularity;

		public ushort processorLevel;

		public ushort processorRevision;
	}

	public struct MEMORY_BASIC_INFORMATION32
	{
		public UIntPtr BaseAddress;

		public UIntPtr AllocationBase;

		public uint AllocationProtect;

		public uint RegionSize;

		public uint State;

		public uint Protect;

		public uint Type;
	}

	public struct MEMORY_BASIC_INFORMATION64
	{
		public UIntPtr BaseAddress;

		public UIntPtr AllocationBase;

		public uint AllocationProtect;

		public uint __alignment1;

		public ulong RegionSize;

		public uint State;

		public uint Protect;

		public uint Type;

		public uint __alignment2;
	}

	public struct MEMORY_BASIC_INFORMATION
	{
		public UIntPtr BaseAddress;

		public UIntPtr AllocationBase;

		public uint AllocationProtect;

		public long RegionSize;

		public uint State;

		public uint Protect;

		public uint Type;
	}

	public const uint MEM_FREE = 65536u;

	public const uint MEM_COMMIT = 4096u;

	public const uint MEM_RESERVE = 8192u;

	public const uint PAGE_READONLY = 2u;

	public const uint PAGE_READWRITE = 4u;

	public const uint PAGE_WRITECOPY = 8u;

	public const uint PAGE_EXECUTE_READWRITE = 64u;

	public const uint PAGE_EXECUTE_WRITECOPY = 128u;

	public const uint PAGE_EXECUTE = 16u;

	public const uint PAGE_EXECUTE_READ = 32u;

	public const uint PAGE_GUARD = 256u;

	public const uint PAGE_NOACCESS = 1u;

	public const uint MEM_PRIVATE = 131072u;

	public const uint MEM_IMAGE = 16777216u;

	public const uint MEM_MAPPED = 262144u;

	[DllImport("kernel32.dll")]
	public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

	[DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
	public static extern UIntPtr Native_VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress, out MEMORY_BASIC_INFORMATION32 lpBuffer, UIntPtr dwLength);

	[DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
	public static extern UIntPtr Native_VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, UIntPtr dwLength);

	[DllImport("kernel32.dll")]
	public static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	public static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

	[DllImport("kernel32.dll")]
	public static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

	[DllImport("kernel32.dll")]
	public static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] IntPtr lpBuffer, UIntPtr nSize, out ulong lpNumberOfBytesRead);

	[DllImport("kernel32")]
	public static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);
}
