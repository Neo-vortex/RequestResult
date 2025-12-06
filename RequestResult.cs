using OneOf;

namespace RequestResult;

/// <summary>
/// This is the request result class.
/// </summary>
/// <typeparam name="T">Type of the result on successful execution</typeparam>
[GenerateOneOf]
public partial class RequestResult<T> : OneOfBase<T, Exception>
{
    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    public bool IsSuccessful => IsT0;

    /// <summary>
    /// Indicates whether the request has failed.
    /// </summary>
    public bool IsFailed => IsT1;

    /// <summary>
    /// Gets the error if the request has failed.
    /// </summary>
    public Exception? Error => IsFailed ? AsT1 : null;

    /// <summary>
    /// Gets the actual value of the request result.
    /// </summary>
    public new T Value
    {
        get
        {
            if (IsSuccessful) return AsT0;
            throw new InvalidOperationException("There is no value since the request has failed.");
        }
    }

    /// <summary>
    /// Throws the exception if the request result has failed.
    /// </summary>
    public void EnsureSuccess()
    {
        if (IsFailed) throw AsT1;
    }

    /// <summary>
    /// Tries to get the value if the request is successful.
    /// </summary>
    public bool TryGetValue(out T? value)
    {
        if (IsSuccessful)
        {
            value = AsT0;
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get the error if the request has failed.
    /// </summary>
    public bool TryGetError(out Exception? error)
    {
        if (IsFailed)
        {
            error = AsT1;
            return true;
        }
        error = null;
        return false;
    }

    // ============ STATIC FACTORY METHODS ============
    
    public static RequestResult<T> Success(T? value = default)
    {
        return new RequestResult<T>(value!);
    }

    public static RequestResult<T> SuccessEmpty()
    {
        return new RequestResult<T>(default(T)!);
    }

    public static RequestResult<T> Fail(string error)
    {
        return new RequestResult<T>(new Exception(error));
    }

    public static RequestResult<T> Fail(Exception error)
    {
        return new RequestResult<T>(error);
    }

    /// <summary>
    /// Creates a result from a nullable value. Fails if null.
    /// </summary>
    public static RequestResult<T> FromNullable(T? value, string errorMessage = "Value is null")
    {
        return value != null 
            ? Success(value) 
            : Fail(errorMessage);
    }

    /// <summary>
    /// Creates a result from a boolean condition.
    /// </summary>
    public static RequestResult<T> FromCondition(bool condition, T value, string errorMessage)
    {
        return condition 
            ? Success(value) 
            : Fail(errorMessage);
    }

    /// <summary>
    /// Creates a result from a boolean condition with factory.
    /// </summary>
    public static RequestResult<T> FromCondition(bool condition, Func<T> valueFactory, string errorMessage)
    {
        return condition 
            ? Success(valueFactory()) 
            : Fail(errorMessage);
    }


    /// <summary>
    /// Explicit conversion to boolean (true if successful).
    /// </summary>
    public static explicit operator bool(RequestResult<T> result)
    {
        return result.IsSuccessful;
    }

    // ============ OPERATORS ============

    /// <summary>
    /// True operator for use in if statements.
    /// </summary>
    public static bool operator true(RequestResult<T> result)
    {
        return result.IsSuccessful;
    }

    /// <summary>
    /// False operator for use in if statements.
    /// </summary>
    public static bool operator false(RequestResult<T> result)
    {
        return result.IsFailed;
    }

    /// <summary>
    /// Logical OR operator - returns first successful result or last failure.
    /// </summary>
    public static RequestResult<T> operator |(RequestResult<T> left, RequestResult<T> right)
    {
        return left.IsSuccessful ? left : right;
    }

    /// <summary>
    /// Logical AND operator - returns first failure or last success.
    /// </summary>
    public static RequestResult<T> operator &(RequestResult<T> left, RequestResult<T> right)
    {
        return left.IsFailed ? left : right;
    }

    /// <summary>
    /// Negation operator - inverts success/failure (value becomes exception message, exception becomes default value).
    /// </summary>
    public static RequestResult<T> operator !(RequestResult<T> result)
    {
        return result.IsSuccessful 
            ? Fail($"Negated success: {result.AsT0}") 
            : Success(default(T)!);
    }

    // ============ FUNCTIONAL METHODS ============

    /// <summary>
    /// Maps the successful value to a new type using the provided function.
    /// </summary>
    public RequestResult<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccessful 
            ? RequestResult<TNew>.Success(mapper(AsT0))
            : RequestResult<TNew>.Fail(AsT1);
    }

    /// <summary>
    /// Maps the successful value to a new RequestResult using the provided function (flatMap/bind).
    /// </summary>
    public RequestResult<TNew> Bind<TNew>(Func<T, RequestResult<TNew>> binder)
    {
        return IsSuccessful 
            ? binder(AsT0)
            : RequestResult<TNew>.Fail(AsT1);
    }

    /// <summary>
    /// Async version of Map.
    /// </summary>
    public async Task<RequestResult<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapper)
    {
        if (!IsSuccessful)
            return RequestResult<TNew>.Fail(AsT1);
        
        try
        {
            var result = await mapper(AsT0);
            return RequestResult<TNew>.Success(result);
        }
        catch (Exception ex)
        {
            return RequestResult<TNew>.Fail(ex);
        }
    }

