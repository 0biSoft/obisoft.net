using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ObisoftNet.Memory
{
    public class ProcessMemory
    {
        private Process _process;
        private IntPtr _openhandle;

        public ProcessMemory(Process proc=null)
        {
            if (proc != null)
                _process = proc;
            else
                _process = Process.GetCurrentProcess();

            _openhandle = NativeMemory.OpenProcess(ProcessOperation.VM_OPERATION | ProcessOperation.VM_READ | ProcessOperation.VM_WRITE,
                        false, _process.Id);
        }

        public IntPtr GetModuleAddress(string Name)
        {
            try
            {
                foreach (ProcessModule ProcMod in _process.Modules)
                    if (Name.ToLower() == ProcMod.ModuleName.ToLower())
                        return ProcMod.BaseAddress;
            }
            catch{}
            return IntPtr.Zero;
        }

        public int Write<T>(IntPtr address, object value)
        {
            var buffer = StructureToByteArray(value);
            int writen = 0;
            NativeMemory.WriteProcessMemory((int)_openhandle, address, buffer, buffer.Length, ref writen);
            return writen;
        }

        public  int Write(IntPtr address, char[] value)
        {
            var buffer = Encoding.UTF8.GetBytes(value);
            int writen = 0;
            NativeMemory.WriteProcessMemory((int)_openhandle, address, buffer, buffer.Length, ref writen);
            return writen;
        }

        public T Read<T>(int address) where T : struct
        {
            var ByteSize = Marshal.SizeOf(typeof(T));

            var buffer = new byte[ByteSize];

            int read = 0;
            NativeMemory.ReadProcessMemory((int)_openhandle, address, buffer, buffer.Length, ref read);

            return ByteArrayToStructure<T>(buffer);
        }

        public byte[] Read(int offset, int size)
        {
            var buffer = new byte[size];
            int read = 0;
            NativeMemory.ReadProcessMemory((int)_openhandle, offset, buffer, size, ref read);
            return buffer;
        }
        public  float[] ReadMatrix<T>(int Adress, int MatrixSize) where T : struct
        {
            var ByteSize = Marshal.SizeOf(typeof(T));
            var buffer = new byte[ByteSize * MatrixSize];
            int read = 0;
            NativeMemory.ReadProcessMemory((int)_openhandle, Adress, buffer, buffer.Length, ref read);
            return ConvertToFloatArray(buffer);
        }
        #region Conversion

        public static float[] ConvertToFloatArray(byte[] bytes)
        {
            if (bytes.Length % 4 != 0)
                throw new ArgumentException();

            var floats = new float[bytes.Length / 4];

            for (var i = 0; i < floats.Length; i++)
                floats[i] = BitConverter.ToSingle(bytes, i * 4);

            return floats;
        }

        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        public static byte[] StructureToByteArray(object obj)
        {
            var length = Marshal.SizeOf(obj);

            var array = new byte[length];

            var pointer = Marshal.AllocHGlobal(length);

            Marshal.StructureToPtr(obj, pointer, true);
            Marshal.Copy(pointer, array, 0, length);
            Marshal.FreeHGlobal(pointer);

            return array;
        }

        #endregion
    }
}
