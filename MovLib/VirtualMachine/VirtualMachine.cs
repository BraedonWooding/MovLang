using System.Collections.Generic;

namespace MovLib.VirtualMachine
{
    /// <summary>
    /// The entire system.
    /// 
    /// Note that this may incorporate multiple other machines connected through pins.
    /// </summary>
    public class System
    {

    }

    /// <summary>
    /// Machines connect to each other through the use of IO Pins
    /// </summary>
    public class IOPin
    {
        /// <summary>
        /// IO pins support flags being set when triggering them, by default they support up to 8 flags in multi-mode or 
        /// </summary>
        public List<string> Flags { get; } = new List<string>();

        
    }

    public enum IOPinMode
    {
        None = 0,
        Read = 1 << 0,
        Write = 1 << 1,

        /// <summary>
        /// ReadWrite is confusing but it's useful in concurrent applications, this is due to it's inherent blocking behaviour.
        /// 
        /// This lets you write *simple* (compared to alternatives) locks by utilising the fact that reading/writing to a pin is blocking given it no data/has data.
        /// </summary>
        ReadWrite = Read | Write,
    }

    /// <summary>
    /// A singular piece of hardware.  This is "somewhat" hard to define and importantly may consist of multiple concurrent SOCs with separate setups.
    /// </summary>
    public class Hardware
    {

    }

    /// <summary>
    /// Programs are executed against a virtual machine.
    /// 
    /// Virtual Machines are probably not the "best" phrase here it actually refers to the entire hardware setup.
    /// </summary>
    public class VirtualMachine
    {
        // 
    }
}
