using System;
using System.Linq;

namespace MiraAPI.GameModes;

public static class GameModeID
{
    public static uint Get<T>() where T : CustomGameMode
    {
        if (!CustomGameModeManager.GameModes.Values.OfType<T>().Any())
        {
            throw new InvalidOperationException($"Game mode {typeof(T).Name} is not registered!");
        }
        
        return (ushort)CustomGameModeManager.GameModes.First(x => x.Value is T).Key;
    }
    
}