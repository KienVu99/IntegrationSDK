using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace aCode.Editor.Utils
{
	public abstract class JsonUtils
	{
        public static object Deserialize(string json)
        {
            return string.IsNullOrEmpty(json) ? null : Parser.Parse(json);
        }

        private sealed class Parser : IDisposable
        {
            private const string WORD_BREAK = "{}[],:\"";

            private static bool IsWordBreak(char c)
            {
                return char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
            }

            private enum Token { None, CurlyOpen, CurlyClose, SquaredOpen, SquaredClose, Colon, Comma, String, Number, True, False, Null };

            private StringReader _json;

            private Parser(string jsonString)
            {
                _json = new StringReader(jsonString);
            }

            public static object Parse(string jsonString)
            {
                using var instance = new Parser(jsonString);
                return instance.ParseValue();
            }

            public void Dispose()
            {
                _json.Dispose();
                _json = null;
            }

            private Dictionary<string, object> ParseObject()
            {
                var table = new Dictionary<string, object>();
                _json.Read();
                while (true)
                {
                    switch (NextToken)
                    {
                        case Token.None:
                            return null;
                        case Token.Comma:
                            continue;
                        case Token.CurlyClose:
                            return table;
                        case Token.CurlyOpen:
                        case Token.SquaredOpen:
                        case Token.SquaredClose:
                        case Token.Colon:
                        case Token.String:
                        case Token.Number:
                        case Token.True:
                        case Token.False:
                        case Token.Null:
                        default:
                            var name = ParseString();
                            if (name == null)  return null;
                            if (NextToken != Token.Colon) return null;
                            _json.Read();
                            table[name] = ParseValue();
                            break;
                    }
                }
            }

            private List<object> ParseArray()
            {
                var array = new List<object>();
                _json.Read();
                var parsing = true;
                while (parsing)
                {
                    var nextToken = NextToken;
                    switch (nextToken)
                    {
                        case Token.None:
                            return null;
                        case Token.Comma:
                            continue;
                        case Token.SquaredClose:
                            parsing = false;
                            break;
                        case Token.CurlyOpen:
                        case Token.CurlyClose:
                        case Token.SquaredOpen:
                        case Token.Colon:
                        case Token.String:
                        case Token.Number:
                        case Token.True:
                        case Token.False:
                        case Token.Null:
                        default:
                            var value = ParseByToken(nextToken);
                            array.Add(value);
                            break;
                    }
                }

                return array;
            }

            private object ParseValue()
            {
                var nextToken = NextToken;
                return ParseByToken(nextToken);
            }

            private object ParseByToken(Token token)
            {
                return token switch
                {
                    Token.String => ParseString(),
                    Token.Number => ParseNumber(),
                    Token.CurlyOpen => ParseObject(),
                    Token.SquaredOpen => ParseArray(),
                    Token.True => true,
                    Token.False => false,
                    Token.Null => null,
                    _ => null
                };
            }

            private string ParseString()
            {
                var s = new StringBuilder();
                _json.Read();

                var parsing = true;
                while (parsing)
                {
                    if (_json.Peek() == -1)
                    {
                        parsing = false;
                        break;
                    }
                    var c = NextChar;
                    switch (c)
                    {
                        case '"':
                            parsing = false;
                            break;
                        case '\\':
                            if (_json.Peek() == -1)
                            {
                                parsing = false;
                                break;
                            }

                            c = NextChar;
                            switch (c)
                            {
                                case '"':
                                case '\\':
                                case '/':
                                    s.Append(c);
                                    break;
                                case 'b':
                                    s.Append('\b');
                                    break;
                                case 'f':
                                    s.Append('\f');
                                    break;
                                case 'n':
                                    s.Append('\n');
                                    break;
                                case 'r':
                                    s.Append('\r');
                                    break;
                                case 't':
                                    s.Append('\t');
                                    break;
                                case 'u':
                                    var hex = new char[4];

                                    for (var i = 0; i < 4; i++)
                                    {
                                        hex[i] = NextChar;
                                    }

                                    s.Append((char) Convert.ToInt32(new string(hex), 16));
                                    break;
                            }

                            break;
                        default:
                            s.Append(c);
                            break;
                    }
                }

                return s.ToString();
            }

            private object ParseNumber()
            {
                var number = NextWord;
                if (number.IndexOf('.') == -1)
                {
                    long.TryParse(number, out var parsedInt);
                    return parsedInt;
                }
                double.TryParse(number, out var parsedDouble);
                return parsedDouble;
            }

            private void EatWhitespace()
            {
                while (char.IsWhiteSpace(PeekChar))
                {
                    _json.Read();

                    if (_json.Peek() == -1)
                    {
                        break;
                    }
                }
            }

            private char PeekChar => Convert.ToChar(_json.Peek());

            private char NextChar => Convert.ToChar(_json.Read());

            private string NextWord
            {
                get
                {
                    var word = new StringBuilder();

                    while (!IsWordBreak(PeekChar))
                    {
                        word.Append(NextChar);

                        if (_json.Peek() == -1)
                        {
                            break;
                        }
                    }

                    return word.ToString();
                }
            }

            private Token NextToken
            {
                get
                {
                    EatWhitespace();

                    if (_json.Peek() == -1)
                    {
                        return Token.None;
                    }

                    switch (PeekChar)
                    {
                        case '{':
                            return Token.CurlyOpen;
                        case '}':
                            _json.Read();
                            return Token.CurlyClose;
                        case '[':
                            return Token.SquaredOpen;
                        case ']':
                            _json.Read();
                            return Token.SquaredClose;
                        case ',':
                            _json.Read();
                            return Token.Comma;
                        case '"':
                            return Token.String;
                        case ':':
                            return Token.Colon;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case '-':
                            return Token.Number;
                    }

                    return NextWord switch
                    {
                        "false" => Token.False,
                        "true" => Token.True,
                        "null" => Token.Null,
                        _ => Token.None
                    };
                }
            }
        }

        public static string Serialize(object obj, bool prettyPrint = false)
        {
            return Serializer.SerializeJson(obj, prettyPrint);
        }

        private sealed class Serializer
        {
            private readonly StringBuilder _builder;
            private readonly bool _prettyPrint;
            private int _indentLevel;

            private Serializer(bool prettyPrint)
            {
                _builder = new StringBuilder();
                this._prettyPrint = prettyPrint;
                _indentLevel = 0;
            }

            public static string SerializeJson(object obj, bool prettyPrint)
            {
                var instance = new Serializer(prettyPrint);

                instance.SerializeValue(obj);

                return instance._builder.ToString();
            }

            private void SerializeValue(object value)
            {
                IList asList;
                IDictionary asDict;
                string asStr;

                if (value == null)
                {
                    _builder.Append("null");
                }
                else if ((asStr = value as string) != null)
                {
                    SerializeString(asStr);
                }
                else if (value is bool)
                {
                    _builder.Append((bool) value ? "true" : "false");
                }
                else if ((asList = value as IList) != null)
                {
                    SerializeArray(asList);
                }
                else if ((asDict = value as IDictionary) != null)
                {
                    SerializeObject(asDict);
                }
                else if (value is char)
                {
                    SerializeString(new string((char) value, 1));
                }
                else
                {
                    SerializeOther(value);
                }
            }

            private void SerializeObject(IDictionary obj)
            {
                var first = true;

                _builder.Append('{');

                _indentLevel++;
                if (_prettyPrint)
                {
                    _builder.AppendLine();
                }

                foreach (var e in obj.Keys)
                {
                    if (!first)
                    {
                        _builder.Append(',');
                        if (_prettyPrint)
                        {
                            _builder.AppendLine();
                        }
                    }

                    if (_prettyPrint)
                    {
                        _builder.Append(new string(' ', _indentLevel * 4));
                    }

                    SerializeString(e.ToString());
                    _builder.Append(':');
                    if (_prettyPrint)
                    {
                        _builder.Append(' ');
                    }

                    SerializeValue(obj[e]);

                    first = false;
                }

                _indentLevel--;

                if (_prettyPrint)
                {
                    _builder.AppendLine();
                    _builder.Append(new string(' ', _indentLevel * 4));
                }

                _builder.Append('}');
            }

            private void SerializeArray(IList anArray)
            {
                _builder.Append('[');

                _indentLevel++;
                if (_prettyPrint)
                {
                    _builder.AppendLine();
                }

                var first = true;

                foreach (var obj in anArray)
                {
                    if (!first)
                    {
                        _builder.Append(',');
                        if (_prettyPrint)
                        {
                            _builder.AppendLine();
                        }
                    }

                    if (_prettyPrint)
                    {
                        _builder.Append(new string(' ', _indentLevel * 4));
                    }

                    SerializeValue(obj);

                    first = false;
                }

                _indentLevel--;
                if (_prettyPrint)
                {
                    _builder.AppendLine();
                    _builder.Append(new string(' ', _indentLevel * 4));
                }

                _builder.Append(']');
            }

            private void SerializeString(string str)
            {
                _builder.Append('\"');

                var charArray = str.ToCharArray();
                foreach (var c in charArray)
                {
                    switch (c)
                    {
                        case '"':
                            _builder.Append("\\\"");
                            break;
                        case '\\':
                            _builder.Append("\\\\");
                            break;
                        case '\b':
                            _builder.Append("\\b");
                            break;
                        case '\f':
                            _builder.Append("\\f");
                            break;
                        case '\n':
                            _builder.Append("\\n");
                            break;
                        case '\r':
                            _builder.Append("\\r");
                            break;
                        case '\t':
                            _builder.Append("\\t");
                            break;
                        default:
                            var codepoint = Convert.ToInt32(c);
                            if ((codepoint >= 32) && (codepoint <= 126))
                            {
                                _builder.Append(c);
                            }
                            else
                            {
                                _builder.Append("\\u");
                                _builder.Append(codepoint.ToString("x4"));
                            }

                            break;
                    }
                }

                _builder.Append('\"');
            }

            private void SerializeOther(object value)
            {
                if (value is float) {
                    _builder.Append(((float) value).ToString("R"));
                }
                else if (value is int || value is uint || value is long || value is sbyte || value is byte || value is short || value is ushort || value is ulong) {
                    _builder.Append(value);
                } else if (value is double || value is decimal){
                    _builder.Append(Convert.ToDouble(value).ToString("R"));
                } else {
                    SerializeString(value.ToString());
                }
            }
        }
	}
}