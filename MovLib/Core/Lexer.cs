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
        public static List<Token> Tokenize(Compiler compiler, FileHandle file)
        {
            var result = new List<Token>();
            var current = new FileSpan();
            while (current.End.ByteOffset < file.Content.Length)
            {
                SkipWs();
                current.Start = current.End;

                // handle comments
                var c = file.Content[current.End.ByteOffset];

                // Comment
                if (c == '#')
                {
                    // skip tokens that are comments
                    SkipWhile(c => c != '\n');
                    // skip the '\n'
                    if (current.End.ByteOffset < file.Content.Length)
                    {
                        IncrementOffset(file.Content[current.End.ByteOffset]);
                    }
                    // drop the token by just letting it restart

                    break;
                }
                else if (c >= 128)
                {
                    // unicode isn't supported
                    compiler.ReportMessage(new CompilerMessage(MessageKind.Error, $"Invalid character '{c}', only ASCII (a-z, A-Z, 0-9, and miscellaneous symbols) is supported outside of comments", current));
                }
                else if (char.IsLetter(c))
                {
                    SkipWhile(c => char.IsLetterOrDigit(c) || c == '_');
                    result.Add(new Token(TokenKind.Identifier, current));
                }
                else if (c == '=')
                {
                    IncrementOffset(c);
                    result.Add(new Token(TokenKind.Mov, current));
                }
                else if (c == ':')
                {
                    IncrementOffset(c);
                    result.Add(new Token(TokenKind.Colon, current));
                }
                else if (c == '?')
                {
                    IncrementOffset(c);
                    result.Add(new Token(TokenKind.If, current));
                }
            }

            void IncrementOffset(char c)
            {
                current.End.ByteOffset++;
                if (c == '\n')
                {
                    current.End.Line++;
                    current.End.Col = 1;
                }
                else
                {
                    current.End.Col++;
                }
            }

            void SkipWhile(Func<char, bool> pred)
            {
                while (current.End.ByteOffset < file.Content.Length)
                {
                    if (pred(file.Content[current.End.ByteOffset]))
                    {
                        IncrementOffset(file.Content[current.End.ByteOffset]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            void SkipWs() => SkipWhile(c => c switch
            {
                ' ' => true,
                '\t' => true,
                '\n' => true,
                _ => false,
            });

            return result;
        }
    }

    public class Token
    {
        public Token(TokenKind kind, FileSpan span, object? arg = null)
        {
            Kind = kind;
            Span = span;
            Arg = arg;
        }

        public TokenKind Kind { get; set; }
        public FileSpan Span { get; }
        public object? Arg { get; set; }
    }

    public class FileHandle
    {
        public FileHandle(string content, string? fileName = null)
        {
            FileName = fileName;
            Content = content;
        }

        /// <summary>
        /// Technically filename won't always exist (i.e. tests, though they mock the filename).
        /// 
        /// And refers to a location in the VFS anyways.
        /// </summary>
        public string? FileName { get; set; }

        public string Content { get; set; }
    }

    public struct FileSpan
    {
        public FileHandle File { get; set; }

        public TokenSpan Start;
        public TokenSpan End;

        public FileSpan(FileHandle file) : this()
        {
            File = file;
            Start = (1, 1, 0);
            End = (1, 1, 0);
        }
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

    public struct TokenSpan
    {
        public int Line;
        public int Col;
        public int ByteOffset;

        public TokenSpan(int line, int col, int byteOffset)
        {
            Line = line;
            Col = col;
            ByteOffset = byteOffset;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is TokenSpan other &&
                   Line == other.Line &&
                   Col == other.Col &&
                   ByteOffset == other.ByteOffset;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Line, Col, ByteOffset);
        }

        public readonly void Deconstruct(out int line, out int col, out int byteOffset)
        {
            line = Line;
            col = Col;
            byteOffset = ByteOffset;
        }

        public static implicit operator (int Line, int Col, int ByteOffset)(TokenSpan value)
        {
            return (value.Line, value.Col, value.ByteOffset);
        }

        public static implicit operator TokenSpan((int Line, int Col, int ByteOffset) value)
        {
            return new TokenSpan(value.Line, value.Col, value.ByteOffset);
        }
    }
}
