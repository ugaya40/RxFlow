using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace RxFlow
{
    public interface IFlow<out T> : IObservable<T>
        where T : class
    {
        IConnectableObservable<T> ConnectableObservable { get; }
    }
    public class Flow<T> : IFlow<T> where T : class
    {
        public static Flow<T> CreateSequence(IObservable<T> source, Func<IObservable<T>, IObservable<object>> sequenceFactory)
        {
            return new Flow<T>(source, sequenceFactory);
        }

        protected Flow(IObservable<T> source, Func<IObservable<T>, IObservable<object>> sequenceFactory)
        {
            ConnectableObservable = source.Publish();
            Sequence = sequenceFactory(ConnectableObservable);
        }

        public IObservable<object> Sequence { get; protected set; }
        public IConnectableObservable<T> ConnectableObservable { get; private set; }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return Sequence.Select(o => (T)o).Subscribe(observer);
        }
    }
}
