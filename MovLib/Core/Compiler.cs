using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovLib.Core
{
    /// <summary>
    /// A program consists of a series of modules which are each independently compiled (but typically *against* a target)
    /// </summary>
    public class Program
    {
        public List<ProgramBlock> Blocks { get; } = new List<ProgramBlock>();
    }

    public class Table
    {
        /// <summary>
        /// The code that builds the table.
        /// 
        /// A very simple example would be an INC table which would be written as
        /// INC[0] = 1
        /// INC[1] = 2
        /// ...
        /// 
        /// We however give the user the INC & DEC tables to simplify things since with those
        /// you can typically write most other tables relatively easily.
        /// 
        /// For example an is even table would be written as
        /// 
        /// r1 = 9999
        /// # stores 1 if the number is even else odd
        /// r2 = 0
        /// 
        /// WHILE:
        ///     # exit loop if we hit 0
        ///     PC = r1 ? END
        ///     IS_EVEN[r1] = r2
        ///     # also do the negative
        ///     IS_EVEN[NEG[r1]] = r2
        ///     # next number
        ///     r1 = DEC[r1]
        /// 
        ///     # if r2 is 1 then the number is even
        ///     PC = r2 ? IS_EVEN
        /// # this also could be implemented by using a INV_BOOL
        /// IS_ODD:
        ///     # fallthrough (we are now odd i.e. r2 = 0)
        ///     # next number will be even
        ///     r2 = 1
        /// IS_EVEN:
        ///     # next number will be odd
        ///     r2 = 1
        /// END:
        /// 
        ///     # let's just say that 0 is even
        ///     IS_EVEN[0] = 0
        ///     
        /// And for the sake of showing NEG (negate) which is much simpler:
        /// NEG[0] = 0
        /// r1 = 9999
        /// r2 = -9999
        /// WHILE:
        ///     PC = r1 ? END
        ///     NEG[r1] = r2
        ///     NEG[r2] = r1
        ///     r1 = DEC[r1]
        ///     r2 = INC[r2]
        /// END:
        /// </summary>
        public string? Content { get; set; }
    }

    public class ProgramBlock
    {
        /// <summary>
        /// Programs 
        /// </summary>
        public string Function { get; set; }

        /// <summary>
        /// The actual code that can be executed.
        /// </summary>
        public string Content { get; set; }
    }

    public class Compiler
    {
        private readonly List<CompilerMessage> _messages = new List<CompilerMessage>();

        public void Compile()
        {

        }

        public void ReportMessage(CompilerMessage message)
        {
            _messages.Add(message);
        }

        public bool HasMessages(MessageKind kind)
        {
            return _messages.Any(m => m.Kind == kind);
        }
    }

    public struct CompilerMessage
    {
        public MessageKind Kind;
        public string Message;
        public FileSpan Span;

        public CompilerMessage(MessageKind kind, string message, FileSpan span)
        {
            Kind = kind;
            Message = message;
            Span = span;
        }
    }

    public enum MessageKind
    {
        Info,
        Warning,
        Error,
    }
}
