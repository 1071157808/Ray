﻿using ProtoBuf;
using Ray.Core.EventSourcing;
using System;

namespace RayTest.IGrains.States
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class AccountState : IState<long>, ITransactionable<AccountState>
    {
        #region base
        public long StateId { get; set; }
        public Int64 Version { get; set; }
        public Int64 DoingVersion { get; set; }
        public DateTime VersionTime { get; set; }
        #endregion
        public decimal Balance { get; set; }

        public AccountState DeepCopy()
        {
            return new AccountState
            {
                StateId = StateId,
                Version = Version,
                DoingVersion = DoingVersion,
                VersionTime = VersionTime,
                Balance = Balance
            };
        }
    }
}
