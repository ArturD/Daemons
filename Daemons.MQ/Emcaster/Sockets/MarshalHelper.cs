using System.Runtime.InteropServices;

namespace Emcaster.Sockets
{
    public static class MarshalHelper
    {
        public static byte[] ConvertToBytes(object obj)
        {
            int structSize = Marshal.SizeOf(obj);
            var allData = new byte[structSize];
            GCHandle handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(allData, GCHandleType.Pinned);
                Marshal.StructureToPtr(obj,
                                       handle.AddrOfPinnedObject(),
                                       false);
                return allData;
            }
            finally
            {
                if (handle != default(GCHandle))
                    handle.Free();
            }
        }

        public static T ConvertBytesTo<T>(byte[] data)
        {
            GCHandle pinnedPacket = default(GCHandle);
            try
            {
                pinnedPacket = GCHandle.Alloc(data, GCHandleType.Pinned);
                return (T)Marshal.PtrToStructure(
                    pinnedPacket.AddrOfPinnedObject(),
                    typeof(T));
            }
            finally
            {
                if (pinnedPacket != default(GCHandle))
                    pinnedPacket.Free();
            }
        }
    }
}