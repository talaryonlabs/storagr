using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Storagr.Data.Entities;

namespace Storagr.Controllers.Models
{
    [DataContract]
    public class LockModel
    {
        [DataMember(Name = "id")] public string LockId;
        [DataMember(Name = "path")] public string Path;
        [DataMember(Name = "locked_at")] public string LockedAt;
        [DataMember(Name = "owner")] public OwnerData Owner;

        public static implicit operator LockModel([NotNull] LockEntity entity) => new LockModel()
        {
            LockId = entity.LockId,
            Path = entity.Path,
            LockedAt = entity.LockedAt.ToString(CultureInfo.CurrentCulture), // TODO return correct string
            Owner = new OwnerData()
            {
                Name = entity.Owner.Username
            }
        };
    }

    [DataContract]
    public class LockRequest
    {
        [DataMember(Name="path", IsRequired = true)] public string Path;
        [DataMember(Name = "ref")] public RefData Ref;
    }

    [DataContract]
    public class LockResponse
    {
        [DataMember(Name = "lock")] public LockModel Lock;
    }

    [DataContract]
    public class UnlockRequest
    {
        [DataMember(Name = "force")] public bool Force;
        [DataMember(Name = "ref")] public RefData Ref;
    }

    [DataContract]
    public class UnlockResponse
    {
        [DataMember(Name = "lock")] public LockModel Lock;
    }

    [DataContract]
    public class LockListRequest
    {
        [FromQuery(Name = "path")] public string Path { get; set; }
        [FromQuery(Name = "id")] public string LockId { get; set; }
        [FromQuery(Name = "cursor")] public string Cursor { get; set; }
        [FromQuery(Name = "limit")] public int Limit { get; set; }
        [FromQuery(Name = "refspec")] public string RefSpec { get; set; }
    }

    [DataContract]
    public class LockListResponse
    {
        [DataMember(Name = "locks")] public IEnumerable<LockModel> Locks;
        [DataMember(Name = "next_cursor")] public string NextCursor;
    }

    [DataContract]
    public class LockVerifyListRequest
    {
        [DataMember(Name = "cursor")] public string Cursor;
        [DataMember(Name = "limit")] public int Limit;
        [DataMember(Name = "ref")] public RefData Ref;
    }

    [DataContract]
    public class LockVerifyListResponse
    {
        [DataMember(Name = "ours")] public List<LockModel> Ours;
        [DataMember(Name = "theirs")] public List<LockModel> Theirs;
        [DataMember(Name = "next_cursor")] public string NextCursor;
    }
}