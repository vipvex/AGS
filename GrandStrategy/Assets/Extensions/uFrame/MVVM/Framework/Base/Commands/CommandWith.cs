using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UniRx;
///// <summary>
///// A command with an argument of type T.
///// Not usually bound to directly but used to forward a command to a parent viewmodel
///// </summary>
///// <typeparam name="T">The argument parameter.</typeparam>
//public class CommandWith<T> : ICommandWith<T>
//{
//    public event CommandEvent OnCommandExecuted;

//    public event CommandEvent OnCommandExecuting;


//    public void Execute(object parameter)
//    {
//        Parameter = parameter;
//    }

//    public bool CanExecute(object parameter)
//    {
//        throw new NotImplementedException();
//    }

//    public object Sender { get; set; }

//    public object Parameter { get; set; }

//    protected Action<T> Delegate { get; set; }

//    public CommandWith(Action<T> @delegate)
//    {
//        Delegate = @delegate;
//    }

//    public CommandWith(T parameter, Action<T> @delegate)
//    {
//        Parameter = parameter;
//        Delegate = @delegate;
//    }

//    //public static implicit operator CommandWith<T>(Action<T> e)
//    //{
//    //    return new CommandWith<T>(e);
//    //}

//    public virtual void Execute()
//    {
//        OnOnCommandExecuting();
//        if (Delegate != null)
//            Delegate((T)Parameter);

//        OnOnCommandComplete();
//    }

//    protected virtual void OnOnCommandComplete()
//    {
//        CommandEvent handler = OnCommandExecuted;
//        if (handler != null) handler();
//    }

//    protected virtual void OnOnCommandExecuting()
//    {
//        CommandEvent handler = OnCommandExecuting;
//        if (handler != null) handler();
//    }
//    public void OnCompleted()
//    {

//    }

//    public void OnError(Exception error)
//    {
//        throw error;
//    }

//    public void OnNext(Unit value)
//    {
//        Execute();
//    }

//    public IDisposable Subscribe(IObserver<Unit> observer)
//    {
//        CommandEvent handler = () => observer.OnNext(Unit.Default);
//        this.OnCommandExecuted += handler;

//        return Disposable.Create(() => OnCommandExecuted -= handler);
//    }
//}
[Obsolete]
public abstract class CommandBase<TArgument> : ICommandWith<TArgument>
{
    private SimpleSubject<TArgument> _executedSubject = new SimpleSubject<TArgument>();

    public void OnCompleted()
    {
        _executedSubject.OnCompleted();
    }

    public void OnError(Exception error)
    {
        _executedSubject.OnError(error);
    }

    public void OnNext(TArgument value)
    {
        Execute(value);
    }

    public void OnNext(Unit value)
    {
        OnNext(default(TArgument));
    }

    public IDisposable Subscribe(IObserver<Unit> observer)
    {
        return _executedSubject.Select(_ => Unit.Default).Subscribe(observer);
    }

    public event CommandEvent OnCommandExecuted;

    protected virtual void OnOnCommandExecuted()
    {
        CommandEvent handler = OnCommandExecuted;
        if (handler != null) handler();
    }

    public event CommandEvent OnCommandExecuting;

    protected virtual void OnOnCommandExecuting()
    {
        CommandEvent handler = OnCommandExecuting;
        if (handler != null) handler();
    }

    protected abstract void Execute();
    public abstract bool CanExecute();

    public void Execute(TArgument parameter)
    {
        var arg = parameter;
        Parameter = arg;
        OnOnCommandExecuting();
        Execute();
        _executedSubject.OnNext(arg);
        OnOnCommandExecuted();
    }
    
    public bool CanExecute(TArgument parameter) 
    {
        Parameter = parameter;
        return CanExecute();
    }

    void ICommand.Execute(object parameter)
    {
        Execute((TArgument)parameter);
    }
    
    bool ICommand.CanExecute(object parameter)
    {
        return CanExecute((TArgument)parameter);
    }

    public IDisposable Subscribe(IObserver<TArgument> observer)
    {
        return _executedSubject.Subscribe(observer);
    }

    public TArgument Parameter { get; set; }

    object IParameterCommand.Parameter
    {
        get
        {
            return Parameter;
        }
        set
        {
            Parameter = (TArgument)value;
        }
    }
}

[Obsolete]
public class CommandWith<TArgument> : CommandBase<TArgument>
{
    public CommandWith()
    {
    }

    public CommandWith(Action<TArgument> argument)
    {
        
    } 
    protected override void Execute()
    {
        throw new NotImplementedException();
    }

    public override bool CanExecute()
    {
        throw new NotImplementedException();
    }
}
[Obsolete]
public class CommandWithSender<TSender> : CommandBase<TSender>
{
 
    protected Action<TSender> Delegate { get; set; }

    public CommandWithSender(Action<TSender> @delegate)
    {
        Delegate = @delegate;
    }

    public CommandWithSender(TSender sender, Action<TSender> @delegate)
    {
        this.Sender = sender;
        Delegate = @delegate;
    }

    public TSender Sender { get; set; }

    public override bool CanExecute()
    {
        return true;
    }

    protected override void Execute()
    {
        if (Delegate != null)
            Delegate(Sender);

    }
}
[Obsolete]
public class CommandWithSenderAndArgument<TSender,TArgument> : CommandBase<TArgument>
{

    public TSender Sender { get; set; }
    
    protected Action<TSender,TArgument> Delegate { get; set; }

    public CommandWithSenderAndArgument(Action<TSender,TArgument> @delegate)
    {
        Delegate = @delegate;
    }

    public CommandWithSenderAndArgument(TSender sender, Action<TSender, TArgument> @delegate)
    {
        Sender = sender;
        Delegate = @delegate;
    }


    protected override void Execute()
    {
        if (Delegate != null)
            Delegate(Sender, Parameter);
    }

    public override bool CanExecute()
    {
        return true;
    }
}

public static partial class ObservableExtensions
{
    [Obsolete]
    public static IDisposable Subscribe<T>(this CommandBase<T> source)
    {
        return source.Subscribe(Observer.Create<T>(Stubs.Ignore<T>, Stubs.Throw, Stubs.Nop));
    }
    [Obsolete]
    public static IDisposable Subscribe<T>(this CommandBase<T> source, Action<T> onNext)
    {
        return source.Subscribe(Observer.Create(onNext, Stubs.Throw, Stubs.Nop));
    }
    [Obsolete]
    public static IDisposable Subscribe<T>(this CommandBase<T> source, Action<T> onNext, Action<Exception> onError)
    {
        return source.Subscribe(Observer.Create(onNext, onError, Stubs.Nop));
    }
    [Obsolete]
    public static IDisposable Subscribe<T>(this CommandBase<T> source, Action<T> onNext, Action onCompleted)
    {
        return source.Subscribe(Observer.Create(onNext, Stubs.Throw, onCompleted));
    }
    [Obsolete]
    public static IDisposable Subscribe<T>(this CommandBase<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted)
    {
        return source.Subscribe(Observer.Create(onNext, onError, onCompleted));
    }
}

public class ViewModelCommand
{
    public ViewModel Sender { get; set; }

}