    /// <summary>
    /// Async version of Bind.
    /// </summary>
    public async Task<RequestResult<TNew>> BindAsync<TNew>(Func<T, Task<RequestResult<TNew>>> binder)
    {
        return IsSuccessful 
            ? await binder(AsT0)
            : RequestResult<TNew>.Fail(AsT1);
    }

    /// <summary>
    /// Executes an action on the value if successful, returns the original result.
    /// </summary>
    public RequestResult<T> Tap(Action<T> action)
    {
        if (IsSuccessful)
            action(AsT0);
        return this;
    }

    /// <summary>
    /// Async version of Tap.
    /// </summary>
    public async Task<RequestResult<T>> TapAsync(Func<T, Task> action)
    {
        if (IsSuccessful)
            await action(AsT0);
        return this;
    }

    /// <summary>
    /// Executes an action on the error if failed, returns the original result.
    /// </summary>
    public RequestResult<T> TapError(Action<Exception> action)
    {
        if (IsFailed)
            action(AsT1);
        return this;
    }

    /// <summary>
    /// Async version of TapError.
    /// </summary>
    public async Task<RequestResult<T>> TapErrorAsync(Func<Exception, Task> action)
    {
        if (IsFailed)
            await action(AsT1);
        return this;
    }

    /// <summary>
    /// Executes an action on either success or failure, returns the original result.
    /// </summary>
    public RequestResult<T> TapBoth(Action<T> onSuccess, Action<Exception> onError)
    {
        if (IsSuccessful)
            onSuccess(AsT0);
        else
            onError(AsT1);
        return this;
    }

    /// <summary>
    /// Returns the value if successful, otherwise returns the provided default value.
    /// </summary>
    public T ValueOr(T defaultValue)
    {
        return IsSuccessful ? AsT0 : defaultValue;
    }

    /// <summary>
    /// Returns the value if successful, otherwise invokes the function to get a default value.
    /// </summary>
    public T ValueOr(Func<T> defaultValueFactory)
    {
        return IsSuccessful ? AsT0 : defaultValueFactory();
    }

    /// <summary>
    /// Returns the value if successful, otherwise invokes the function with the error to get a default value.
    /// </summary>
    public T ValueOr(Func<Exception, T> defaultValueFactory)
    {
        return IsSuccessful ? AsT0 : defaultValueFactory(AsT1);
    }

    /// <summary>
    /// Executes one of two functions depending on success or failure.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Exception, TResult> onFailure)
    {
        return IsSuccessful ? onSuccess(AsT0) : onFailure(AsT1);
    }

    /// <summary>
    /// Executes one of two actions depending on success or failure.
    /// </summary>
    public void Match(Action<T> onSuccess, Action<Exception> onFailure)
    {
        if (IsSuccessful)
            onSuccess(AsT0);
        else
            onFailure(AsT1);
    }

