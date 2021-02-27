# ExampleMod
An example Valheim mod made with [InSlimVML](https://github.com/PJninja/InSlimVML)

## Usage
1. Grab the project files and open the project using the IDE of your choice
2. Edit the project references depending on your local paths
3. Build and test the mod to make sure it loads
4. Edit the project to create your own **amazing** mod
## Notes
- Depends on .NET Framework 4.8, if you don't have it installed either upgrade your installation or downgrade the project
- If you're feeling adventurous, you can use something like the following PostBuildEvent in your .csproj to copy your built dll when you run a build:
  ```
  <PropertyGroup>
    <PostBuildEvent>xcopy "$(OutputPath)$(MSBuildProjectName).dll" "(ABSOLUTE\PATH\TO)\Valheim\InSlimVML\Mods\" /syd</PostBuildEvent>
  </PropertyGroup>
  ```
  Make sure that Valheim is closed before you build if you want the file copied.