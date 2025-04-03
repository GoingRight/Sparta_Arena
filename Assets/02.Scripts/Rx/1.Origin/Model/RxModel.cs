using System;
using Akasha;

namespace Akasha
{
    public abstract class RxModel : IRxModel, IRxExprOwner
    {
        public object? Owner { get; private set; }

        public RxModel(object owner)
        {
            if (owner is IRxStateOwner)
                Owner = owner;
            else throw new InvalidOperationException($"[RxModel] {owner}는 RxModel를 소유할 권한이 없습니다.");
        }

    }
}