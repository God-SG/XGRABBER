using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XGRAB.Util.Memory.Structures;

namespace Memory;

public class Mem
{
	public Proc mProc = new Proc();

	public UIntPtr VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress, out Imps.MEMORY_BASIC_INFORMATION lpBuffer)
	{
		if (mProc.Is64Bit || IntPtr.Size == 8)
		{
			Imps.MEMORY_BASIC_INFORMATION64 tmp64 = default(Imps.MEMORY_BASIC_INFORMATION64);
			UIntPtr result = Imps.Native_VirtualQueryEx(hProcess, lpAddress, out tmp64, new UIntPtr((uint)Marshal.SizeOf(tmp64)));
			lpBuffer.BaseAddress = tmp64.BaseAddress;
			lpBuffer.AllocationBase = tmp64.AllocationBase;
			lpBuffer.AllocationProtect = tmp64.AllocationProtect;
			lpBuffer.RegionSize = (long)tmp64.RegionSize;
			lpBuffer.State = tmp64.State;
			lpBuffer.Protect = tmp64.Protect;
			lpBuffer.Type = tmp64.Type;
			return result;
		}
		Imps.MEMORY_BASIC_INFORMATION32 tmp65 = default(Imps.MEMORY_BASIC_INFORMATION32);
		UIntPtr result2 = Imps.Native_VirtualQueryEx(hProcess, lpAddress, out tmp65, new UIntPtr((uint)Marshal.SizeOf(tmp65)));
		lpBuffer.BaseAddress = tmp65.BaseAddress;
		lpBuffer.AllocationBase = tmp65.AllocationBase;
		lpBuffer.AllocationProtect = tmp65.AllocationProtect;
		lpBuffer.RegionSize = tmp65.RegionSize;
		lpBuffer.State = tmp65.State;
		lpBuffer.Protect = tmp65.Protect;
		lpBuffer.Type = tmp65.Type;
		return result2;
	}

	public bool OpenProcess(int pid, out string FailReason)
	{
		if (pid <= 0)
		{
			FailReason = "OpenProcess given proc ID 0.";
			return false;
		}
		if (mProc.Process != null && mProc.Process.Id == pid)
		{
			FailReason = "mProc.Process is null";
			return true;
		}
		try
		{
			mProc.Process = Process.GetProcessById(pid);
			if (mProc.Process != null && !mProc.Process.Responding)
			{
				FailReason = "Process is not responding or null.";
				return false;
			}
			mProc.Handle = Imps.OpenProcess(2035711u, bInheritHandle: true, pid);
			if (mProc.Handle == IntPtr.Zero)
			{
				int eCode = Marshal.GetLastWin32Error();
				Process.LeaveDebugMode();
				mProc = null;
				FailReason = "failed opening a handle to the target process(GetLastWin32ErrorCode: " + eCode + ")";
				return false;
			}
			mProc.Is64Bit = Environment.Is64BitOperatingSystem && Imps.IsWow64Process(mProc.Handle, out var retVal) && !retVal;
			mProc.MainModule = mProc.Process.MainModule;
			FailReason = "";
			return true;
		}
		catch (Exception ex)
		{
			FailReason = "OpenProcess has crashed. " + ex;
			return false;
		}
	}

	public bool OpenProcess(string proc)
	{
		string FailReason;
		return OpenProcess(GetProcIdFromName(proc), out FailReason);
	}

	public int GetProcIdFromName(string name)
	{
		Process[] processes = Process.GetProcesses();
		if (name.ToLower().Contains(".exe"))
		{
			name = name.Replace(".exe", "");
		}
		if (name.ToLower().Contains(".bin"))
		{
			name = name.Replace(".bin", "");
		}
		Process[] array = processes;
		foreach (Process theprocess in array)
		{
			if (theprocess.ProcessName.Equals(name, StringComparison.CurrentCultureIgnoreCase))
			{
				return theprocess.Id;
			}
		}
		return 0;
	}

	public string LoadCode(string name, string iniFile)
	{
		StringBuilder returnCode = new StringBuilder(1024);
		if (!string.IsNullOrEmpty(iniFile))
		{
			if (File.Exists(iniFile))
			{
				Imps.GetPrivateProfileString("codes", name, "", returnCode, (uint)returnCode.Capacity, iniFile);
			}
		}
		else
		{
			returnCode.Append(name);
		}
		return returnCode.ToString();
	}

	public UIntPtr GetCode(string name, string path = "", int size = 8)
	{
		string theCode = "";
		if (mProc == null)
		{
			return UIntPtr.Zero;
		}
		if (mProc.Is64Bit)
		{
			if (size == 8)
			{
				size = 16;
			}
			return Get64BitCode(name, path, size);
		}
		theCode = (string.IsNullOrEmpty(path) ? name : LoadCode(name, path));
		if (string.IsNullOrEmpty(theCode))
		{
			return UIntPtr.Zero;
		}
		if (theCode.Contains(" "))
		{
			theCode = theCode.Replace(" ", string.Empty);
		}
		if (!theCode.Contains("+") && !theCode.Contains(","))
		{
			try
			{
				return new UIntPtr(Convert.ToUInt32(theCode, 16));
			}
			catch
			{
				Console.WriteLine("Error in GetCode(). Failed to read address " + theCode);
				return UIntPtr.Zero;
			}
		}
		string newOffsets = theCode;
		if (theCode.Contains("+"))
		{
			newOffsets = theCode.Substring(theCode.IndexOf('+') + 1);
		}
		byte[] memoryAddress = new byte[size];
		if (newOffsets.Contains(','))
		{
			List<int> offsetsList = new List<int>();
			string[] array = newOffsets.Split(',');
			foreach (string oldOffsets in array)
			{
				string test = oldOffsets;
				if (oldOffsets.Contains("0x"))
				{
					test = oldOffsets.Replace("0x", "");
				}
				int preParse = 0;
				if (!oldOffsets.Contains("-"))
				{
					preParse = int.Parse(test, NumberStyles.AllowHexSpecifier);
				}
				else
				{
					test = test.Replace("-", "");
					preParse = int.Parse(test, NumberStyles.AllowHexSpecifier);
					preParse *= -1;
				}
				offsetsList.Add(preParse);
			}
			int[] offsets = offsetsList.ToArray();
			if (theCode.Contains("base") || theCode.Contains("main"))
			{
				Imps.ReadProcessMemory(mProc.Handle, (UIntPtr)(ulong)((int)mProc.MainModule.BaseAddress + offsets[0]), memoryAddress, (UIntPtr)(ulong)size, IntPtr.Zero);
			}
			else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
			{
				string[] moduleName = theCode.Split('+');
				IntPtr altModule = IntPtr.Zero;
				if (!moduleName[0].ToLower().Contains(".dll") && !moduleName[0].ToLower().Contains(".exe") && !moduleName[0].ToLower().Contains(".bin"))
				{
					string theAddr = moduleName[0];
					if (theAddr.Contains("0x"))
					{
						theAddr = theAddr.Replace("0x", "");
					}
					altModule = (IntPtr)int.Parse(theAddr, NumberStyles.HexNumber);
				}
				else
				{
					try
					{
						altModule = GetModuleAddressByName(moduleName[0]);
					}
					catch
					{
					}
				}
				Imps.ReadProcessMemory(mProc.Handle, (UIntPtr)(ulong)((int)altModule + offsets[0]), memoryAddress, (UIntPtr)(ulong)size, IntPtr.Zero);
			}
			else
			{
				Imps.ReadProcessMemory(mProc.Handle, (UIntPtr)(ulong)offsets[0], memoryAddress, (UIntPtr)(ulong)size, IntPtr.Zero);
			}
			uint num1 = BitConverter.ToUInt32(memoryAddress, 0);
			UIntPtr base1 = (UIntPtr)0uL;
			for (int j = 1; j < offsets.Length; j++)
			{
				base1 = new UIntPtr(Convert.ToUInt32(num1 + offsets[j]));
				Imps.ReadProcessMemory(mProc.Handle, base1, memoryAddress, (UIntPtr)(ulong)size, IntPtr.Zero);
				num1 = BitConverter.ToUInt32(memoryAddress, 0);
			}
			return base1;
		}
		int trueCode = Convert.ToInt32(newOffsets, 16);
		IntPtr altModule2 = IntPtr.Zero;
		if (theCode.ToLower().Contains("base") || theCode.ToLower().Contains("main"))
		{
			altModule2 = mProc.MainModule.BaseAddress;
		}
		else if (!theCode.ToLower().Contains("base") && !theCode.ToLower().Contains("main") && theCode.Contains("+"))
		{
			string[] moduleName2 = theCode.Split('+');
			if (!moduleName2[0].ToLower().Contains(".dll") && !moduleName2[0].ToLower().Contains(".exe") && !moduleName2[0].ToLower().Contains(".bin"))
			{
				string theAddr2 = moduleName2[0];
				if (theAddr2.Contains("0x"))
				{
					theAddr2 = theAddr2.Replace("0x", "");
				}
				altModule2 = (IntPtr)int.Parse(theAddr2, NumberStyles.HexNumber);
			}
			else
			{
				try
				{
					altModule2 = GetModuleAddressByName(moduleName2[0]);
				}
				catch
				{
				}
			}
		}
		else
		{
			altModule2 = GetModuleAddressByName(theCode.Split('+')[0]);
		}
		return (UIntPtr)(ulong)((int)altModule2 + trueCode);
	}

	public IntPtr GetModuleAddressByName(string name)
	{
		return mProc.Process.Modules.Cast<ProcessModule>().SingleOrDefault((ProcessModule m) => string.Equals(m.ModuleName, name, StringComparison.OrdinalIgnoreCase)).BaseAddress;
	}

	public UIntPtr Get64BitCode(string name, string path = "", int size = 16)
	{
		string theCode = "";
		theCode = (string.IsNullOrEmpty(path) ? name : LoadCode(name, path));
		if (string.IsNullOrEmpty(theCode))
		{
			return UIntPtr.Zero;
		}
		if (theCode.Contains(" "))
		{
			theCode.Replace(" ", string.Empty);
		}
		string newOffsets = theCode;
		if (theCode.Contains("+"))
		{
			newOffsets = theCode.Substring(theCode.IndexOf('+') + 1);
		}
		byte[] memoryAddress = new byte[size];
		if (!theCode.Contains("+") && !theCode.Contains(","))
		{
			try
			{
				return new UIntPtr(Convert.ToUInt64(theCode, 16));
			}
			catch
			{
				Console.WriteLine("Error in GetCode(). Failed to read address " + theCode);
				return UIntPtr.Zero;
			}
		}
		if (newOffsets.Contains(','))
		{
			List<long> offsetsList = new List<long>();
			string[] array = newOffsets.Split(',');
			foreach (string oldOffsets in array)
			{
				string test = oldOffsets;
				if (oldOffsets.Contains("0x"))
				{
					test = oldOffsets.Replace("0x", "");
				}
				long preParse = 0L;
				if (!oldOffsets.Contains("-"))
				{
					preParse = long.Parse(test, NumberStyles.AllowHexSpecifier);
				}
				else
				{
					test = test.Replace("-", "");
					preParse = long.Parse(test, NumberStyles.AllowHexSpecifier);
					preParse *= -1;
				}
				offsetsList.Add(preParse);
			}
			long[] offsets = offsetsList.ToArray();
			if (theCode.Contains("base") || theCode.Contains("main"))
			{
				Imps.ReadProcessMemory(mProc.Handle, (UIntPtr)(ulong)((long)mProc.MainModule.BaseAddress + offsets[0]), memoryAddress, (UIntPtr)(ulong)size, IntPtr.Zero);
			}
			else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
			{
				string[] moduleName = theCode.Split('+');
				IntPtr altModule = IntPtr.Zero;
				if (!moduleName[0].ToLower().Contains(".dll") && !moduleName[0].ToLower().Contains(".exe") && !moduleName[0].ToLower().Contains(".bin"))
				{
					altModule = (IntPtr)long.Parse(moduleName[0], NumberStyles.HexNumber);
				}
				else
				{
					try
					{
						altModule = GetModuleAddressByName(moduleName[0]);
					}
					catch
					{
					}
				}
				Imps.ReadProcessMemory(mProc.Handle, (UIntPtr)(ulong)((long)altModule + offsets[0]), memoryAddress, (UIntPtr)(ulong)size, IntPtr.Zero);
			}
			else
			{
				Imps.ReadProcessMemory(mProc.Handle, (UIntPtr)(ulong)offsets[0], memoryAddress, (UIntPtr)(ulong)size, IntPtr.Zero);
			}
			long num1 = BitConverter.ToInt64(memoryAddress, 0);
			UIntPtr base1 = (UIntPtr)0uL;
			for (int j = 1; j < offsets.Length; j++)
			{
				base1 = new UIntPtr(Convert.ToUInt64(num1 + offsets[j]));
				Imps.ReadProcessMemory(mProc.Handle, base1, memoryAddress, (UIntPtr)(ulong)size, IntPtr.Zero);
				num1 = BitConverter.ToInt64(memoryAddress, 0);
			}
			return base1;
		}
		long trueCode = Convert.ToInt64(newOffsets, 16);
		IntPtr altModule2 = IntPtr.Zero;
		if (theCode.Contains("base") || theCode.Contains("main"))
		{
			altModule2 = mProc.MainModule.BaseAddress;
		}
		else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
		{
			string[] moduleName2 = theCode.Split('+');
			if (!moduleName2[0].ToLower().Contains(".dll") && !moduleName2[0].ToLower().Contains(".exe") && !moduleName2[0].ToLower().Contains(".bin"))
			{
				string theAddr = moduleName2[0];
				if (theAddr.Contains("0x"))
				{
					theAddr = theAddr.Replace("0x", "");
				}
				altModule2 = (IntPtr)long.Parse(theAddr, NumberStyles.HexNumber);
			}
			else
			{
				try
				{
					altModule2 = GetModuleAddressByName(moduleName2[0]);
				}
				catch
				{
				}
			}
		}
		else
		{
			altModule2 = GetModuleAddressByName(theCode.Split('+')[0]);
		}
		return (UIntPtr)(ulong)((long)altModule2 + trueCode);
	}

	public string MSize()
	{
		if (mProc.Is64Bit)
		{
			return "x16";
		}
		return "x8";
	}

	public Task<IEnumerable<long>> AoBScan(string search, bool writable = false, bool executable = true, string file = "")
	{
		return AoBScan(0L, long.MaxValue, search, writable, executable, mapped: false, file);
	}

	public Task<IEnumerable<long>> AoBScan(long start, long end, string search, bool writable = false, bool executable = true, bool mapped = false, string file = "")
	{
		return AoBScan(start, end, search, readable: false, writable, executable, mapped, file);
	}

	public Task<IEnumerable<long>> AoBScan(long start, long end, string search, bool readable, bool writable, bool executable, bool mapped, string file = "")
	{
		return Task.Run(delegate
		{
			List<MemoryRegionResult> list = new List<MemoryRegionResult>();
			string[] array = LoadCode(search, file).Split(' ');
			byte[] aobPattern = new byte[array.Length];
			byte[] mask = new byte[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				if (text == "??" || (text.Length == 1 && text == "?"))
				{
					mask[i] = 0;
					array[i] = "0x00";
				}
				else if (char.IsLetterOrDigit(text[0]) && text[1] == '?')
				{
					mask[i] = 240;
					array[i] = text[0] + "0";
				}
				else if (char.IsLetterOrDigit(text[1]) && text[0] == '?')
				{
					mask[i] = 15;
					array[i] = "0" + text[1];
				}
				else
				{
					mask[i] = byte.MaxValue;
				}
			}
			for (int j = 0; j < array.Length; j++)
			{
				aobPattern[j] = (byte)(Convert.ToByte(array[j], 16) & mask[j]);
			}
			Imps.SYSTEM_INFO lpSystemInfo = default(Imps.SYSTEM_INFO);
			Imps.GetSystemInfo(out lpSystemInfo);
			UIntPtr minimumApplicationAddress = lpSystemInfo.minimumApplicationAddress;
			UIntPtr maximumApplicationAddress = lpSystemInfo.maximumApplicationAddress;
			if (start < (long)minimumApplicationAddress.ToUInt64())
			{
				start = (long)minimumApplicationAddress.ToUInt64();
			}
			if (end > (long)maximumApplicationAddress.ToUInt64())
			{
				end = (long)maximumApplicationAddress.ToUInt64();
			}
			UIntPtr uIntPtr = new UIntPtr((ulong)start);
			Imps.MEMORY_BASIC_INFORMATION lpBuffer = default(Imps.MEMORY_BASIC_INFORMATION);
			while (VirtualQueryEx(mProc.Handle, uIntPtr, out lpBuffer).ToUInt64() != 0L && uIntPtr.ToUInt64() < (ulong)end && (ulong)((long)uIntPtr.ToUInt64() + lpBuffer.RegionSize) > uIntPtr.ToUInt64())
			{
				bool flag = lpBuffer.State == 4096;
				flag &= lpBuffer.BaseAddress.ToUInt64() < maximumApplicationAddress.ToUInt64();
				flag &= (lpBuffer.Protect & 0x100) == 0;
				flag &= (lpBuffer.Protect & 1) == 0;
				flag &= lpBuffer.Type == 131072 || lpBuffer.Type == 16777216;
				if (mapped)
				{
					flag &= lpBuffer.Type == 262144;
				}
				if (flag)
				{
					bool flag2 = (lpBuffer.Protect & 2) != 0;
					bool flag3 = (lpBuffer.Protect & 4) != 0 || (lpBuffer.Protect & 8) != 0 || (lpBuffer.Protect & 0x40) != 0 || (lpBuffer.Protect & 0x80) != 0;
					bool flag4 = (lpBuffer.Protect & 0x10) != 0 || (lpBuffer.Protect & 0x20) != 0 || (lpBuffer.Protect & 0x40) != 0 || (lpBuffer.Protect & 0x80) != 0;
					flag2 = flag2 && readable;
					flag3 = flag3 && writable;
					flag4 = flag4 && executable;
					flag = flag && (flag2 || flag3 || flag4);
				}
				if (!flag)
				{
					uIntPtr = new UIntPtr(lpBuffer.BaseAddress.ToUInt64() + (ulong)lpBuffer.RegionSize);
				}
				else
				{
					MemoryRegionResult memoryRegionResult = default(MemoryRegionResult);
					memoryRegionResult.CurrentBaseAddress = uIntPtr;
					memoryRegionResult.RegionSize = lpBuffer.RegionSize;
					memoryRegionResult.RegionBase = lpBuffer.BaseAddress;
					MemoryRegionResult item2 = memoryRegionResult;
					uIntPtr = new UIntPtr(lpBuffer.BaseAddress.ToUInt64() + (ulong)lpBuffer.RegionSize);
					if (list.Count > 0)
					{
						MemoryRegionResult memoryRegionResult2 = list[list.Count - 1];
						if ((long)(ulong)memoryRegionResult2.RegionBase + memoryRegionResult2.RegionSize == (long)(ulong)lpBuffer.BaseAddress)
						{
							list[list.Count - 1] = new MemoryRegionResult
							{
								CurrentBaseAddress = memoryRegionResult2.CurrentBaseAddress,
								RegionBase = memoryRegionResult2.RegionBase,
								RegionSize = memoryRegionResult2.RegionSize + lpBuffer.RegionSize
							};
							continue;
						}
					}
					list.Add(item2);
				}
			}
			ConcurrentBag<long> bagResult = new ConcurrentBag<long>();
			Parallel.ForEach(list, delegate(MemoryRegionResult item, ParallelLoopState parallelLoopState, long index)
			{
				long[] array2 = CompareScan(item, aobPattern, mask);
				foreach (long item3 in array2)
				{
					bagResult.Add(item3);
				}
			});
			return (from c in bagResult.ToList()
				orderby c
				select c).AsEnumerable();
		});
	}

	private unsafe long[] CompareScan(MemoryRegionResult item, byte[] aobPattern, byte[] mask)
	{
		if (mask.Length != aobPattern.Length)
		{
			throw new ArgumentException("aobPattern.Length != mask.Length");
		}
		IntPtr buffer = Marshal.AllocHGlobal((int)item.RegionSize);
		Imps.ReadProcessMemory(mProc.Handle, item.CurrentBaseAddress, buffer, (UIntPtr)(ulong)item.RegionSize, out var bytesRead);
		int result = -aobPattern.Length;
		List<long> ret = new List<long>();
		do
		{
			result = FindPattern((byte*)buffer.ToPointer(), (int)bytesRead, aobPattern, mask, result + aobPattern.Length);
			if (result >= 0)
			{
				ret.Add((long)(ulong)item.CurrentBaseAddress + (long)result);
			}
		}
		while (result != -1);
		Marshal.FreeHGlobal(buffer);
		return ret.ToArray();
	}

	private unsafe int FindPattern(byte* body, int bodyLength, byte[] pattern, byte[] masks, int start = 0)
	{
		int foundIndex = -1;
		if (bodyLength <= 0 || pattern.Length == 0 || start > bodyLength - pattern.Length || pattern.Length > bodyLength)
		{
			return foundIndex;
		}
		for (int index = start; index <= bodyLength - pattern.Length; index++)
		{
			if ((body[index] & masks[0]) != (pattern[0] & masks[0]))
			{
				continue;
			}
			bool match = true;
			for (int index2 = pattern.Length - 1; index2 >= 1; index2--)
			{
				if ((body[index + index2] & masks[index2]) != (pattern[index2] & masks[index2]))
				{
					match = false;
					break;
				}
			}
			if (match)
			{
				foundIndex = index;
				break;
			}
		}
		return foundIndex;
	}

	public string ReadString(string code, string file = "", int length = 32, bool zeroTerminated = true, Encoding stringEncoding = null)
	{
		if (stringEncoding == null)
		{
			stringEncoding = Encoding.UTF8;
		}
		byte[] memoryNormal = new byte[length];
		UIntPtr theCode = GetCode(code, file);
		if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 65536)
		{
			return "";
		}
		if (Imps.ReadProcessMemory(mProc.Handle, theCode, memoryNormal, (UIntPtr)(ulong)length, IntPtr.Zero))
		{
			if (!zeroTerminated)
			{
				return stringEncoding.GetString(memoryNormal);
			}
			return stringEncoding.GetString(memoryNormal).Split('\0')[0];
		}
		return "";
	}
}
