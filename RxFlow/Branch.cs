using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RxFlow
{
    public interface IBranch { }
    public interface IBranch<in T> : ISubject<T, object>, IBranch
        where T : class { }
    public class Branch<T> : IBranch<T> where T : class
    {
        private int _referenceSequenceCount = 0;
        private int _calledOnCompletedCount = 0;

        public static Branch<T> CreateBranch(Func<IObservable<T>, IObservable<object>> sequenceFactory)
        {
            return new Branch<T>(sequenceFactory);
        }
        public IObservable<object> Sequence { get; set; }
        protected ISubject<T> Subject { get; set; }
        protected Branch(Func<IObservable<T>, IObservable<object>> sequenceFactory)
        {
            Subject = new ReplaySubject<T>();
            Sequence = sequenceFactory(Subject);
        }

        public void IncrementReferencedSequenceCount()
        {
            Interlocked.Increment(ref _referenceSequenceCount);
        }
        private void IncrementCalledOnCompletedCount()
        {
            Interlocked.Increment(ref _calledOnCompletedCount);
        }

        public int ReferencedSequenceCount { get { return _referenceSequenceCount; } }
        public int CalledOnCompletedCount { get { return _calledOnCompletedCount; } }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return Sequence.Subscribe(observer);
        }

        public void OnCompleted()
        {
            IncrementCalledOnCompletedCount();
            if (CalledOnCompletedCount == ReferencedSequenceCount)
            {
                Subject.OnCompleted();
            }
        }

        public void OnError(Exception error)
        {
            Subject.OnError(error);
        }

        public void OnNext(T value)
        {
            Subject.OnNext(value);
        }
    }

    public static class BranchExtensions
    {
        public static IObservable<T> Junction<T>(this IObservable<T> source, Branch<T> branch)
           where T : class
        {
            return Junction(source, _ => true, _ => _, branch);
        }

        public static IObservable<T> Junction<T>(this IObservable<T> source, Func<T, bool> branchSelector, Branch<T> branch)
            where T : class
        {
            return Junction(source, branchSelector, _ => _, branch);
        }

        public static IObservable<TIn> Junction<TIn, TOut>(this IObservable<TIn> source, Func<TIn, TOut> converter, Branch<TOut> branch)
            where TOut : class
        {
            return Junction(source, _ => true, converter, branch);
        }

        public static IObservable<TIn> Junction<TIn, TOut>(this IObservable<TIn> source, Func<TIn, bool> branchSelector, Func<TIn, TOut> converter, Branch<TOut> branch)
            where TOut : class
        {
            var result = new ReplaySubject<TIn>();
            branch.IncrementReferencedSequenceCount();
            source.Where(branchSelector).Select(converter).Subscribe(branch);
            source.Where(value => !branchSelector(value)).Subscribe(result);
            return result;
        }
    }
}
