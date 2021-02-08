using System;

namespace Taskete
{
    public interface INotifyDirty
    {
        event EventHandler Dirty;
    }
}