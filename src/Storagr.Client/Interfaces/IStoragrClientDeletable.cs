﻿namespace Storagr.Client
{
    public interface IStoragrClientDeletable<TItem>
    {
        IStoragrClientRunner<TItem> Delete();
    }
}