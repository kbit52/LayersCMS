﻿using LayersCMS.Data.Domain.Core.Security;
using LayersCMS.Data.Persistence.Interfaces.Reads.Base;

namespace LayersCMS.Data.Persistence.Interfaces.Reads
{
    public interface ILayersCmsUserReads : ILayersCmsReads<LayersCmsUser>
    {
        LayersCmsUser GetForLogin(string emailAddress, string plainTextPassword);
        LayersCmsUser GetByEmailAddress(string emailAddress);
        LayersCmsUser GetByEmailAddress(string emailAddress, int excludeUserId);
    }
}