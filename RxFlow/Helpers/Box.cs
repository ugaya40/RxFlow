using System;
using System.Reactive.Linq;

namespace RxFlow
{
    public class Box<T> where T : struct
    {
        public Box(T value)
        {
            Value = value;
        }
        public T Value { get; set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public static class BoxExtensions
    {
        public static IObservable<Box<T>> ToBox<T>(this IObservable<T> source) where T : struct
        {
            return source.Select(i => new Box<T>(i));
        }

        public static Box<T> ToBox<T>(this T source) where T : struct
        {
            return new Box<T>(source);
        }
    }
}
