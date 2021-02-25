# DevTools
### What is it?
Tools. For devs. If you're not a person making mods or know what this is, you probably shouldn't be using it.
### How to use it?
1. Install it (InSlimVML required)
2. Use the console command `csrun` to run any block of C# code
### How does it work?
DevTools wraps your input in a class and method, compiles it, then uses reflection to invoke that method.  
This means you will need to use `Console.instance.Print();` or something similar in your code to see any output.
### TODO
- User defined usings (currently a static list, should cover most cases though)
-- To get around this, use the fully qualified type name