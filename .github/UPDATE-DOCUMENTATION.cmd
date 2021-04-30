# TODO: Give feedback to user if they have not installed Doxygen / have not put Doxygen in their environment path.
# TODO: Only git add docs/\* when there are actually changes. Otherwise give feedback to the user that no changes needed to be made.
# TODO: Add cls and give better feedback to the user.
@echo off
cd ..
doxygen
git add docs/\*
git commit -m "Update Documentation"
echo.
echo.
echo.
echo.
echo.
echo.
echo Attempted to stage commit with updated documentation. See above for results.
echo.
echo.
echo.
echo.
echo.
echo.
pause
