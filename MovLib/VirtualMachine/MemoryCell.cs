using System;

namespace MovLib.VirtualMachine
{
    public class MemoryCell : IStorageSlot
    {
        private Value[] _cells = new Value[0];
        private int _offset = 0;

        /// <summary>
        /// ROM vs RAM
        /// </summary>
        public bool IsReadonly { get; set; } = false;

        /// <summary>
        /// Is the offset writable (i.e. for a stack pointer).
        /// </summary>
        public bool OffsetWritable { get; set; } = false;

        public ValueKind ValueType { get; set; } = ValueKind.Int;

        public bool CanIndexWrite => !IsReadonly;

        public bool CanIndexRead => true;

        public bool CanRead => true;

        /// <summary>
        /// You can only write to this if the offset is writable
        /// </summary>
        public bool CanWrite => OffsetWritable;

        /// <summary>
        /// Cells always have a value (since they get defaulted to 0 for example).
        /// </summary>
        public bool ReadsAreBlocking => false;
        public bool WritesAreBlocking => false;

        public Value Read()
        {
            return new Value(ValueKind.Int, _offset);
        }

        public void Write(Value value)
        {
            if (!OffsetWritable) throw new InvalidOperationException();
            _offset = value.GetInt();
        }

        public Value IndexedRead(Value index)
        {
            // for now not support vectors
            return _cells[_offset + index.GetInt()];
        }

        public void IndexedWrite(Value index, Value value)
        {
            if (value.Kind != ValueType) throw new InvalidCastException();

            _cells[_offset + index.GetInt()] = value;
        }

        public static MemoryCell Ram(int length, ValueKind type)
        {
            return new MemoryCell()
            {
                _cells = new Value[length],
                IsReadonly = false,
                OffsetWritable = false,
                ValueType = type,
            };
        }

        public static MemoryCell Rom(int length, ValueKind type)
        {
            return new MemoryCell()
            {
                _cells = new Value[length],
                IsReadonly = true,
                OffsetWritable = false,
                ValueType = type,
            };
        }

        public static MemoryCell StackPointer(int length, ValueKind type)
        {
            return new MemoryCell()
            {
                _cells = new Value[length],
                IsReadonly = false,
                OffsetWritable = true,
                ValueType = type,
            };
        }
    }
}
