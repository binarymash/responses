# Responses

## Introduction

This library provides a fluent API for building standardized response objects that, optionally, contain payloads and errors.

Built in .NET Core and targetting [.NET Standard 1.1](https://github.com/dotnet/standard/blob/master/docs/versions.md), the library can be used in .NET Framework >= 4.5, or cross-platform in .NET Core >= 1.0.

### Example

Lets assume our existing code defines an `Address` object...
```
class Address
{
	public Name { get; set; }
	public Street { get; set; }
	public City { get; set; }
	// etc...
}

var address = new Address(...) // initialise values
```

...we can use `BuildResponse` to build a `Response` that contains an `Address` instance, and set an `Error` on the response...

```
var response = BuildResponse
    .WithPayload(address)
    .AndWithErrors(new Error("InvalidName", "The name must be set"))
    .Create();
```
...and then the response serialized to JSON is...
```
{
	"Payload":
    {
		"Name":"",
        "Street":"Buckingham Palace",
        "City":"London"
    },
    "Errors":[
    	{ "Code":"InvalidName", "Message":"The name must be set" }
    ]
}
```

## Installing

Stable builds are published to https://www.myget.org/F/binarymash-stable/api/v3/index.json. Ensure you have this as a Nuget package source, then install via Nuget in the usual way

```
Install-Package BinaryMash.Responses
```


## How to use

You'll find everything you need in the `Binarymash.Responses` namespace.

```
using BinaryMash.Responses;
```

### Response types
There are two types of Response object. Choose which type to use based on what you want your response to contain.

`BinaryMash.Responses.Response<T>` is a response that contains a strongly typed payload. We will use this when we want to return a value from a method:

```
// synchronous example
Response<Address> GetAddress(uint id); // replaces Address GetAddress(uint id)

// asynchronous example
async Task<Response<Address>> GetAddress(uint id); // replaces async Task<Address> GetAddress(unit id)
```

`BinaryMash.Responses.Response` is a response that contains no payload. We will use this when we don't care about a return value:

```
// synchronous example
Response SetAddress(); // replaces void SetAddress(Address address)

// asynchronous example
async Task<Response> SetAddress(Address address); // replaces async Task SetAddress(Address address)
```


Now, lets look at how to create these responses.

### Empty response

The simplest example creates an empty response:

```
var response = BuildResponse
    .WithNoPayload()
    .Create();
```

...serialzed into JSON, this response will look like...

```
{
	"Errors":[]
}
```

Notice that the empty response contains an empty collection of errors. Lets learn how to add some errors to a response.

### Empty response with errors

The following code lets us add errors to the empty response.

```
var response = BuildResponse
    .WithNoPayload()
    // errors can be added individually...
    .AndWithErrors(new Error("InvalidName", "The name must be set"))
    // ...and as a collection...
    .AndWithErrors(new []
    {
        new Error("InvalidStreet", "The street must be set"),
        new Error("InvalidCity", "The city must be set")
    })
    .Create();
```

Serialized, this looks like...

```
{
	"Errors":[
    	{ "Code":"InvalidName", "Message":"The name must be set" },
        { "Code":"InvalidStreet", "Message":"The street must be set" },
        { "Code":"InvalidCity", "Message":"The city must be set" }
     ]
}
```

Great. We can return an response with errors. But how do we return a payload value?

### Payload response

Lets assume we have an address object...
```
class Address
{
	public Name { get; set; }
	public Street { get; set; }
	public City { get; set; }
	// etc...
}
```

This code builds a response that contains an instance of `Address`.

```
Address address;

// some code here to set value of address (not shown)

var response = BuildResponse
    .WithPayload<Address>(address)
    .Create();
```
In most cases the compiler will infer the type of the payload from the value you supply, so we can omit the type definition from the code.

```
	// ...
    .WithPayload(address)
	// ...
```
The serialized response is...
```
{
	"Payload":
    {
		"Name":"The Queen of England",
        "Street":"Buckingham Palace",
        "City":"London"
    },
    "Errors":[]
}
```

### Payload response with errors

Of course, we can add errors to the payload response in just the same way we did on the empty response:

```
var response = BuildResponse
    .WithPayload(address)
    .AndWithErrors(new Error("InvalidName", "The name must be set"))
    .Create();
```

### Payload response with default value

If we are setting errors on a payload response, we might decide it is not meaningful to also set a value in the payload. However, will lkely be constrained at compile time to return a `Response<T>` rather than a `Response`.

In this case, rather than having to always explicitly specify a value for the payload, we can choose to implicitly return the default value of our payload type. We can do this by using the parameterless version of `.WithPayload<T>()`...

```
var response = BuildResponse
    .WithPayload<Address>()
    .AndWithErrors(new Error("InvalidName", "The name must be set"))
    .Create();
```

This is exactly the same as explicitly set the default value:

```
    // ...
    .WithPayload<Address>(default(Address))
    // ...
```

In most cases it will be the same as setting the payload to null:

```
	// ...
    .WithPayload((Address)null)
    // ...
```
The serialized output will look something like this
```
{
	"Payload":null,
    "Errors":[
    	{"Code":"InvalidName","Message":"The name must be set"}
    ]
}
```


## Build Status

The repository is built on [AppVeyor](https://ci.appveyor.com/project/binarymash/responses).

### Stable

[![Build status](https://ci.appveyor.com/api/projects/status/p51dvusrw32u9b17/branch/master?svg=true)](https://ci.appveyor.com/project/binarymash/responses/branch/master)

Stable builds are published to https://www.myget.org/F/binarymash-stable/api/v3/index.json

### Development


[![Build status](https://ci.appveyor.com/api/projects/status/p51dvusrw32u9b17/branch/develop?svg=true)](https://ci.appveyor.com/project/binarymash/responses/branch/develop)

Development builds are published to https://www.myget.org/F/binarymash-unstable/api/v3/index.json