    /// <summary>
    /// Async version of Match with result.
    /// </summary>
    public async Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> onSuccess, 
        Func<Exception, Task<TResult>> onFailure)
    {
        return IsSuccessful 
            ? await onSuccess(AsT0) 
            : await onFailure(AsT1);
    }

    /// <summary>
    /// Filters the result based on a predicate. Converts success to failure if predicate fails.
    /// </summary>
    public RequestResult<T> Filter(Func<T, bool> predicate, string errorMessage = "Filter predicate failed")
    {
        if (!IsSuccessful)
            return this;
        
        return predicate(AsT0) 
            ? this 
            : Fail(errorMessage);
    }

    /// <summary>
    /// Async version of Filter.
    /// </summary>
    public async Task<RequestResult<T>> FilterAsync(Func<T, Task<bool>> predicate, string errorMessage = "Filter predicate failed")
    {
        if (!IsSuccessful)
            return this;
        
        var passes = await predicate(AsT0);
        return passes ? this : Fail(errorMessage);
    }

    /// <summary>
    /// Recovers from failure by providing a fallback value.
    /// </summary>
    public RequestResult<T> Recover(Func<Exception, T> recovery)
    {
        return IsFailed 
            ? Success(recovery(AsT1))
            : this;
    }

    /// <summary>
    /// Recovers from failure by providing a fallback RequestResult.
    /// </summary>
    public RequestResult<T> RecoverWith(Func<Exception, RequestResult<T>> recovery)
    {
        return IsFailed 
            ? recovery(AsT1)
            : this;
    }

    /// <summary>
    /// Async version of Recover.
    /// </summary>
    public async Task<RequestResult<T>> RecoverAsync(Func<Exception, Task<T>> recovery)
    {
        if (!IsFailed)
            return this;
        
        try
        {
            var result = await recovery(AsT1);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Fail(ex);
        }
    }

    /// <summary>
    /// Async version of RecoverWith.
    /// </summary>
    public async Task<RequestResult<T>> RecoverWithAsync(Func<Exception, Task<RequestResult<T>>> recovery)
    {
        return IsFailed 
            ? await recovery(AsT1)
            : this;
    }

    /// <summary>
    /// Maps the error to a different exception.
    /// </summary>
    public RequestResult<T> MapError(Func<Exception, Exception> errorMapper)
    {
        return IsFailed 
            ? Fail(errorMapper(AsT1))
            : this;
    }

    /// <summary>
    /// Retries the operation if it fails, up to maxAttempts times.
    /// </summary>
    public static async Task<RequestResult<T>> RetryAsync(
        Func<Task<RequestResult<T>>> operation,
        int maxAttempts = 3,
        TimeSpan? delayBetweenAttempts = null)
    {
        var delay = delayBetweenAttempts ?? TimeSpan.FromMilliseconds(100);
        RequestResult<T> result = default!;

        for (var i = 0; i < maxAttempts; i++)
        {
            result = await operation();
            if (result.IsSuccessful)
                return result;

            if (i < maxAttempts - 1)
                await Task.Delay(delay);
        }

        return result;
    }

    /// <summary>
    /// Executes the operation with a timeout.
    /// </summary>
    public static async Task<RequestResult<T>> WithTimeoutAsync(
        Func<Task<T>> operation,
        TimeSpan timeout)
    {
        try
        {
            var task = operation();
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                return Success(await task);
            }
            return Fail($"Operation timed out after {timeout.TotalSeconds} seconds");
        }
        catch (Exception ex)
        {
            return Fail(ex);
        }
    }

    /// <summary>
    /// Combines two results using a combiner function.
    /// </summary>
    public RequestResult<TResult> Zip<TOther, TResult>(
        RequestResult<TOther> other,
        Func<T, TOther, TResult> combiner)
    {
        if (IsFailed)
            return RequestResult<TResult>.Fail(AsT1);
        if (other.IsFailed)
            return RequestResult<TResult>.Fail(other.Error!);

        return RequestResult<TResult>.Success(combiner(AsT0, other.AsT0));
    }

    /// <summary>
    /// Async version of Zip.
    /// </summary>
    public async Task<RequestResult<TResult>> ZipAsync<TOther, TResult>(
        Task<RequestResult<TOther>> otherTask,
        Func<T, TOther, TResult> combiner)
    {
        if (IsFailed)
            return RequestResult<TResult>.Fail(AsT1);

        var other = await otherTask;
        if (other.IsFailed)
            return RequestResult<TResult>.Fail(other.Error!);

        return RequestResult<TResult>.Success(combiner(AsT0, other.AsT0));
    }

    // ============ CONVERSION METHODS ============

    /// <summary>
    /// Converts to a nullable value (null if failed).
    /// </summary>
    public T? ToNullable()
    {
        return IsSuccessful ? AsT0 : default;
    }

    /// <summary>
    /// Converts to a tuple of (value, error).
    /// </summary>
    public (T? Value, Exception? Error) ToTuple()
    {
        return IsSuccessful ? (AsT0, null) : (default, AsT1);
    }

    /// <summary>
    /// Converts to Option/Maybe pattern (null for failure).
    /// </summary>
    public T? ToOption()
    {
        return ToNullable();
    }

    /// <summary>
    /// Converts to Either pattern with explicit left/right.
    /// </summary>
    public Either<Exception, T> ToEither()
    {
        return IsSuccessful 
            ? Either<Exception, T>.Right(AsT0)
            : Either<Exception, T>.Left(AsT1);
    }

    /// <summary>
    /// Converts to ValueTuple for deconstruction.
    /// </summary>
    public void Deconstruct(out bool isSuccess, out T? value, out Exception? error)
    {
        isSuccess = IsSuccessful;
        value = IsSuccessful ? AsT0 : default;
        error = IsFailed ? AsT1 : null;
    }

    // ============ EXISTING ASYNC METHODS ============

    public async Task EnsureSuccessAsync()
    {
        await Task.Run(EnsureSuccess);
    }

    public async Task<T?> TryGetValueAsync()
    {
        return await Task.Run(() => TryGetValue(out var value) ? value : default);
    }

    public async Task<Exception?> TryGetErrorAsync()
    {
        return await Task.Run(() => TryGetError(out var error) ? error : null);
    }

    public override string ToString()
    {
        if (IsSuccessful)
        {
            var value = AsT0;
            return value == null ? "Success: (no value)" : $"Success: {value}";
        }
        return $"Error: {Error?.Message ?? "Unknown error"}";
    }

    /// <summary>
    /// Gets a detailed string representation including stack trace for errors.
    /// </summary>
    public string ToDetailedString()
    {
        if (!IsSuccessful) return $"Error: {Error?.Message ?? "Unknown error"}\nStack Trace: {Error?.StackTrace}";
        var value = AsT0;
        return value == null ? "Success: (no value)" : $"Success: {value}";
    }
}

