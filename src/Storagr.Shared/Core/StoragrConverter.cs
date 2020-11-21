using System;
using System.Collections.Generic;
using Storagr.Shared.Data;

namespace Storagr.Shared
{
    public class StoragrAction
    {
        public string Href;
        public int ExpiresIn;
        public DateTime ExpiresAt;
        public IDictionary<string, string> Header;
    }
    
    public static class StoragrConverter
    {
        public static BatchAction ToBatchAction(StoragrAction request) => new BatchAction()
        {
            Href = request.Href,
            Header = request.Header,
            ExpiresAt = request.ExpiresAt,
            ExpiresIn = request.ExpiresIn
        };
    }
}