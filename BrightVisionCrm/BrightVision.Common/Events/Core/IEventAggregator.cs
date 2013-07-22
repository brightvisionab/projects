namespace BrightVision.Common.Events.Core
{
    using System;

    public interface IEventAggregator {
        IObservable<T> GetEvent<T>();

        void Notify<T>(T value);
    }
}
