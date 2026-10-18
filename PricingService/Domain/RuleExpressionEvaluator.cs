using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PricingService.Domain;

internal static class RuleExpressionEvaluator
{
    public static bool EvaluateBoolean(string expression, IReadOnlyDictionary<string, object> variables)
    {
        var result = new Parser(expression, variables).Evaluate();
        return result is bool value
            ? value
            : throw new InvalidOperationException($"Rule expression '{expression}' did not evaluate to a boolean.");
    }

    public static decimal EvaluateDecimal(string expression, IReadOnlyDictionary<string, object> variables)
    {
        var result = new Parser(expression, variables).Evaluate();
        try
        {
            return Convert.ToDecimal(result, CultureInfo.InvariantCulture);
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException)
        {
            throw new InvalidOperationException($"Rule expression '{expression}' did not evaluate to a number.", exception);
        }
    }

    private sealed class Parser
    {
        private readonly string expression;
        private readonly IReadOnlyDictionary<string, object> variables;
        private readonly IReadOnlyList<Token> tokens;
        private int position;

        public Parser(string expression, IReadOnlyDictionary<string, object> variables)
        {
            this.expression = expression ?? throw new ArgumentNullException(nameof(expression));
            this.variables = variables ?? throw new ArgumentNullException(nameof(variables));
            tokens = Tokenize(expression);
        }

        public object Evaluate()
        {
            var value = ParseConditional();
            Expect(TokenType.End);
            return value();
        }

        private Func<object> ParseConditional()
        {
            var condition = ParseOr();
            if (!Match(TokenType.Question))
            {
                return condition;
            }

            var whenTrue = ParseConditional();
            Expect(TokenType.Colon);
            var whenFalse = ParseConditional();
            return () => ToBoolean(condition()) ? whenTrue() : whenFalse();
        }

        private Func<object> ParseOr()
        {
            var left = ParseAnd();
            while (Match(TokenType.OrOr))
            {
                var current = left;
                var right = ParseAnd();
                left = () => ToBoolean(current()) || ToBoolean(right());
            }

            return left;
        }

        private Func<object> ParseAnd()
        {
            var left = ParseEquality();
            while (Match(TokenType.AndAnd))
            {
                var current = left;
                var right = ParseEquality();
                left = () => ToBoolean(current()) && ToBoolean(right());
            }

            return left;
        }

        private Func<object> ParseEquality()
        {
            var left = ParseComparison();
            while (Current.Type is TokenType.EqualEqual or TokenType.BangEqual)
            {
                var operation = Advance().Type;
                var current = left;
                var right = ParseComparison();
                left = () => operation == TokenType.EqualEqual
                    ? AreEqual(current(), right())
                    : !AreEqual(current(), right());
            }

            return left;
        }

        private Func<object> ParseComparison()
        {
            var left = ParseAddition();
            while (Current.Type is TokenType.Greater or TokenType.GreaterOrEqual or TokenType.Less or TokenType.LessOrEqual)
            {
                var operation = Advance().Type;
                var current = left;
                var right = ParseAddition();
                left = () =>
                {
                    var comparison = Compare(current(), right());
                    return operation switch
                    {
                        TokenType.Greater => comparison > 0,
                        TokenType.GreaterOrEqual => comparison >= 0,
                        TokenType.Less => comparison < 0,
                        _ => comparison <= 0
                    };
                };
            }

            return left;
        }

        private Func<object> ParseAddition()
        {
            var left = ParseMultiplication();
            while (Current.Type is TokenType.Plus or TokenType.Minus)
            {
                var operation = Advance().Type;
                var current = left;
                var right = ParseMultiplication();
                left = () => operation == TokenType.Plus
                    ? ToDecimal(current()) + ToDecimal(right())
                    : ToDecimal(current()) - ToDecimal(right());
            }

            return left;
        }

        private Func<object> ParseMultiplication()
        {
            var left = ParseUnary();
            while (Current.Type is TokenType.Star or TokenType.Slash)
            {
                var operation = Advance().Type;
                var current = left;
                var right = ParseUnary();
                left = () => operation == TokenType.Star
                    ? ToDecimal(current()) * ToDecimal(right())
                    : ToDecimal(current()) / ToDecimal(right());
            }

            return left;
        }

        private Func<object> ParseUnary()
        {
            if (Match(TokenType.Bang))
            {
                var operand = ParseUnary();
                return () => !ToBoolean(operand());
            }

            if (Match(TokenType.Minus))
            {
                var operand = ParseUnary();
                return () => -ToDecimal(operand());
            }

            if (Match(TokenType.Plus))
            {
                var operand = ParseUnary();
                return () => ToDecimal(operand());
            }

            return ParsePrimary();
        }