// ============ EITHER TYPE FOR CONVERSION ============

public abstract class Either<TLeft, TRight>
{
    public static Either<TLeft, TRight> Left(TLeft value) => new LeftValue(value);
    public static Either<TLeft, TRight> Right(TRight value) => new RightValue(value);

    private class LeftValue : Either<TLeft, TRight>
    {
        public TLeft Value { get; }
        public LeftValue(TLeft value) => Value = value;
    }

    private class RightValue : Either<TLeft, TRight>
    {
        public TRight Value { get; }
        public RightValue(TRight value) => Value = value;
    }
}

// ============ EXTENSION METHODS ============

public static class RequestResultExtensions
{
    /// <summary>
    /// Combines multiple results into a single result containing a collection.
    /// Fails if any result fails.
    /// </summary>
    public static RequestResult<IEnumerable<T>> Combine<T>(this IEnumerable<RequestResult<T>> results)
    {
        var resultList = results.ToList();
        var failed = resultList.FirstOrDefault(r => r.IsFailed);
        
        return failed != null ? RequestResult<IEnumerable<T>>.Fail(failed.Error!) : RequestResult<IEnumerable<T>>.Success(resultList.Select(r => r.Value));
    }

    /// <summary>
    /// Combines multiple results, collecting all errors if any fail.
    /// </summary>
    public static RequestResult<IEnumerable<T>> CombineAll<T>(this IEnumerable<RequestResult<T>> results)
    {
        var resultList = results.ToList();
        var failures = resultList.Where(r => r.IsFailed).ToList();
        
        if (failures.Any())
        {
            var aggregateException = new AggregateException(
                "Multiple operations failed",
                failures.Select(f => f.Error!));
            return RequestResult<IEnumerable<T>>.Fail(aggregateException);
        }
        
        return RequestResult<IEnumerable<T>>.Success(resultList.Select(r => r.Value));
    }

