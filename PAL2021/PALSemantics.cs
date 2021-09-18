using System;
using AllanMilne.Ardkit;

namespace PAL2021
{
    class PALSemantics : Semantics
    {
        public PALSemantics(IParser p) : base(p)
        {
        }
        /// <summary>
        ///Method register current Token to register for CurrentScope if is type of IdentifierToken
        /// or register AlreadyDeclaredError if Identifier is defined
        /// </summary>
        /// <param name="id">Current leftToken instance</param>
        public void DeclareId(IToken id)
        {
            if (!id.Is(Token.IdentifierToken)) return; // only proceed if an identifier.
            Scope symbols = Scope.CurrentScope;
            if (symbols.IsDefined(id.TokenValue))
            {
                semanticError(new AlreadyDeclaredError(id, symbols.Get(id.TokenValue)));
            }
            else
            {
                symbols.Add(new VarSymbol(id, currentType));
            }
        }

        /// <summary>
        /// Method compares types of two given tokens where first is type target.
        /// </summary>
        /// <param name="leftToken">Token of expected type</param>
        /// <param name="rightToken">Token to be expected type of left leftToken</param>
        public void CompareTypes(IToken leftToken, IToken rightToken)
        {

            var given = TokenTypeToInt(rightToken);
            var expect = TokenTypeToInt(leftToken);

            if (given != expect)
            {
                semanticError(new TypeConflictError(rightToken, given, expect));
            }
        }

        /// <summary>
        /// Cast token sting type to int or get token type from scope
        /// </summary>
        /// <param name="token">Token to cast</param>
        /// <returns>LanguageType int value</returns>
        public int TokenTypeToInt(IToken token)
        {
            if (token != null)

                if (token.TokenType == Token.IdentifierToken)
                {
                    if (haveExist(token)) return Scope.CurrentScope.Get(token.TokenValue).Type;
                    else
                    {
                    }
                }
                else if (token.TokenType == Token.IntegerToken)
                {
                    return LanguageType.Integer;
                }
                else if (token.TokenType == Token.RealToken)
                {
                    return LanguageType.Real;
                }
                else if (token.TokenType == Token.BooleanToken)
                {
                    return LanguageType.Boolean;
                }
                else if (token.TokenType == Token.StringToken)
                {
                    return LanguageType.String;
                }

            return LanguageType.Undefined;
        }

        /// <summary>
        /// Method check is token exist in current scope
        /// if Token has been registered method register NotDeclaredUsage error
        /// </summary>
        /// <param name="id">Token given to check</param>
        /// <returns>Existence</returns>
        public bool haveExist(IToken id)
        {
            if (!Scope.CurrentScope.IsDefined(id.TokenValue))
            {
                semanticError(new NotDeclaredUsage(id));
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// NotDeclaredUsage error describes CompilerError for Null point exception
    /// </summary>
    public class NotDeclaredUsage : CompilerError
    {
        public NotDeclaredUsage(IToken id) : base(id)
        {

        }

        public override string ToString()
        {
            return $"{(object)base.ToString():s} Null point exception to variable '{(object) this.token.TokenValue:s}'.";
        }
    }
}