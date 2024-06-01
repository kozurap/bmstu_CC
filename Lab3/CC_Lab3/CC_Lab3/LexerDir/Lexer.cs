using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC_Lab3.LexerDir
{
    public class Lexer
    {
        public int NextTokenPosition => GetNextTokenPosition();
        public string NextToken => _tokens[_currentIndex + 1];

        private readonly string[] _tokens;
        private int _currentIndex;

        public Lexer(string input)
        {
            _tokens = input.Split(' ');
            _currentIndex = -1;
        }

        public void MoveNext(int count = 1)
        {
            _currentIndex += count;
        }

        private int GetNextTokenPosition()
        {
            var beforeTokens = _tokens
                .Take(_currentIndex + 1)
                .ToList();

            var length = beforeTokens.Sum(token => token.Length);

            length += beforeTokens.Count;

            return length;
        }
    }
}
