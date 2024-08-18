# Mira API

A thorough Among Us modding API that covers:
- Roles
- Options
- Gamemodes
- HUD ("Cooldown buttons")
  
Mira API strives to be simple and easy to use, while also using as many base game elements as possible. The result is a less intrusive, better modding API that covers general use cases.

# Usage

## Roles
Roles are very simple in Mira API. There are 3 things you need to do to create a custom role:
1. Create a class that inherits from a base game role (like `RoleBehaviour`, `CrewmateRole`, `ImpostorRole`, etc).
2. Implement the `ICustomRole` interface from Mira API.
3. Add the `[RegisterCustomRole]` attribute to the class.

See [this file](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/CustomRole.cs) for a code example.

## Options


## Disclaimer

> This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.
