using System;

namespace MovLib.VirtualMachine
{
    [Flags]
    public enum SlotMode
    {
        None = 0,
        Read = 1 << 0,
        Write = 1 << 1,

        /// <summary>
        /// If there is a value in the slot do we overwrite the value?
        /// </summary>
        WritesAreBlocking = 1 << 2,

        /// <summary>
        /// Removes the value on read causing future reads to fail
        /// </summary>
        ReadsConsumeValue = 1 << 3,

        /// <summary>
        /// Can you supply flags when calling it
        /// </summary>
        HasFlags = 1 << 4,

        /// <summary>
        /// Can you supply mulitiple flags
        /// </summary>
        MultiFlags = 1 << 5,

        /// <summary>
        /// ReadWrite is confusing but it's useful in concurrent applications, this is due to it's inherent blocking behaviour.
        /// 
        /// This lets you write *simple* (compared to alternatives) locks by utilising the fact that reading/writing to a pin is blocking given it no data/has data.
        /// 
        /// Typically, you would need an extra component with a single read & write pin to implement read-write (with it's code just being a loop of `OUT = IN`)
        /// but this has a few downsides:
        /// 1. "Cost" requiring a new component with a cpu
        /// 2. Tick delays, there is a 1-tick delay since writes happen at the end of ticks so releasing a lock has a 1-tick delay before the lock can be taken again
        ///    realistically, this is pretty minor.
        /// 3. Complexity, you have to link up modules to this external component.
        /// 
        /// ReadWrite pins let you handle this through a single pin buffer, this along with the standalone pin component let's you implement very simple locking behaviour
        /// (presuming external read-write is called LOCK)
        ///
        /// # take lock, this would be a "function"
        /// # this is a generic semaphore implementation (i.e. up to n locks at once) but it can easily be done as a simple 0/1 lock to just enable one at a time
        /// WHILE:
        ///     r1 = LOCK
        ///     # if LOCK = 0 then no more space for lock else we have space to acquire
        ///     PC = r1 ? WHILE
        /// # critical block start
        ///     # push an updated lock value
        ///     LOCK = DEC[r1]
        ///     # ... do your main critical statement here
        ///     
        ///     # then to release lock we just need to release our lock by INC the lock back
        ///     # note: we shouldn't use r1 anymore
        ///     LOCK = INC[LOCK]
        /// # critical block end
        /// 
        /// note: you can implement a signifcantly more efficient lock if you truly just need 1 cpu at a time by holding off on writing back the updated lock value
        /// *till* the end of the critical block (which will cause all readers to block instead of spinning).
        /// i.e.
        /// 
        /// # we never write "0" to the lock in this block we only ever write 0 (and the PIN is empty for the entire time that it's locked)
        /// LOCK
        /// # critical block here ...
        /// # (the value written to the lock doesn't matter)
        /// # guaranteed to be thread-safe since the LOCK value will be empty for this duration
        /// LOCK = 1
        /// 
        /// Which is obviously much simpler of an implementation and is a proper sleeping lock (as opposed to a spin lock).
        /// The spin lock is very useful though to implement barriers since you could implement a n barrier like this
        /// 
        /// # initial value (this could also just be set on the PIN, but this way you can also have resettable barriers)
        /// # though keep in mind that you can't write to a ReadWrite PIN that has a value already.
        /// BARRIER = 5
        /// 
        /// # reduce the barrier
        /// # this is guaranteed to be thread-safe since only one CPU can read a value at once
        /// BARRIER = DEC[BARRIER]
        /// 
        /// # then we just need to wait till it's 0
        /// WHILE:
        ///     r1 = BARRIER
        ///     # important that we write back to BARRIER because
        ///     # when it hits 0 we need all cpus to read that 0 value from barrier.
        ///     BARRIER = r1
        ///     PC = r1 ? END_BARRIER
        /// END_BARRIER:
        /// 
        /// So as you can see it's very convenient to have a ReadWrite PIN with the code looking a lot cleaner and it erases
        /// the need for a CPU component to copy the IN to OUT.
        /// </summary>
        ReadWrite = Read | Write,
    }
}
