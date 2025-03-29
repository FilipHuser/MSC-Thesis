using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHAPILib
{
    public static class Converter<T> where T : struct
    {
        public static Func<byte[], int, double> GetPayloadConvertFunction()
        {
            Func<byte[], int, double>? convertFunc = null;

            switch (typeof(T))
            {
                case Type t when t == typeof(sbyte):
                    convertFunc = (data, offset) => (double)(sbyte)data[offset];
                    break;
                case Type t when t == typeof(short):
                    convertFunc = (data, offset) => (double)BitConverter.ToInt16(data, offset);
                    break;
                case Type t when t == typeof(int):
                    convertFunc = (data, offset) => (double)BitConverter.ToInt32(data, offset);
                    break;
                case Type t when t == typeof(long):
                    convertFunc = (data, offset) => (double)BitConverter.ToInt64(data, offset);
                    break;
                case Type t when t == typeof(double):
                    convertFunc = (data, offset) => BitConverter.ToDouble(data, offset);
                    break;
                default:
                    throw new NotImplementedException("Unsupported conversion type!");
            }

            return convertFunc;
        }
    }
}
