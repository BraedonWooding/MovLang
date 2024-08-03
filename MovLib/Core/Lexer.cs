using System;
using System.Collections.Generic;
using System.Text;

namespace MovLib.Core
{
    /// <summary>
    /// Splits our program into a series of tokens
    /// </summary>
    public static class Lexer
    {
        public List<Token> Tokenize(string file)
        {

        }
    }

    public class Token
    {
        public Token(TokenKind kind, object? arg = null)
        {
            Kind = kind;
            Arg = arg;
        }

        public TokenKind Kind { get; set; }
        public object? Arg { get; set; }
    }

    public class FileHandle
    {
        /// <summary>
        /// Technically filename won't always exist (i.e. tests, though they mock the filename).
        /// 
        /// And refers to a location in the VFS anyways.
        /// </summary>
        public string? FileName { get; set; }
    }

    public struct FileSpan
    {
        public FileHandle File { get; set; }

        public (int Row, int Col) Start {  get; set; }
        public (int Row, int Col) End { get; set; }
    }

    public enum TokenKind
    {
        /// <summary>
        /// =, our one instruction
        /// </summary>
        Mov,

        /// <summary>
        /// ?, this is used to make cmovs
        /// i.e. r1 = r2 ? 1, will move the value 1 into r1 if r2 != 0
        /// </summary>
        If,

        /// <summary>
        /// At this point it could be a macro reference, a register, a pin, a table, or something else.
        /// 
        /// So it's just an "identifier"
        /// </summary>
        Identifier,

        /// <summary>
        /// Used to identify labels
        /// </summary>
        Colon,
    }
}
