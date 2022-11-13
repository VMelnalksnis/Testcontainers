# NuGet Package Template

Repository template for a .NET Standard NuGet package

### Steps after creating a repository from template

1. Update `RepositoryUrl` in [Directory.Build.props](./Directory.Build.props)
2. Rename solution/projects
3. Fix solution name in [test.yml](.github/workflows/test.yml)
4. Fix project name(s) in [release.yml](.github/workflows/release.yml)
5. Add NUGET_KEY secret
6. Sign assemblies (see project properties in Rider/Visual Studio)
