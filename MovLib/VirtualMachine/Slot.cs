using System;
using System.Collections.Generic;

namespace MovLib.VirtualMachine
{
    /// <summary>
    /// Machines connect to each other through the use of slots.
    /// 
    /// Slots are also used to implement registers!
    /// </summary>
    public class Slot : IStorageSlot
    {
        /// <summary>
        /// IO pins support flags being set when triggering them.
        /// 
        /// MultiFlag mode lets you set multiple flags at once but you can only have one entry point.
        /// SingleFlag mode only lets one flag to be set at once but you can have multiple entry points (one per each flag).
        ///     note: this is useful especially when you get multi-core setups allowing you to execute each possible entry point concurrently.
        /// </summary>
        public List<string> Flags { get; } = new List<string>();

        public SlotMode Mode { get; set; }
        public ValueKind ValueType { get; set; }

        /// <summary>
        /// When this doesn't hold a value it will cause all reads to fail
        /// </summary>
        public Value? Value { get; set; } = null;

        public bool CanIndexWrite => false;
        public bool CanIndexRead => false;
        public bool CanRead => Mode.HasFlag(SlotMode.Read);
        public bool CanWrite => Mode.HasFlag(SlotMode.Write);

        /// <summary>
        /// Technically, we could just do Value == null for this, but I would rather us handle it better.
        /// 
        /// Because if !ReadsConsumeValue and somehow Value == null I would rather a crash then for it to cause blocking.
        /// </summary>
        public bool ReadsAreBlocking => Value == null && Mode.HasFlag(SlotMode.ReadsConsumeValue);
        public bool WritesAreBlocking => Value != null && Mode.HasFlag(SlotMode.WritesAreBlocking);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Value Read()
        {
            if (!CanRead) throw new InvalidOperationException();
            if (ReadsAreBlocking) throw new InvalidOperationException();
            // this handles the case of Value == null && !ReadsAreBlocking
            // this is a much more internal bug case.
            if (Value == null) throw new InvalidOperationException();

            var result = Value;
            if (Mode.HasFlag(SlotMode.ReadsConsumeValue))
            {
                Value = null;
            }
            return result.Value;
        }

        public void Write(Value value)
        {
            if (!CanWrite) throw new InvalidOperationException();
            if (WritesAreBlocking) throw new InvalidOperationException();
            if (value.Kind != ValueType) throw new InvalidCastException();

            Value = value;
        }

        public static Slot Register(Value defaultValue)
        {
            return new Slot
            {
                Flags = { },
                Mode = SlotMode.ReadWrite,
                Value = defaultValue,
                ValueType = defaultValue.Kind,
            };
        }
    }
}
