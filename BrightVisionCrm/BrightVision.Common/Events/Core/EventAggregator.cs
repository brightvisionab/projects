namespace BrightVision.Common.Events.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Collections.Concurrent;
    using System.Reactive.Linq;
    public class EventAggregator : IEventAggregator
    {
        private readonly ConcurrentDictionary<Type, object> _subjects = new ConcurrentDictionary<Type, object>();

        public IObservable<T> GetEvent<T>() {
            object _subject;
            if (_subjects.TryGetValue(typeof(T), out _subject))
                _subjects.TryRemove(typeof(T), out _subject);

            var subject = (ISubject<T>)_subjects.GetOrAdd(typeof(T), t => new Subject<T>());
            return subject.AsObservable();
        }

       
        public void Notify<T>(T value) {
            object subject;

            if (_subjects.TryGetValue(typeof(T), out subject)) {
                ((ISubject<T>)subject).OnNext(value);
            }
        }
    }

}
