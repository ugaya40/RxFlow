Simple Flow Control Library with Rx(Reactive Extensions).

##Supported Enviroment

* .NET Framework 4.5
* Windows 8
* Windows Phone Silverlight 8
* Xamarin.Android
* Xamarin.iOS

##Simple Usage
####Junction
Switch sequence.Switched value not flowed backward.
```csharp
static void Sample1()
{
	var branchA = Branch.CreateBranch<int>(input =>
        input.Do(i => Console.WriteLine("branchA :" + i))
        .Subscribe());

    var branchB = Branch.CreateBranch<int>(input =>
        input.Do(i => Console.WriteLine("branchB :" + i))
        .Subscribe());

    Observable.Range(1, 10)
        .Junction(i => i % 2 == 0, branchA)
        .Junction(i => i % 3 == 0, branchB)
        .Subscribe();
}

/*output
* branchA :2
* branchB :3
* branchA :4
* branchA :6 // Not processed in branchB
* branchA :8
* branchB :9
* branchA :10
*/
```
####Distribution
Distribute sequence.Distributed value flowed backward.
```csharp
static void Sample2()
{
    var branchA = Branch.CreateBranch<int>(input =>
        input.Do(i => Console.WriteLine("branchA :" + i))
        .Subscribe());

    var branchB = Branch.CreateBranch<int>(input =>
        input.Do(i => Console.WriteLine("branchB :" + i))
        .Subscribe());

    Observable.Range(1, 10)
        .Distribution(i => i % 2 == 0, branchA)
        .Distribution(i => i % 3 == 0, branchB)
        .Subscribe();
}

/*output
* branchA :2
* branchB :3
* branchA :4
* branchA :6 //processed in branchA
* branchB :6 //processed in branchB
* branchA :8
* branchB :9
* branchA :10
*/
```
####Many to Many
Many main sequences to many branches.
```csharp
static void Sample3()
{
    var branchA = Branch.CreateBranch<int>(input =>
        input.Do(i => Console.WriteLine("branchA :" + i))
        .Subscribe());

    var branchB = Branch.CreateBranch<int>(input =>
        input.Do(i => Console.WriteLine("branchB :" + i))
        .Subscribe());

    Observable.Range(1, 5)
        .Distribution(i => i % 2 == 0, branchA)
        .Distribution(i => i % 3 == 0, branchB)
        .Subscribe();

    Observable.Range(6, 5)
        .Distribution(i => i % 2 == 0, branchA)
        .Distribution(i => i % 3 == 0, branchB)
        .Subscribe();
}

/*output
* branchA :2
* branchB :3
* branchA :4
* branchA :6
* branchB :6
* branchA :8
* branchB :9
* branchA :10
*/
```
####Branch to Branch
```csharp
static void Sample4()
{
    var branchA = Branch.CreateBranch<int>(input =>
        input.Do(i => Console.WriteLine("branchA :" + i))
        .Subscribe());

    var branchB = Branch.CreateBranch<int>(input =>
        input.Do(i => Console.WriteLine("branchB :" + i))
        .Junction(i => i % 2 == 0, branchA)
        .Subscribe());

    Observable.Range(1, 10)
        .Junction(i => i % 3 == 0, branchB)
        .Subscribe();
}

/*output
branchB :3
branchB :6
branchA :6 //branchB to branchA
branchB :9
*/
```
##License
MIT License
