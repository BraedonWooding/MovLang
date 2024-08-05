using System;

namespace MovLib.VirtualMachine
{
    /// <summary>
    /// This is the "lowest" form of what a value is in terms of storage.
    /// 
    /// This could be a register?  Could be a memory cell, could be a slot (like an IO pin).
    /// 
    /// Key thing to register is that everything is effectively a slot.  For example:
    /// - A register is literally just a ReadWrite slot (even PC is a ReadWrite slot)
    /// - Tables (readonly) are literally just indexed readonly cells
    /// - Heap is a collection of memory cells
    /// - Stack is just a indexed heap but the address moves as you push/pop
    /// 
    /// There are 2 ways you can interact with a storage slot
    /// - Read i.e. `MEM`, `r1`, ...
    /// - Indexed Read i.e. `MEM[0]`, `SP[-1]`
    /// All values support "reading" but indexed sets like Heap/Stack will simply just return their current
    /// pointer value (which is 0 for mem & could be anything up to the length for SP).
    /// 
    /// Indexed reads are only supported on indexed storage slots.  Indexing is typically meant to just be an int
    /// but that's not always true.  For example;
    /// - `v1[TABLE]` and `TABLE[v1]` are both supported and TABLE[v1] applies the table lookup *over* v1
    ///   where as v1[TABLE] does the inverse (i.e. for each value in TABLE map it to the index in v1).
    /// </summary>
    public interface IStorageSlot
    {
        /// <summary>
        /// Does this cell support indexing
        /// </summary>
        public bool CanIndexWrite { get; }

        /// <summary>
        /// ROM is only index read for example.
        /// </summary>
        public bool CanIndexRead { get; }

        public bool SupportsIndexing => CanIndexWrite | CanIndexRead;

        /// <summary>
        /// WriteOnly slots exist.
        /// </summary>
        public bool CanRead { get; }

        /// <summary>
        /// ReadOnly memory is pretty common.
        /// </summary>
        public bool CanWrite { get; }

        /// <summary>
        /// We don't support mixing cell types.
        /// </summary>
        public ValueKind ValueType { get; }

        public bool ReadsAreBlocking => false;
        public bool WritesAreBlocking => false;

        public Value Read() => throw new InvalidOperationException();
        public void Write(Value value) => throw new InvalidOperationException();

        public Value IndexedRead(Value index) => throw new InvalidOperationException();
        public void IndexedWrite(Value index, Value value) => throw new InvalidOperationException();
    }
}
