using System;

namespace AsmResolver.Net.Cts.Collections
{
    public class DelegatedMemberCollection<TOwner, TMember> : MemberCollection<TOwner, TMember>
        where TOwner : class, IMetadataMember
        where TMember : IMetadataMember
    {
        private readonly Func<TMember, TOwner> _getOwner;
        private readonly Action<TMember, TOwner> _setOwner;

        public DelegatedMemberCollection(TOwner owner, Func<TMember, TOwner> getOwner, Action<TMember, TOwner> setOwner)
            : base(owner)
        {
            if (getOwner == null)
                throw new ArgumentNullException("getOwner");
            if (setOwner == null)
                throw new ArgumentNullException("setOwner");
            _getOwner = getOwner;
            _setOwner = setOwner;
        }

        protected override TOwner GetOwner(TMember item)
        {
            return _getOwner(item);
        }

        protected override void SetOwner(TMember item, TOwner owner)
        {
            _setOwner(item, owner);
        }
    }
}