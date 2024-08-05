using System;

namespace MovLib.VirtualMachine
{
    public struct Value
    {
        public Value(ValueKind kind, object data)
        {
            Kind = kind;
            Data = data;
        }

        public ValueKind Kind { get; set; }
        public object Data { get; set; }

        public int GetInt()
        {
            if (Kind != ValueKind.Int) throw new InvalidOperationException();
            if (Data is int n)
            {
                return n;
            }
            else
            {
                throw new InvalidCastException();
            }
        }
    }
}
