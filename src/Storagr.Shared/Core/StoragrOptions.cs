﻿using Microsoft.Extensions.Options;

namespace Storagr
{
    public class StoragrOptions<T> : IOptions<T> 
        where T : class, new()
    {
        T IOptions<T>.Value => (T)(object)this;
    }
}