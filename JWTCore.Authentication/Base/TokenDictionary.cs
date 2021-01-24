using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JWTCore.Authentication.Models;

namespace JWTCore.Authentication.Base
{
    public static class TokenDictionary
    {
        private readonly static IDictionary<int, HashSet<TokenDescription>> _tokens = new Dictionary<int, HashSet<TokenDescription>>();

        public static IReadOnlyDictionary<int, HashSet<TokenDescription>> Tokens
        {
            get => new ReadOnlyDictionary<int, HashSet<TokenDescription>>(_tokens);
        }

        public static void Add(int id, TokenDescription token)
        {
            lock (_tokens)
            {
                if (!_tokens.TryGetValue(id, out HashSet<TokenDescription> tokens))
                {
                    tokens = new HashSet<TokenDescription>();
                    _tokens.Add(id, tokens);
                }

                lock (tokens)
                {
                    tokens.Add(token);
                    tokens.RemoveDefeated();
                }
            }
        }

        public static IEnumerable<TokenDescription> GetTokens(int id)
        {
            if (_tokens.TryGetValue(id, out HashSet<TokenDescription> tokens))
                return tokens;
            return Enumerable.Empty<TokenDescription>();
        }

        public static void Remove(int id)
        {
            lock (_tokens)
                if (_tokens.Any(x => x.Key == id))
                    _tokens.Remove(id);
        }

        public static void Remove(int id, string token)
        {
            lock (_tokens)
            {
                if (!_tokens.TryGetValue(id, out HashSet<TokenDescription> tokens))
                    return;

                lock (tokens)
                {
                    tokens.RemoveWhere(x => x.Value.Equals(token, StringComparison.OrdinalIgnoreCase));
                    tokens.RemoveDefeated();
                    if (tokens.Count() == 0)
                        _tokens.Remove(id);
                }
            }
        }

        private static void RemoveDefeated(this HashSet<TokenDescription> tokens)
        {
            var now = DateTime.UtcNow;
            if (tokens.Count == 0)
                return;
            tokens.RemoveWhere(x => x.Expired < now);
        }
    }
}