        private Func<object> ParsePrimary()
        {
            var token = Current;
            switch (token.Type)
            {
                case TokenType.Number:
                    Advance();
                    var numericText = token.Text.TrimEnd('m', 'M');
                    var number = decimal.Parse(numericText, NumberStyles.Number, CultureInfo.InvariantCulture);
                    return () => number;
                case TokenType.String:
                    Advance();
                    var text = (string)token.Value!;
                    return () => text;
                case TokenType.Identifier:
                    Advance();
                    if (string.Equals(token.Text, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        return () => true;
                    }

                    if (string.Equals(token.Text, "false", StringComparison.OrdinalIgnoreCase))
                    {
                        return () => false;
                    }

                    return () => variables.TryGetValue(token.Text, out var value)
                        ? value
                        : throw new KeyNotFoundException($"Rule variable '{token.Text}' was not supplied.");
                case TokenType.LeftParen:
                    Advance();
                    var nested = ParseConditional();
                    Expect(TokenType.RightParen);
                    return nested;
                default:
                    throw Error($"Expected a value but found '{token.Text}'.", token.Position);
            }
        }

        private static bool AreEqual(object left, object right)
        {
            if (IsNumeric(left) && IsNumeric(right))
            {
                return ToDecimal(left) == ToDecimal(right);
            }

            return left is string leftText && right is string rightText
                ? string.Equals(leftText, rightText, StringComparison.Ordinal)
                : Equals(left, right);
        }

        private static int Compare(object left, object right)
        {
            if (IsNumeric(left) && IsNumeric(right))
            {
                return ToDecimal(left).CompareTo(ToDecimal(right));
            }

            if (left is string leftText && right is string rightText)
            {
                return string.Compare(leftText, rightText, StringComparison.Ordinal);
            }

            throw new InvalidOperationException("Rule comparisons require two numbers or two strings.");
        }

        private static bool IsNumeric(object value)
        {
            return value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;
        }

        private static decimal ToDecimal(object value)
        {
            try
            {
                return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            }
            catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException)
            {
                throw new InvalidOperationException("Rule arithmetic requires numeric values.", exception);
            }
        }

        private static bool ToBoolean(object value)
        {
            return value is bool result
                ? result
                : throw new InvalidOperationException("Rule conditions must evaluate to a boolean.");
        }

        private bool Match(TokenType type)
        {
            if (Current.Type != type)
            {
                return false;
            }

            Advance();
            return true;
        }

        private void Expect(TokenType type)
        {
            if (!Match(type))
            {
                throw Error($"Expected {type} but found '{Current.Text}'.", Current.Position);
            }
        }

        private Token Advance()
        {
            var token = Current;
            if (position < tokens.Count - 1)
            {
                position++;
            }

            return token;
        }

        private Token Current => tokens[position];

        private FormatException Error(string message, int tokenPosition)
        {
            return new FormatException($"{message} Expression: '{expression}', position: {tokenPosition}.");
        }

        private static IReadOnlyList<Token> Tokenize(string input)
        {
            var result = new List<Token>();
            var index = 0;
            while (index < input.Length)
            {
                if (char.IsWhiteSpace(input[index]))
                {
                    index++;
                    continue;
                }

                var start = index;
                var current = input[index++];
                if (char.IsDigit(current) || current == '.' && index < input.Length && char.IsDigit(input[index]))
                {
                    while (index < input.Length && (char.IsDigit(input[index]) || input[index] == '.'))
                    {
                        index++;
                    }

                    if (index < input.Length && input[index] is 'm' or 'M')
                    {
                        index++;
                    }

                    result.Add(new Token(TokenType.Number, input[start..index], null, start));
                    continue;
                }

                if (char.IsLetter(current) || current == '_')
                {
                    while (index < input.Length && (char.IsLetterOrDigit(input[index]) || input[index] == '_'))
                    {
                        index++;
                    }

                    result.Add(new Token(TokenType.Identifier, input[start..index], null, start));
                    continue;
                }

                if (current == '"')
                {
                    var text = new StringBuilder();
                    var closed = false;
                    while (index < input.Length)
                    {
                        current = input[index++];
                        if (current == '"')
                        {
                            closed = true;
                            break;
                        }

                        if (current == '\\')
                        {
                            if (index >= input.Length)
                            {
                                break;
                            }

                            current = input[index++];
                            text.Append(current switch
                            {
                                '"' => '"',
                                '\\' => '\\',
                                'n' => '\n',
                                'r' => '\r',
                                't' => '\t',
                                _ => throw new FormatException($"Invalid string escape at position {index - 1}.")
                            });
                        }
                        else
                        {
                            text.Append(current);
                        }
                    }

                    if (!closed)
                    {
                        throw new FormatException($"Unterminated string literal at position {start}.");
                    }

                    result.Add(new Token(TokenType.String, input[start..index], text.ToString(), start));
                    continue;
                }

                var (type, length) = current switch
                {
                    '+' => (TokenType.Plus, 1),
                    '-' => (TokenType.Minus, 1),
                    '*' => (TokenType.Star, 1),
                    '/' => (TokenType.Slash, 1),
                    '(' => (TokenType.LeftParen, 1),
                    ')' => (TokenType.RightParen, 1),
                    '?' => (TokenType.Question, 1),
                    ':' => (TokenType.Colon, 1),
                    '!' when Next(input, index, '=') => (TokenType.BangEqual, 2),
                    '!' => (TokenType.Bang, 1),
                    '=' when Next(input, index, '=') => (TokenType.EqualEqual, 2),
                    '>' when Next(input, index, '=') => (TokenType.GreaterOrEqual, 2),
                    '>' => (TokenType.Greater, 1),
                    '<' when Next(input, index, '=') => (TokenType.LessOrEqual, 2),
                    '<' => (TokenType.Less, 1),
                    '&' when Next(input, index, '&') => (TokenType.AndAnd, 2),
                    '|' when Next(input, index, '|') => (TokenType.OrOr, 2),
                    _ => throw new FormatException($"Unexpected character '{current}' at position {start}.")
                };

                if (length == 2)
                {
                    index++;
                }

                result.Add(new Token(type, input[start..index], null, start));
            }

            result.Add(new Token(TokenType.End, string.Empty, null, input.Length));
            return result;
        }

        private static bool Next(string input, int index, char expected)
        {
            return index < input.Length && input[index] == expected;
        }
    }

    private readonly record struct Token(TokenType Type, string Text, object? Value, int Position);

    private enum TokenType
    {
        Number,
        String,
        Identifier,
        Plus,
        Minus,
        Star,
        Slash,
        LeftParen,
        RightParen,
        Question,
        Colon,
        EqualEqual,
        BangEqual,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,
        AndAnd,
        OrOr,
        Bang,
        End
    }
}
