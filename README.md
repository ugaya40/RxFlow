Simple Flow Control Library with Rx(Reactive Extensions).

##Supported Enviroment

* .NET Framework 4.5
* Windows 8
* Windows Phone Silverlight 8
* Xamarin.Android
* Xamarin.iOS

##SimpleUsage
```csharp
//Box<T> is ValueTypeWrapper in RxFlow
private static void Main(string[] args)
{
    var context = new FlowContext();

    var branchA = context.CreateBranch<Box<int>>(source =>
        source.Select(i => i + " As String")
            .Do(i => Console.WriteLine("Branch A: " + i))
        );

    var branchB = context.CreateBranch<Box<int>>(source =>
        source.Select(i => TimeSpan.FromSeconds(i.Value).ToBox())
            .Do(i => Console.WriteLine("Branch B: " + i.Value.ToString()))
        );

    //MainFlow
    context.CreateSequence(Enumerable.Range(1, 10).ToObservable().ToBox(), i =>
        i.Junction(e => e.Value%2 == 0, branchA) // to branchA
            .Junction(e => e.Value%3 == 0, branchB) // to branchB
            .Do(e => Console.WriteLine("Flow: " + e.Value))
        );

    context.Start();
    Console.Read();
}

    /*output
     * Flow: 1
     * Branch A: 2 As String
     * Branch B: 00:00:03
     * Branch A: 4 As String
     * Flow: 5
     * Branch A: 6 As String
     * Flow: 7
     * Branch A: 8 As String
     * Branch B: 00:00:09
     * Branch A: 10 As String
    */
```
Flow to Branch (1 to Many and Many to Many) Supported.
##License
MIT License
