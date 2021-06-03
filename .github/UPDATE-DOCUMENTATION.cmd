@echo off
cd ..
doxygen
git add docs/\*
git commit -m "Update Documentation"
pause
