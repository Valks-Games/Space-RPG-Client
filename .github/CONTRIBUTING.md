## Setup
Current project Unity version `2020.2.1f1`, remind me to update this if I ever change the project version.

## Coding Style
### Methods
- Methods should follow PascalFormat
- Always fully expand `{}`
### Properties
- Use a `_` in front of all parameters in methods and constructors.
### Comments
Use XML for Doxygen
```cs
/// <summary>
/// Hello world
/// </summary>
```

## Creating a Pull Request
1. Always test the application to see if it works as intended with no additional bugs you may be adding!
2. State all the changes you made in the PR, not everyone will understand what you've done!

## Generating Documentation
Make sure [Doxygen](https://www.doxygen.nl/index.html) is installed and added to your environment path.

Run `.github/UPDATE-DOCUMENTATION.cmd` after all your initial commits then push.

## Documentation
Documentation can be found [here](https://valks-games.github.io/valks-game/html/index.html).
