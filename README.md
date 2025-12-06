# RequestResult\<T>

A powerful, functional, and type-safe result type for C# that eliminates exception-based error handling and enables railway-oriented programming.

## 📋 Table of Contents

- [Installation](#installation)
- [Overview](#overview)
- [Core Features](#core-features)
- [Factory Methods](#factory-methods)
- [Operators](#operators)
- [Functional Programming Methods](#functional-programming-methods)
- [Error Handling & Recovery](#error-handling--recovery)
- [Async Operations](#async-operations)
- [Collection Operations](#collection-operations)
- [Conversion Methods](#conversion-methods)
- [Advanced Features](#advanced-features)
- [Usage Examples](#usage-examples)
- [Best Practices](#best-practices)

## 🚀 Installation

```bash
# Install OneOf package (required dependency)
dotnet add package OneOf
```

Add the `RequestResult.cs` file to your project.

## 📖 Overview

`RequestResult<T>` is a discriminated union type that represents either a successful result with a value of type `T`, or a failure with an `Exception`. This pattern eliminates the need for try-catch blocks and makes error handling explicit and composable.

```csharp
// Traditional approach
try
{
    var user = GetUser(id);
    var orders = GetOrders(user.Id);
    return ProcessOrders(orders);
}
catch (Exception ex)
{
    // Handle error
}

// With RequestResult
return GetUser(id)
    .Bind(user => GetOrders(user.Id))
    .Map(ProcessOrders);
```

## 🎯 Core Features

### Basic Properties

```csharp
var result = RequestResult<int>.Success(42);

bool isSuccess = result.IsSuccessful;  // true
bool isFailed = result.IsFailed;        // false
Exception? error = result.Error;        // null
int value = result.Value;               // 42 (throws if failed)
```

### TryGet Methods

```csharp
// Try to get value
if (result.TryGetValue(out var value))
{
    Console.WriteLine($"Success: {value}");
}

// Try to get error
if (result.TryGetError(out var error))
{
    Console.WriteLine($"Error: {error.Message}");
}
```

### EnsureSuccess

```csharp
// Throws exception if result is failed
result.EnsureSuccess();

// Async version
await result.EnsureSuccessAsync();
```

## 🏭 Factory Methods

### Success

```csharp
// Create successful result
var result = RequestResult<string>.Success("Hello");

// With nullable value
var result = RequestResult<string>.Success(null);

// Empty success (for void operations)
var result = RequestResult<object>.SuccessEmpty();
```

### Fail

```csharp
// Create failed result with message
var result = RequestResult<int>.Fail("Something went wrong");

// Create failed result with exception
var result = RequestResult<int>.Fail(new InvalidOperationException("Error"));
```

### FromNullable

```csharp
string? nullableValue = GetNullableValue();
var result = RequestResult<string>.FromNullable(
    nullableValue, 
    "Value cannot be null"
);
```

### FromCondition

```csharp
// With direct value
var result = RequestResult<int>.FromCondition(
    age >= 18,
    age,
    "Must be 18 or older"
);

// With factory function
var result = RequestResult<User>.FromCondition(
    IsValid(data),
    () => CreateUser(data),
    "Invalid user data"
);
```

## ⚡ Operators

### Implicit Conversions

```csharp
// Automatic conversion from value to successful result
RequestResult<int> result = 42;

// Automatic conversion from exception to failed result
RequestResult<int> result = new Exception("Error");
```

### Explicit Conversions

```csharp
// Convert to value (throws if failed)
int value = (int)result;

// Convert to nullable
int? nullable = (int?)result;

// Convert to boolean
bool isSuccess = (bool)result;
```

### Logical Operators

```csharp
// OR - returns first successful result
var result = TryPrimary() | TrySecondary() | TryFallback();

// AND - returns first failure or last success
var result = ValidateStep1() & ValidateStep2() & ValidateStep3();

// Negation - inverts success/failure
var inverted = !result;
```

### Conditional Operators

```csharp
// Use directly in if statements
if (result)
{
    // Executes if successful
    Console.WriteLine("Success!");
}
```

## 🔄 Functional Programming Methods

### Map

Transforms the value if successful, propagates errors.

```csharp
RequestResult<int> number = RequestResult<int>.Success(5);
RequestResult<string> text = number.Map(n => $"Number: {n}");
// Result: Success("Number: 5")

RequestResult<string> textAsync = await number.MapAsync(async n => 
{
    await Task.Delay(100);
    return $"Number: {n}";
});
```

### Bind (FlatMap)

Chains operations that return `RequestResult`.

```csharp
RequestResult<User> user = GetUser(id);
RequestResult<Order[]> orders = user.Bind(u => GetOrders(u.Id));

// Async version
RequestResult<Order[]> ordersAsync = await user.BindAsync(async u => 
    await GetOrdersAsync(u.Id)
);
```

### Filter

Validates the value with a predicate.

```csharp
var result = RequestResult<int>.Success(15)
    .Filter(n => n > 10, "Number must be greater than 10");
// Result: Success(15)

var failed = RequestResult<int>.Success(5)
    .Filter(n => n > 10, "Number must be greater than 10");
// Result: Fail("Number must be greater than 10")

// Async version
var resultAsync = await value.FilterAsync(
    async v => await ValidateAsync(v),
    "Validation failed"
);
```

### Tap

Performs side effects without changing the result.

```csharp
var result = GetUser(id)
    .Tap(user => Console.WriteLine($"Found user: {user.Name}"))
    .Tap(user => LogAccess(user.Id))
    .Map(user => user.Email);

// Async version
var resultAsync = await GetUserAsync(id)
    .TapAsync(async user => await LogUserAccessAsync(user));
```

### TapError

Performs side effects on errors.

```csharp
var result = GetUser(id)
    .TapError(ex => Console.WriteLine($"Error: {ex.Message}"))
    .TapError(ex => LogError(ex));

// Async version
await result.TapErrorAsync(async ex => await LogErrorAsync(ex));
```

### TapBoth

Performs side effects on both success and failure.

```csharp
var result = GetUser(id)
    .TapBoth(
        user => Console.WriteLine($"Success: {user.Name}"),
        error => Console.WriteLine($"Error: {error.Message}")
    );
```

### Match

Pattern matching for both success and failure cases.

```csharp
// With return value
string message = result.Match(
    onSuccess: value => $"Success: {value}",
    onFailure: error => $"Error: {error.Message}"
);

// Without return value (Action)
result.Match(
    onSuccess: value => Console.WriteLine($"Got: {value}"),
    onFailure: error => LogError(error)
);

// Async version
var message = await result.MatchAsync(
    onSuccess: async value => await FormatAsync(value),
    onFailure: async error => await FormatErrorAsync(error)
);
```

### ValueOr

Gets value or provides a default.

```csharp
// With direct default value
int value = result.ValueOr(0);

// With factory function
int value = result.ValueOr(() => GetDefaultValue());

// With error-based default
int value = result.ValueOr(error => 
{
    LogError(error);
    return -1;
});
```

## 🛡️ Error Handling & Recovery

### Recover

Recovers from failure by providing a fallback value.

```csharp
var result = GetUser(id)
    .Recover(error => GetDefaultUser());

// Async version
var result = await GetUserAsync(id)
    .RecoverAsync(async error => await GetDefaultUserAsync());
```

### RecoverWith

Recovers with a fallback `RequestResult`.

```csharp
var result = GetFromCache(key)
    .RecoverWith(error => GetFromDatabase(key))
    .RecoverWith(error => GetFromBackup(key));

// Async version
var result = await GetFromCacheAsync(key)
    .RecoverWithAsync(async error => await GetFromDatabaseAsync(key));
```

### MapError

Transforms exceptions.

```csharp
var result = GetData()
    .MapError(ex => new CustomException("Failed to get data", ex));
```

## ⏱️ Async Operations

### RetryAsync

Automatically retries failed operations.

```csharp
var result = await RequestResult<Data>.RetryAsync(
    operation: async () => await FetchDataAsync(),
    maxAttempts: 5,
    delayBetweenAttempts: TimeSpan.FromSeconds(1)
);
```

### WithTimeoutAsync

Executes operation with timeout.

```csharp
var result = await RequestResult<Data>.WithTimeoutAsync(
    operation: async () => await SlowOperationAsync(),
    timeout: TimeSpan.FromSeconds(30)
);
```

### Zip / ZipAsync

Combines two results.

```csharp
var userResult = GetUser(userId);
var ordersResult = GetOrders(userId);

var combined = userResult.Zip(
    ordersResult,
    (user, orders) => new { User = user, Orders = orders }
);

// Async version
var combined = await userResult.ZipAsync(
    GetOrdersAsync(userId),
    (user, orders) => new { User = user, Orders = orders }
);
```

## 📦 Collection Operations

### Combine

Combines multiple results into one. Fails if any result fails.

```csharp
var results = new[]
{
    RequestResult<int>.Success(1),
    RequestResult<int>.Success(2),
    RequestResult<int>.Success(3)
};

RequestResult<IEnumerable<int>> combined = results.Combine();
// Result: Success([1, 2, 3])
```

### CombineAll

Combines all results, collecting all errors if any fail.

```csharp
var results = new[]
{
    RequestResult<int>.Success(1),
    RequestResult<int>.Fail("Error 1"),
    RequestResult<int>.Fail("Error 2")
};

var combined = results.CombineAll();
// Result: Fail(AggregateException with both errors)
```

### Partition

Splits results into successes and failures.

```csharp
var results = new[]
{
    RequestResult<int>.Success(1),
    RequestResult<int>.Fail("Error"),
    RequestResult<int>.Success(2)
};

var (successes, failures) = results.Partition();
// successes: [1, 2]
// failures: [Exception("Error")]
```

### TraverseAsync

Applies async function to collection, stops on first failure.

```csharp
var userIds = new[] { 1, 2, 3, 4, 5 };

var result = await userIds.TraverseAsync(async id => 
    await GetUserAsync(id)
);
// Result: RequestResult<IEnumerable<User>>
```

### TraverseAllAsync

Processes all items in parallel, collects all errors.

```csharp
var userIds = new[] { 1, 2, 3, 4, 5 };

var result = await userIds.TraverseAllAsync(async id => 
    await GetUserAsync(id)
);
```

### SequenceAsync

Executes operations in sequence until one fails.

```csharp
var operations = new Func<Task<RequestResult<Data>>>[]
{
    async () => await Step1Async(),
    async () => await Step2Async(),
    async () => await Step3Async()
};

var result = await operations.SequenceAsync();
```

## 🔄 Conversion Methods

### ToNullable / ToOption

```csharp
int? nullable = result.ToNullable();
// Returns value if successful, null if failed
```

### ToTuple

```csharp
var (value, error) = result.ToTuple();
```

### ToEither

```csharp
Either<Exception, int> either = result.ToEither();
```

### Deconstruction

```csharp
var (isSuccess, value, error) = result;

if (isSuccess)
{
    Console.WriteLine($"Value: {value}");
}
```

### ToString Methods

```csharp
string simple = result.ToString();
// "Success: 42" or "Error: Something went wrong"

string detailed = result.ToDetailedString();
// Includes stack trace for errors
```

## 🔧 Advanced Features

### Try / TryAsync

Wraps operations in try-catch automatically.

```csharp
// Synchronous
var result = RequestResultExtensions.Try(() => 
{
    return RiskyOperation();
});

// Async
var result = await RequestResultExtensions.TryAsync(async () => 
{
    return await RiskyOperationAsync();
});

// For void operations (returns RequestResult<Unit>)
var result = RequestResultExtensions.Try(() => 
{
    PerformAction();
});

var result = await RequestResultExtensions.TryAsync(async () => 
{
    await PerformActionAsync();
});
```

### ToRequestResultAsync

Converts Task to RequestResult with exception handling.

```csharp
Task<User> userTask = GetUserAsync(id);
RequestResult<User> result = await userTask.ToRequestResultAsync();
```

### Flatten

Unwraps nested RequestResults.

```csharp
RequestResult<RequestResult<int>> nested = GetNestedResult();
RequestResult<int> flattened = nested.Flatten();
```

## 💡 Usage Examples

### Basic Error Handling

```csharp
public RequestResult<User> GetUser(int id)
{
    if (id <= 0)
        return RequestResult<User>.Fail("Invalid ID");
    
    var user = _repository.Find(id);
    return RequestResult<User>.FromNullable(user, "User not found");
}
```

### Chaining Operations

```csharp
public async Task<RequestResult<OrderSummary>> ProcessOrder(int orderId)
{
    return await GetOrder(orderId)
        .Bind(order => ValidateOrder(order))
        .BindAsync(async order => await ChargePayment(order))
        .MapAsync(async order => await CreateSummary(order))
        .TapAsync(async summary => await SendConfirmationEmail(summary))
        .RecoverAsync(async error => await HandleError(error));
}
```

### Multiple Validations

```csharp
public RequestResult<User> ValidateUser(UserInput input)
{
    return RequestResult<User>.Success(new User(input))
        .Filter(u => !string.IsNullOrEmpty(u.Email), "Email is required")
        .Filter(u => u.Age >= 18, "Must be 18 or older")
        .Filter(u => IsValidEmail(u.Email), "Invalid email format");
}
```

### Parallel Processing

```csharp
public async Task<RequestResult<Dashboard>> GetDashboard(int userId)
{
    var userTask = GetUserAsync(userId);
    var ordersTask = GetOrdersAsync(userId);
    var settingsTask = GetSettingsAsync(userId);
    
    var userResult = await userTask.ToRequestResultAsync();
    
    return await userResult.ZipAsync(
        ordersTask.ToRequestResultAsync(),
        (user, orders) => new { user, orders }
    ).ZipAsync(
        settingsTask.ToRequestResultAsync(),
        (data, settings) => new Dashboard
        {
            User = data.user,
            Orders = data.orders,
            Settings = settings
        }
    );
}
```

### Retry with Fallback

```csharp
public async Task<RequestResult<Data>> GetData(string key)
{
    return await RequestResult<Data>.RetryAsync(
        async () => await GetFromPrimarySource(key),
        maxAttempts: 3,
        delayBetweenAttempts: TimeSpan.FromSeconds(2)
    )
    .RecoverWithAsync(async error => 
        await GetFromBackupSource(key)
    )
    .RecoverAsync(async error => 
        await GetDefaultData()
    );
}
```

### Collection Processing

```csharp
public async Task<RequestResult<Report>> GenerateReport(int[] userIds)
{
    var results = await userIds.TraverseAsync(async id => 
        await GetUserData(id)
    );
    
    return results.Map(userData => CreateReport(userData));
}
```

### Match Pattern

```csharp
public async Task<IActionResult> GetUserEndpoint(int id)
{
    var result = await GetUser(id);
    
    return result.Match(
        onSuccess: user => Ok(user),
        onFailure: error => error switch
        {
            NotFoundException => NotFound(error.Message),
            UnauthorizedException => Unauthorized(),
            _ => StatusCode(500, error.Message)
        }
    );
}
```

### Using Operators

```csharp
// Try multiple sources
var data = GetFromCache(key) 
    | GetFromDatabase(key) 
    | GetDefaultData();

// Validate multiple conditions
var result = CheckAge() 
    & CheckEmail() 
    & CheckPhone();

// Simple conditional
RequestResult<int> result = GetValue();
if (result)
{
    ProcessValue(result.Value);
}
```

## ✅ Best Practices

### 1. Always Handle Both Cases

```csharp
// ❌ Bad - ignores errors
var value = result.Value;

// ✅ Good - handles both cases
result.Match(
    onSuccess: value => ProcessValue(value),
    onFailure: error => LogError(error)
);
```

### 2. Use Bind for Chaining

```csharp
// ❌ Bad - nested results
var result = GetUser(id);
if (result.IsSuccessful)
{
    var orders = GetOrders(result.Value.Id);
    if (orders.IsSuccessful)
    {
        return orders;
    }
}

// ✅ Good - flat chain
return GetUser(id)
    .Bind(user => GetOrders(user.Id));
```

### 3. Use Tap for Side Effects

```csharp
// ❌ Bad - breaks the chain
var result = GetUser(id);
if (result.IsSuccessful)
{
    LogAccess(result.Value);
}
return result.Map(user => user.Email);

// ✅ Good - preserves chain
return GetUser(id)
    .Tap(user => LogAccess(user))
    .Map(user => user.Email);
```

### 4. Prefer Railway-Oriented Programming

```csharp
// ✅ Good - clear flow
return ValidateInput(request)
    .Bind(data => SaveToDatabase(data))
    .MapAsync(async entity => await EnrichData(entity))
    .Tap(result => PublishEvent(result))
    .Map(entity => MapToDto(entity));
```

### 5. Use Try for External Calls

```csharp
// ✅ Good - automatic exception handling
var result = await RequestResultExtensions.TryAsync(async () => 
    await externalApi.GetDataAsync()
);
```

## 🎓 Unit Type

For operations that don't return a value, use `Unit`:

```csharp
public RequestResult<Unit> DeleteUser(int id)
{
    _repository.Delete(id);
    return RequestResult<Unit>.Success(Unit.Value);
}

// Or use extension method
var result = RequestResultExtensions.Try(() => 
{
    PerformAction();
});
```

## 📄 License

MIT

## 🤝 Contributing

Contributions welcome! Please feel free to submit a Pull Request.

---

**Built with ❤️ using OneOf library**
