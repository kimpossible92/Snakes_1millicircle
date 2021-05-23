using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SteamNetworking
{
    public enum SendType
    {
        Unreliable = 0,
        UnreliableNoDelay = 1,
        Reliable = 2,
        ReliableWithBuffering = 3
    };

    public static class SendTypeExtensionMethods
    {
        public static Facepunch.Steamworks.Networking.SendType GetNetworkingSendType(this SendType sendType)
        {
            return (Facepunch.Steamworks.Networking.SendType)sendType;
        }
    }
    public class ByteSerializer
    {
        public static byte[] GetBytes<T>(T t)
        {
            int size = Marshal.SizeOf(t);
            byte[] data = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(t, ptr, true);
            Marshal.Copy(ptr, data, 0, size);
            Marshal.FreeHGlobal(ptr);

            return data;
        }
        public static T FromBytes<T>(byte[] data)
        {
            int size = Marshal.SizeOf(typeof(T));

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(data, 0, ptr, size);
            T t = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            return t;
        }
    }
}