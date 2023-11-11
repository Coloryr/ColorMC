using System;
using System.Collections.Generic;

namespace ColorMC.Gui.Utils;

public class SelfPublisher<T> : IObservable<T>
{
    private readonly List<IObserver<T>> _observers = new();

    public IDisposable Subscribe(IObserver<T> observer)
    {
        _observers.Add(observer);
        return new Unsubscribe(_observers, observer);
    }

    private class Unsubscribe : IDisposable
    {
        private readonly List<IObserver<T>> _observers;
        private readonly IObserver<T> _observer;
        public Unsubscribe(List<IObserver<T>> observers, IObserver<T> observer)
        {
            _observer = observer;
            _observers = observers;
        }

        public void Dispose()
        {
            _observers?.Remove(_observer);
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