    /// <summary>
    /// Partitions results into successes and failures.
    /// </summary>
    public static (IEnumerable<T> Successes, IEnumerable<Exception> Failures) Partition<T>(
        this IEnumerable<RequestResult<T>> results)
    {
        var resultList = results.ToList();
        var successes = resultList.Where(r => r.IsSuccessful).Select(r => r.Value);
        var failures = resultList.Where(r => r.IsFailed).Select(r => r.Error!);
        return (successes, failures);
    }

    /// <summary>
    /// Traverses a collection and applies an async function that returns RequestResult.
    /// </summary>
    public static async Task<RequestResult<IEnumerable<TResult>>> TraverseAsync<T, TResult>(
        this IEnumerable<T> source,
        Func<T, Task<RequestResult<TResult>>> selector)
    {
        var results = new List<TResult>();
        
        foreach (var item in source)
        {
            var result = await selector(item);
            if (result.IsFailed)
                return RequestResult<IEnumerable<TResult>>.Fail(result.Error!);
            
            results.Add(result.Value);
        }
        
        return RequestResult<IEnumerable<TResult>>.Success(results);
    }

    /// <summary>
    /// Applies a selector to each element and collects all successful results.
    /// </summary>
    public static async Task<RequestResult<IEnumerable<TResult>>> TraverseAllAsync<T, TResult>(
        this IEnumerable<T> source,
        Func<T, Task<RequestResult<TResult>>> selector)
    {
        var tasks = source.Select(selector);
        var results = await Task.WhenAll(tasks);
        return results.CombineAll();
    }

    /// <summary>
    /// Wraps a try-catch around a function and returns a RequestResult.
    /// </summary>
    public static RequestResult<T> Try<T>(Func<T> func)
    {
        try
        {
            return RequestResult<T>.Success(func());
        }
        catch (Exception ex)
        {
            return RequestResult<T>.Fail(ex);
        }
    }

    /// <summary>
    /// Async version of Try.
    /// </summary>
    public static async Task<RequestResult<T>> TryAsync<T>(Func<Task<T>> func)
    {
        try
        {
            var result = await func();
            return RequestResult<T>.Success(result);
        }
        catch (Exception ex)
        {
            return RequestResult<T>.Fail(ex);
        }
    }

    /// <summary>
    /// Wraps an action in try-catch and returns a unit result.
    /// </summary>
    public static RequestResult<Unit> Try(Action action)
    {
        try
        {
            action();
            return RequestResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return RequestResult<Unit>.Fail(ex);
        }
    }

    /// <summary>
    /// Async version of Try for actions.
    /// </summary>
    public static async Task<RequestResult<Unit>> TryAsync(Func<Task> action)
    {
        try
        {
            await action();
            return RequestResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return RequestResult<Unit>.Fail(ex);
        }
    }

    /// <summary>
    /// Converts Task&lt;T&gt; to Task&lt;RequestResult&lt;T&gt;&gt; with exception handling.
    /// </summary>
    public static async Task<RequestResult<T>> ToRequestResultAsync<T>(this Task<T> task)
    {
        try
        {
            var result = await task;
            return RequestResult<T>.Success(result);
        }
        catch (Exception ex)
        {
            return RequestResult<T>.Fail(ex);
        }
    }

    /// <summary>
    /// Flattens nested RequestResult.
    /// </summary>
    public static RequestResult<T> Flatten<T>(this RequestResult<RequestResult<T>> nested)
    {
        return nested.IsSuccessful 
            ? nested.Value 
            : RequestResult<T>.Fail(nested.Error!);
    }

    /// <summary>
    /// Sequences results by executing them in order until one fails.
    /// </summary>
    public static async Task<RequestResult<T>> SequenceAsync<T>(
        this IEnumerable<Func<Task<RequestResult<T>>>> operations)
    {
        RequestResult<T> lastResult = default!;
        
        foreach (var operation in operations)
        {
            lastResult = await operation();
            if (lastResult.IsFailed)
                return lastResult;
        }
        
        return lastResult;
    }
}

// ============ UNIT TYPE FOR VOID OPERATIONS ============

/// <summary>
/// Represents a void/unit type for operations that don't return a value.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    public static readonly Unit Value = new Unit();
    
    public bool Equals(Unit other) => true;
    public override bool Equals(object? obj) => obj is Unit;
    public override int GetHashCode() => 0;
    public override string ToString() => "()";
    
    public static bool operator ==(Unit left, Unit right) => true;
    public static bool operator !=(Unit left, Unit right) => false;
}