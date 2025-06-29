using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAPILib
{
    public static class ByteArrayConverter<T> where T : struct
    {
        public static Func<byte[], int, object> GetConvertFunction()
        {
            Func<byte[], int, object>? convertFunc = null;

            switch (typeof(T))
            {
                case Type t when t == typeof(sbyte):
                    convertFunc = (data, offset) => (sbyte)data[offset];
                    break;
                case Type t when t == typeof(short):
                    convertFunc = (data, offset) => BitConverter.ToInt16(data, offset);
                    break;
                case Type t when t == typeof(int):
                    convertFunc = (data, offset) => BitConverter.ToInt32(data, offset);
                    break;
                case Type t when t == typeof(long):
                    convertFunc = (data, offset) => BitConverter.ToInt64(data, offset);
                    break;
                case Type t when t == typeof(double):
                    convertFunc = (data, offset) => BitConverter.ToDouble(data, offset);
                    break;
                case Type t when t == typeof(char):
                    convertFunc = (data, offset) => BitConverter.ToChar(data, offset);
                    break;
                default:
                    throw new NotImplementedException("Unsupported conversion type!");
            }

            return convertFunc;
        }
    }
}
