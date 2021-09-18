using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using AllanMilne.Ardkit;

namespace PAL2021
{
    class PALParser : RecoveringRdParser
    {
        private PALSemantics _semantics;

        public PALParser() : base(new PALScanner())
        {
            _semantics = new PALSemantics(this);
        }

        //<Program>
        protected override void recStarter()
        {
            Scope.OpenScope();
            mustBe("PROGRAM");
            mustBe(Token.IdentifierToken); //program name - no semantic
            mustBe("WITH");
            recVarDecls();
            mustBe("IN");
            recStatemets();
            mustBe("END");
            Scope.CloseScope();
        }

        //<VarDecls>
        private void recVarDecls()
        {
            do
            {
                var identLists = recIdentLists();
                mustBe("AS");
                recType();
                foreach (IToken token in identLists)
                    _semantics.DeclareId(token);
            } while (have(Token.IdentifierToken));
        }

        //<IdentList>
        private List<IToken> recIdentLists()
        {
            var tokens = new List<IToken> {scanner.CurrentToken};
            mustBe(Token.IdentifierToken);
            while (have(","))
            {
                mustBe(",");
                if(have(Token.IdentifierToken))
                    tokens.Add(scanner.CurrentToken);
                mustBe(Token.IdentifierToken);
            }

            return tokens;
        }

        //<Type>
        private void recType()
        {
            if (have("REAL"))
            {
                mustBe("REAL");
                _semantics.CurrentType = LanguageType.Real;
            }
            else if (have("INTEGER"))
            {
                mustBe("INTEGER");
                _semantics.CurrentType = LanguageType.Integer;
            }
            else syntaxError("<Type>");
        }

        //<Statement>
        private void recStatemets()
        {
            do
            {
                if (have(Token.IdentifierToken)) recAssignment();
                else if (have("UNTIL")) recLoop();
                else if (have("IF")) recConditional();
                else if (have("INPUT") || have("OUTPUT")) recIO();
                else syntaxError("<Statement>");
            } while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || have("OUTPUT"));
        }

        //<Assignment>
        private void recAssignment()
        {
            var token = scanner.CurrentToken;
            _semantics.haveExist(token);
            mustBe(Token.IdentifierToken);
            mustBe("=");
            var tokenType2 = recExpression();
            _semantics.CompareTypes(token, tokenType2);
        }

        //<Expression>
        private IToken recExpression()
        {
            var typeToken1 = recTerm();
            while (have("+") || have("-"))
            {
                mustBe(have("+") ? "+" : "-");
                var typeToken2 = recTerm();
                _semantics.CompareTypes(typeToken1, typeToken2);
            }

            return typeToken1;
        }

        //<Term>
        private IToken recTerm()
        {
            var typeToken1 = recFactor();
            while (have("*") || have("/"))
            {
                mustBe(have("*") ? "*" : "/");

                var typeToken2 = recFactor();
                _semantics.CompareTypes(typeToken1, typeToken2);
            }

            return typeToken1;
        }

        //<Factor>
        private IToken recFactor()
        {
            IToken token;
            if (have("+") || have("-"))
                mustBe(have("+") ? "+" : "-");

            if (have(Token.IdentifierToken) || have(Token.IntegerToken) || have(Token.RealToken))
            {
                token = recValue();
            }
            else if (have("("))
            {
                mustBe("(");
                token = recExpression();
                mustBe(")");
            }
            else
            {
                syntaxError("<Factor>");
                return null;
            }

            return token;
        }

        //<Value>
        private IToken recValue()
        {
            var token = scanner.CurrentToken;
            if (have(Token.IdentifierToken))
            {
                mustBe(Token.IdentifierToken);
            }
            else if (have(Token.IntegerToken))
            {
                mustBe(Token.IntegerToken);
            }
            else if (have(Token.RealToken))
            {
                mustBe(Token.RealToken);
            }
            else
            {
                syntaxError("<Value>"); 
            }

            return token;
        }

        //<Loop>
        private void recLoop()
        {
            mustBe("UNTIL");
            recBooleanExpr();
            mustBe("REPEAT");
            while ((have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || have("OUTPUT")))
            {
                recStatemets();
            }

            mustBe("ENDLOOP");
        }

        //<BooleanExpr>
        private void recBooleanExpr()
        {
            var token1 = recExpression();
            if (have("<"))
                mustBe("<");
            else if (have("="))
                mustBe("=");
            else if (have(">"))
                mustBe(">");
            else syntaxError("<BooleanExpr>");
            var token2 = recExpression();
            _semantics.CompareTypes(token1, token2);
        }

        //<Conditional>
        private void recConditional()
        {
            mustBe("IF");
            recBooleanExpr();
            mustBe("THEN");
            while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || have("OUTPUT"))
            {
                recStatemets();
            }

            if (have("ELSE"))
            {
                mustBe("ELSE");
                while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || have("OUTPUT"))
                {
                    recStatemets();
                }
            }

            mustBe("ENDIF");
        }

        //<I-o>
        private void recIO()
        {
            if (have("INPUT"))
            {
                mustBe("INPUT");
                var identLists = recIdentLists();
                foreach (var token in identLists)
                {
                    _semantics.haveExist(token);
                }
            }
            else if (have("OUTPUT"))
            {
                mustBe("OUTPUT");
                var expression = recExpression();
                //_semantics.haveExist(expression);
                while (have(","))
                {
                    mustBe(",");
                    recExpression();
                }
            }
        }
    }
}