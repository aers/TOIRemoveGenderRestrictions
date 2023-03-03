using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MelonLoader;

namespace TOIRemoveGenderRestrictions
{
    public static class AsmPatch
    {
        public static bool ApplyPatch(string patchName, Type type, string fieldName, string signature, int scanSize, byte[] bytes)
        {
            ProcessModule gameAssembly = null;

            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                if (!module.ModuleName.Equals("GameAssembly.dll")) continue;
                gameAssembly = module;
            }

            if (gameAssembly is null)
            {
                MelonLogger.Msg($"Unable to locate GameAssembly.dll module.");
                return false;
            }
            
            var fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            if (fieldInfo == null)
            {
                MelonLogger.Msg($"Unable to locate field {fieldName} in type {type.Name} for patch {patchName}.");
                return false;
            }

            var methodPtr = Marshal.ReadIntPtr((IntPtr) fieldInfo.GetValue(null));
            var patchAddress = Scan(methodPtr, scanSize, signature);

            if (patchAddress == IntPtr.Zero)
            {
                MelonLogger.Msg(
                        $"Unable to locate patch address in method at GameAssembly.dll+{methodPtr.ToInt64() - gameAssembly.BaseAddress.ToInt64():X8} with signature {signature} for patch {patchName}.");
                return false;

            }
            
            VirtualProtect(patchAddress, (uint) bytes.Length, Protection.PAGE_EXECUTE_READWRITE, out Protection old);
            for (int i = 0; i < bytes.Length; i++)
            {
                Marshal.WriteByte(patchAddress + i, bytes[i]);
            }
            VirtualProtect(patchAddress, (uint) bytes.Length, old, out Protection _);
            
            MelonLogger.Msg($"Patched {patchName} at GameAssembly.dll+{patchAddress.ToInt64() - gameAssembly.BaseAddress.ToInt64():X8}.");

            return true;
        }

        private static IntPtr Scan(IntPtr startAddress, int size, string signature)
        {
            var (needle, mask) = ParseSignature(signature);
            var index = IndexOf(startAddress, size, needle, mask);

            if (index < 0)
                return IntPtr.Zero;

            return startAddress + index;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int IndexOf(IntPtr bufferPtr, int bufferLength, byte[] needle, bool[] mask)
        {
            if (needle.Length > bufferLength) return -1;
            var badShift = BuildBadCharTable(needle, mask);
            var last = needle.Length - 1;
            var offset = 0;
            var maxoffset = bufferLength - needle.Length;
            var buffer = (byte*)bufferPtr;

            while (offset <= maxoffset)
            {
                int position;
                for (position = last; needle[position] == *(buffer + position + offset) || mask[position]; position--)
                {
                    if (position == 0)
                        return offset;
                }

                offset += badShift[*(buffer + offset + last)];
            }

            return -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] BuildBadCharTable(byte[] needle, bool[] mask)
        {
            int idx;
            var last = needle.Length - 1;
            var badShift = new int[256];
            for (idx = last; idx > 0 && !mask[idx]; --idx)
            {
            }

            var diff = last - idx;
            if (diff == 0) diff = 1;

            for (idx = 0; idx <= 255; ++idx)
                badShift[idx] = diff;
            for (idx = last - diff; idx < last; ++idx)
                badShift[needle[idx]] = last - idx;
            return badShift;
        }

        private static (byte[] Needle, bool[] Mask) ParseSignature(string signature)
        {
            signature = signature.Replace(" ", string.Empty);

            var needleLength = signature.Length / 2;
            var needle = new byte[needleLength];
            var mask = new bool[needleLength];
            for (var i = 0; i < needleLength; i++)
            {
                var hexString = signature.Substring(i * 2, 2);
                if (hexString == "??" || hexString == "**")
                {
                    needle[i] = 0;
                    mask[i] = true;
                    continue;
                }

                needle[i] = byte.Parse(hexString, NumberStyles.AllowHexSpecifier);
                mask[i] = false;
            }

            return (needle, mask);
        }
        
        private static void PatchMemory(IntPtr address, int offset, byte[] bytes)
        {
            var writeAddress = address + offset;

            VirtualProtect(writeAddress, (uint) bytes.Length, Protection.PAGE_EXECUTE_READWRITE, out Protection old);
            for (int i = 0; i < bytes.Length; i++)
            {
                Marshal.WriteByte(writeAddress + i, bytes[i]);
            }
            VirtualProtect(writeAddress, (uint) bytes.Length, old, out Protection _);
        }
        
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize,
            Protection flNewProtect, out Protection lpflOldProtect);

        private enum Protection {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }
        

    }
}