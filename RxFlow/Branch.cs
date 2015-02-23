using System;
using System.Reactive;
using System.Reactive.Subjects;

namespace RxFlow
{
    public static class Branch
    {
        public static Branch<T> CreateBranch<T>(Func<IObservable<T>, IDisposable> sequenceFactory)
        {
            var result = Branch<T>.CreateBranch(sequenceFactory);
            return result;
        }
    }

    public class Branch<T>
    {
        private readonly Func<IObservable<T>, IDisposable> _sequenceFactory;

        protected Branch(Func<IObservable<T>, IDisposable> sequenceFactory)
        {
            _sequenceFactory = sequenceFactory;
        }

        public static Branch<T> CreateBranch(Func<IObservable<T>, IDisposable> sequenceFactory)
        {
            return new Branch<T>(sequenceFactory);
        }

        public IObserver<T> GetObserver()
        {
            var subject = new ReplaySubject<T>();
            _sequenceFactory(subject);
            return subject;
        }
    }

    public static class BranchExtensions
    {
        public static IObservable<T> Junction<T>(this IObservable<T> source, Branch<T> branch)
            where T : class
        {
            return Junction(source, _ => true, _ => _, branch);
        }

        public static IObservable<T> Junction<T>(this IObservable<T> source, Func<T, bool> branchSelector,
            Branch<T> branch)
        {
            return Junction(source, branchSelector, _ => _, branch);
        }

        public static IObservable<TIn> Junction<TIn, TOut>(this IObservable<TIn> source, Func<TIn, TOut> converter,
            Branch<TOut> branch)
        {
            return Junction(source, _ => true, converter, branch);
        }

        public static IObservable<TIn> Junction<TIn, TOut>(this IObservable<TIn> source, Func<TIn, bool> branchSelector,
            Func<TIn, TOut> converter, Branch<TOut> branch)
        {
            var branchObserver = branch.GetObserver();
            return new AnonymousObservable<TIn>(observer =>
            {
                return source.Subscribe(new AnonymousObserver<TIn>(
                    value =>
                    {
                        if (branchSelector(value))
                        {
                            branchObserver.OnNext(converter(value));
                        }
                        else
                        {
                            observer.OnNext(value);
                        }
                    },
                    ex =>
                    {
                        branchObserver.OnError(ex);
                        observer.OnError(ex);
                    },
                    () =>
                    {
                        branchObserver.OnCompleted();
                        observer.OnCompleted();
                    }));
            });
        }

        public static IObservable<T> Distribution<T>(this IObservable<T> source, Branch<T> branch)
        {
            return Distribution(source, _ => true, _ => _, branch);
        }

        public static IObservable<T> Distribution<T>(this IObservable<T> source, Func<T, bool> branchSelector,
            Branch<T> branch)
        {
            return Distribution(source, branchSelector, _ => _, branch);
        }

        public static IObservable<TIn> Distribution<TIn, TOut>(this IObservable<TIn> source, Func<TIn, TOut> converter,
            Branch<TOut> branch)
        {
            return Distribution(source, _ => true, converter, branch);
        }

        public static IObservable<TIn> Distribution<TIn, TOut>(this IObservable<TIn> source,
            Func<TIn, bool> branchSelector, Func<TIn, TOut> converter, Branch<TOut> branch)
        {
            var branchObserver = branch.GetObserver();
            return new AnonymousObservable<TIn>(observer =>
            {
                return source.Subscribe(new AnonymousObserver<TIn>(
                    value =>
                    {
                        if (branchSelector(value))
                        {
                            branchObserver.OnNext(converter(value));
                        }
                        observer.OnNext(value);
                    },
                    ex =>
                    {
                        branchObserver.OnError(ex);
                        observer.OnError(ex);
                    },
                    () =>
                    {
                        branchObserver.OnCompleted();
                        observer.OnCompleted();
                    }));
            });
        }
    }
}