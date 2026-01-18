# JNatPack
### Presentation
JNatPack is a tool written in C# which lets you pack a `.jar` file in a native executable for Windows/MacOS(x86_64&arm64)/Linux.\
Even tho it uses c#, the end user **does not need** .NET installed on it's computer, as it generates a self contained executable. (thus containing .NET)

### How to use
As the usage says:
```
Usage:
  JNatPack [jarfile.jar] [jre.zip] [output directory] [output name] [for mac only: is arm64? (if yes, put any value)]"
```
To be clearer: 
* **1st argument**: Your Jar file
* **2nd argument**: Your jre. You should use jlink to generate a tailor made jre, otherwise the final executable may be heavy
* **3rd argument**: The output directory. It should be created beforehand
* **4th argument**: The output name. It is the name of the final executable file. You may still rename it later
* **5th argument**: (MacOS only) Is compiling for arm64. Allows you to build for MacOS arm64. **ONLY DO IT IF YOUR MAC HAS AN ARM CHIP, IT WILL CRASH OTHERWISE**.

### Additional information
This project is in a proof of conecpt stage, so pull requests and issue reporting would be more than greatly appreciated.\
Hope this tool finds you well!!
