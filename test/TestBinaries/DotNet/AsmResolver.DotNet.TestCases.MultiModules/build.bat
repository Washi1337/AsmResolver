IF EXIST bin (
    rm -r bin
) 

mkdir bin

csc Secondary/MyModel.cs /target:module /out:bin/MyModel.netmodule
csc ManifestModule/Manifest.cs /addmodule:bin/MyModel.netmodule /out:bin/Manifest.exe  