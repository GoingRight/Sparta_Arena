using System.Collections;
using System.Collections.Generic;
using Akasha;
using UnityEngine;

public abstract class BaseEntity : RxContextBehaviour, IRxStateOwner, IFiniteFieldSubscriber, IFiniteLocalEventSubscriber
{
    protected override void OnInit()
    {
        base.OnInit();
        SetupModels();
        SetupParts();
    }

    protected abstract void SetupModels();

    protected virtual void SetupParts() { }
}