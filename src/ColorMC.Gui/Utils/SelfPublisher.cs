using System;
using System.Collections.Generic;

namespace ColorMC.Gui.Utils;

public class SelfPublisher<T> : IObservable<T>
{
    private readonly List<IObserver<T>> _observers = [];

    public IDisposable Subscribe(IObserver<T> observer)
    {
        _observers.Add(observer);
        return new Unsubscribe(_observers, observer);
    }

    private class Unsubscribe(List<IObserver<T>> observers, IObserver<T> observer) : IDisposable
    {
        public void Dispose()
        {
            observers.Remove(observer);
        }
    }
    public void Notify(T data)
    {
        foreach (var observer in _observers)
        {
            observer.OnNext(data);
        }
    }
}