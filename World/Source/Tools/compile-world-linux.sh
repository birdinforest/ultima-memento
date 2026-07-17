# Exclude System/obj/** — msbuild leaves AssemblyAttributes.cs there, and -recurse would
# pick them up and fail with CS0579 (TargetFrameworkAttribute applied multiple times).
mcs -optimize+ -unsafe -t:exe -out:WorldLinux.exe -win32icon:../System/icon.ico -nowarn:219,414 -d:NEWTIMERS -d:NEWPARENT -d:MONO $(find ../System -name '*.cs' -not -path '*/obj/*') -main:Server.Core
mv ./WorldLinux.exe ../../
