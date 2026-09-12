using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using Object = Il2CppSystem.Object;

namespace MiraAPI.Utilities;

/// <summary>
/// A wrapper for state machine objects to access their parent instance and state.
/// </summary>
/// <typeparam name="T">The type of the parent class that owns the state machine.</typeparam>
public class StateMachineWrapper<T> where T : Il2CppObjectBase
{
    private readonly Il2CppObjectBase _stateMachine;

    // normally it is fields, but IL2CPP turns them into properties
    private readonly PropertyInfo _thisProperty;
    private readonly PropertyInfo _stateProperty;
    private readonly PropertyInfo _currentProperty;
    private readonly Dictionary<string, PropertyInfo> _propertyCache;

    private T? _parentInstance;

    /// <summary>
    /// Gets the instance of the <typeparamref name="T"/>.
    /// </summary>
    public T Instance => _parentInstance ??= (T)_thisProperty.GetValue(_stateMachine)!;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateMachineWrapper{T}"/> class.
    /// </summary>
    /// <param name="stateMachine">The state machine instance to wrap.</param>
    public StateMachineWrapper(Il2CppObjectBase stateMachine)
    {
        _stateMachine = stateMachine;

        var type = _stateMachine.GetType();
        _thisProperty = AccessTools.Property(type, "__4__this");
        _stateProperty = AccessTools.Property(type, "__1__state");
        _currentProperty = AccessTools.Property(type, "__2__current");

        if (_thisProperty == null || _stateProperty == null || _currentProperty == null)
        {
            throw new MissingMemberException($"Could not find required properties in type '{type}'.");
        }

        _propertyCache = [];
    }

    /// <summary>
    /// Gets the current state of the state machine.
    /// </summary>
    /// <returns>The current state as an integer.</returns>
    public int GetState()
    {
        return (int)_stateProperty.GetValue(_stateMachine)!;
    }

    /// <summary>
    /// Sets the current state of the state machine.
    /// </summary>
    /// <param name="newState">The new state to use.</param>
    public void SetState(int newState)
    {
        _stateProperty.SetValue(_stateMachine, newState);
    }

    /// <summary>
    /// Sets the newest yield return of the state machine.
    /// </summary>
    /// <param name="newReturn">The new return to use.</param>
    public void SetRecentReturn(Object newReturn)
    {
        _currentProperty.SetValue(_stateMachine, newReturn);
    }

    /// <summary>
    /// Gets a parameter of type <typeparamref name="TField"/> from the state machine by its name.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to retrieve.</param>
    /// <typeparam name="TField">The type of the parameter to retrieve.</typeparam>
    /// <returns>The value of the specified parameter.</returns>
    /// <exception cref="MissingFieldException">Thrown if the specified parameter does not exist.</exception>
    public TField GetParameter<TField>(string parameterName)
    {
        if (_propertyCache.TryGetValue(parameterName, out var property))
        {
            return (TField)property.GetValue(_stateMachine)!;
        }

        property = AccessTools.Property(_stateMachine.GetType(), parameterName)
                   ?? throw new MissingFieldException($"Could not find parameter '{parameterName}' in state machine of type '{_stateMachine.GetType()}'.");

        _propertyCache[parameterName] = property;
        return (TField)property.GetValue(_stateMachine)!;
    }
}
