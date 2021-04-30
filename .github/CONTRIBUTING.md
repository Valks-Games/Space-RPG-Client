## Setup
Current project Unity version `2020.2.1f1`, remind me to update this if I ever change the project version.

## Coding Style
### Methods
- Methods should follow PascalFormat
- Try to keep accessor (getter) and mutator (setter) methods at bottom of classes. (This is more of like a guide, you do not have to follow this rule by the book)
- Add `private` to MonoBehavior methods
- If using `{}` always expand them
### Properties
- Use a `_` in front of all properties in methods and constructors.
### Comments
Start comments with `/*!` so Doxygen can recognize them.
```cs
/*!
 * This is a function.
 * 
 * @return Returns this.
 */
```

## Creating a Pull Request
1. Always test the application to see if it works as intended with no additional bugs you may be adding!
2. State all the changes you made in the PR, not everyone will understand what you've done!

## Generating Documentation
Make sure [Doxygen](https://www.doxygen.nl/index.html) is installed and added to your environment path.

Run `.github/UPDATE-DOCUMENTATION.cmd` after all your initial commits then push.

## Documentation
Documentation can be found [here](https://valks-games.github.io/valks-game/html/index.html).
