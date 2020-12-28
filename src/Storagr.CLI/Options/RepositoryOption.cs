﻿using System.CommandLine;

namespace Storagr.CLI
{
    public class RepositoryOption : Option<string>
    {
        public RepositoryOption()
            : base(new[] {"--repository"}, "")
        {
            IsRequired = true;
        }
    }
}