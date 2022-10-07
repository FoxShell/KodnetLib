# KodnetLib

Library in C# that make easier Reflection, dynamic invoke of methods, get/set properties. Also offers a better integration as COM+ server (or COM Hosting), specially for VisualFoxPro.

Reflection is slow, CallSites or CompiledExpressions are complicated, **KodnetLib** allows dynamic call methods, get and set properties, on instance and static types, fast and easy. Internally use a combination of 
Reflection, CallSites and CompiledExpressions, giving the best of all methods.

- Faster than *Reflection*
- Fast and Easy like *CallSites* or *CompiledExpressions*


Some examples:

## TypeDescriptors basic example:

```c#

// load default assembiles 
FoxShell.Kodnet kodnet = new FoxShell.Kodnet();

Type sbType = kodnet.GetTypeFromString("System.Text.StringBuilder");
TypeDescriptor td =  TypeDescriptor .Get(sbType);

// create a new StringBuilder object
object sb = td.noninstance["construct"].Invoke(null, new object[]{});

// or exactly the same:
sb = td.Constructor.Invoke(new object[]{});


// call instance methods
td.instance["Append"].Invoke(sb, new object[]{ "A" });
td.instance["Append"].Invoke(sb, new object[]{ "B" });
td.instance["Append"].Invoke(sb, new object[]{ "C" });
td.instance["Append"].Invoke(sb, new object[]{ "D" });

string str = (string)td.instance["ToString"].Invoke(sb, new object[]{  });

// getting properties
int length = (int)td.instance["Length"].Invoke(sb, new object[]{ });

// setting properties
td.instance[".set.Length"].Invoke(sb, new object[]{ 100 });

``` 

## TypeDescriptors for create generic objects:


```c#

FoxShell.Kodnet kodnet = new FoxShell.Kodnet();

// create generic instances (harder with reflection, easy with kodnetlib)
Type ListStringType = kodnet.GetTypeFromString("System.Collections.Generic.List<System.String>");
TypeDescriptor td =  TypeDescriptor .Get(ListStringType);

object list = td.Constructor.Invoke(new object[]{});

td.instance["Add"].Invoke(list, new object[] { "A" } );
td.instance["Add"].Invoke(list, new object[] { "E" } );
td.instance["Add"].Invoke(list, new object[] { "I" } );
td.instance["Add"].Invoke(list, new object[] { "O" } );
td.instance["Add"].Invoke(list, new object[] { "U" } );

string[] arr = (string[]) td.instance["ToArray"].Invoke(list, new object[]{});


Type DictType = kodnet.GetTypeFromString("System.Collections.Generic.Dictionary<System.String, System.Object>");
td =  TypeDescriptor .Get(DictType);

object dict = td.Constructor.Invoke(new object[]{});

// property indexers set
td.instance[".set.Item"].Invoke(dict, new object[]{ "name", "James" });
td.instance[".set.Item"].Invoke(dict, new object[]{ "age", 28 });

// this is equivalent to:

System.Collections.Generic.Dictionary<System.String, System.Object> dictx = (System.Collections.Generic.Dictionary<System.String, System.Object>)dict;

dictx["name"] = "James";
dictx["age"] = 28;


// property indexer get

td.instance["Item"].Invoke(dict, new object[]{ "name" }); // James
td.instance["Item"].Invoke(dict, new object[]{ "age" }); // 28

td.instance["Item"].Invoke(dict, new object[]{ "xxx" }); // Throw error, key not exists


```


## **CallSiteInvokers** for fast and easy **dynamic**  programming. 


Consider this example: 

```c# 

public int Process(object com /* com is a COM+ object with unknown members*/, int n1, int n2){

    // dynamic call sum method on object
    dynamic obj = com;
    return (int)obj.Sum(1,2);

}
```

this is good, you can do the same with CallSiteInvokers


```c# 
public int Process(object com /* com is a object with unknown members*/, int n1, int n2){

    return (int)FoxShell.CallSiteInvoker.GetInvokerparam2("Sum").Invoke(com, n1, n2);

}
```

Very good. What about save a cache of CallSiteInvoker


```c# 

public FoxShell.invokerparam2 SumInvoker = FoxShell.CallSiteInvoker.GetInvokerparam2("Sum");

public int Process(object com /* com is a object with unknown members*/, int n1, int n2){

    return (int)SumInvoker.Invoke(com, n1, n2);

}
```

Perfect. What about if you only know what method you need call at runtime?
**dynamic** keyword is not enough for that, but **CallSiteInvoker** can:

```c# 
public int Process(object com /* com is a object with unknown members*/, string methodToInvoke, int n1, int n2){

    return (int)FoxShell.CallSiteInvoker.GetInvokerparam2(methodToInvoke).Invoke(com, n1, n2);

}
```

But, harder, what about if you don't know how many parameters receives the unknown method?


```c# 
public object Process(object com /* com is a object with unknown members*/, string methodToInvoke, params object[] args){

    return FoxShell.CallSiteInvoker.invokeMethod(com, methodToInvoke, args);

}
```

The best part of this, is that **CallSiteInvokers** are automatically cached, giving more performance in some cases than use **dynamic** keywords.

