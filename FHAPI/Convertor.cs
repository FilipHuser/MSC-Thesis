using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PacketDotNet;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FHAPILib
{
    public static class Convertor<T> where T : struct
    {
        public static T? ConvertPayload(byte[] payload , int offset)
        {
            Func<byte[], int, T>? convertFunc = null;

            switch (typeof(T))
            {
                case Type t when t == typeof(sbyte):
                    convertFunc = (data, offset) => (T)(object)(sbyte)data[offset];
                    break;
                case Type t when t == typeof(short):
                    convertFunc = (data , offset) => (T)(object)BitConverter.ToInt16(data, offset);
                    break;
                case Type t when t == typeof(int):
                    convertFunc = (data, offset) => (T)(object)BitConverter.ToInt32(data, offset);
                    break;
                case Type t when t == typeof(long):
                    convertFunc = (data, offset) => (T)(object)BitConverter.ToInt64(data, offset);
                    break;
                case Type t when t == typeof(double):
                    convertFunc = (data, offset) => (T)(object)BitConverter.ToDouble(data, offset);
                    break;
            }
            return convertFunc != null ? convertFunc(payload , offset) : default;
        }
    }
}