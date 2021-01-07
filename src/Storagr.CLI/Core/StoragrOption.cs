using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using Storagr.Shared;

namespace Storagr.CLI
{
    public class StoragrOption : Option
    {
        public StoragrOption(string alias, string description = null) 
            : base(alias, description)
        {
        }

        public StoragrOption(string[] aliases, string description = null) 
            : base(aliases, description)
        {
        }
    }

    public class StoragrOption<T> : Option<T>
    {
        public StoragrOption(string alias, string description = null)
            : base(alias, description)
        {
        }

        public StoragrOption(string[] aliases, string description = null)
            : base(aliases, description)
        {
        }

        public StoragrOption(string alias, ParseArgument<T> parseArgument, bool isDefault = false,
            string description = null)
            : base(alias, parseArgument, isDefault, description)
        {
        }

        public StoragrOption(string[] aliases, ParseArgument<T> parseArgument, bool isDefault = false,
            string description = null)
            : base(aliases, parseArgument, isDefault, description)
        {
        }

        public StoragrOption(string alias, Func<T> getDefaultValue, string description = null)
            : base(alias, getDefaultValue, description)
        {
        }

        public StoragrOption(string[] aliases, Func<T> getDefaultValue, string description = null)
            : base(aliases, getDefaultValue, description)
        {
        }
        
        protected static ulong ParseNamedSize(ArgumentResult result)
        {
            if (result.Tokens.Count == 1)
            {
                return StoragrHelper.ParseNamedSize(result.Tokens[0].Value);
            }

            result.ErrorMessage = $"No value given.";
            return default;
        }
        
        protected static TimeSpan ParseNamedDelay(ArgumentResult result)
        {
            if (result.Tokens.Count == 1)
            {
                return StoragrHelper.ParseNamedDelay(result.Tokens[0].Value);
            }

            result.ErrorMessage = $"No value given.";
            return default;
        }
    }
}