using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;

namespace RxFlow
{
    public class FlowContext
    {
        public IList<IFlow<object>> Sequences { get; private set; }
        public IList<IBranch> Junctions { get; private set; }

        public event EventHandler Executed;

        public FlowContext()
        {
            Sequences = new List<IFlow<object>>();
            Junctions = new List<IBranch>();
        }

        public Branch<T> CreateBranch<T>(Func<IObservable<T>, IObservable<object>> sequenceFactory) where T : class
        {
            var result = Branch<T>.CreateBranch(sequenceFactory);
            Junctions.Add(result);
            return result;
        }

        public Flow<T> CreateSequence<T>(IObservable<T> source, Func<IObservable<T>, IObservable<object>> sequenceFactory) where T : class
        {
            var result = Flow<T>.CreateSequence(source, sequenceFactory);
            Sequences.Add(result);
            return result;
        }

        public IDisposable Start(Action<object> onNext, bool isParallel = false)
        {
            return Run(new AnonymousObserver<object>(onNext, ex => { }, () => { }), isParallel);
        }

        public IDisposable Start(Action<object> onNext, Action onCompleted, bool isParallel = false)
        {
            return Run(new AnonymousObserver<object>(onNext, ex => { }, onCompleted), isParallel);
        }

        public IDisposable Start(Action<object> onNext, Action<Exception> onError, Action onCompleted, bool isParallel = false)
        {
            return Run(new AnonymousObserver<object>(onNext, onError, onCompleted), isParallel);
        }

        public IDisposable Run(IObserver<object> observer = null, bool isParallel = false)
        {
            var argument = observer ?? new AnonymousObserver<object>(_ => { });

            var sources = Junctions.Cast<IObservable<object>>().Concat(Sequences).Merge();
            var disposable = sources.Subscribe(argument.OnNext, argument.OnError, () => { argument.OnCompleted(); RaiseExecuted(); });

            if (isParallel)
            {
                Task.WaitAll(Sequences.ToObservable().Do(seq => seq.ConnectableObservable.Connect()).ToTask());
                return disposable;
            }

            foreach (var batchSequence in Sequences)
            {
                batchSequence.ConnectableObservable.Connect();
            }

            return disposable;
        }

        private void RaiseExecuted()
        {
            var handler = Executed;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
