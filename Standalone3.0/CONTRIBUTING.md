# Steamworks.NET.AnyCPU Contributing Guidelines

## Table of Contents
- [Introduction](#introduction)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Development Workflow](#development-workflow)
- [Code Generation](#code-generation)
- [Testing](#testing)
- [Pull Request Guidelines](#pull-request-guidelines)
- [Community](#community)

## Introduction

### Project Overview
Steamworks.NET.AnyCPU is a fork of Steamworks.NET that provides AnyCPU support, allowing the library to work seamlessly across different processor architectures (x86, x64, ARM, etc.).

### Relations between `Akarinnnnn/Steamworks.NET.AnyCPU` and `rlabrecque/Steamworks.NET`

Steamworks.NET.AnyCPU originated as a patch for Steamworks.NET. However, due to its extensive modifications to `CodeGen`, the resulting commit volume became too large, far exceeding that of the branch that upgraded the target SDK from 1.62 to 1.63. Therefore, in the short term, AnyCPU will remain as an independent fork.

**Main Contributors:**
- @Akarinnnnn Any CPU rewrite
- @Chicken-Bones ~~Vibe code the guy above~~AnyCPU idea initiator, DLL resolve hook and conditional marshal prototype provider  
- @ryan-linehan CI/CD in NuGet packaging

**Credits**
- @rlabrecque 

## Getting Started

### Prerequisites
- **Python 3.13** (required for CodeGen)
- **.NET SDK** (version specified in project files)
- **Git**
- **Visual Studio 2022** or **Visual Studio Code** (using VSC for Python, VS for C# is recommended)
- **Rider** and **PyCharm** (if you prefer JetBrains tools, but currently `.gitignore` isn't ready for them)

### Setting Up Development Environment
1. Clone the repository:
	```bash
	git clone https://github.com/Akarinnnnn/Steamworks.NET.AnyCPU.git
	cd Steamworks.NET.AnyCPU
	```

2. Verify Python installation:
	```bash
	python --version
	# Should output: Python 3.13.x
	# or python3 if your python command is bound to 2.x
	```

3. Restore .NET dependencies:
	```bash
	dotnet restore Standalone3.0/Steamworks.NET.sln
	```

4. Build the project:
	```bash
	dotnet build Standalone3.0/Steamworks.NET.sln
	```

5. Create your `Steamworks.NET.AnyCPU` package
	```bash
	dotnet pack --version $SEMVER Standalone3.0/Steamworks.NET.sln
	```

## Project Structure

### 1. Solution View of `Steamworks.NET`
- **`src/`**: Shared source between Steamworks.NET.AnyCPU and Unity Package Manager. Linked from `$REPOSITORY_ROOT/com.rlabrecque.steamworks.net/Runtime`
  - **Important**: Any edits without `#if STEAMWORKS_ANYCPU` should be carefully considered as they will affect Unity, which targeting to `netstandard2.0` or `netstandard2.1` depends on Unity version.
  - `src/autogen`: First type of generated source files. Generator is located at `$REPOSITORY_ROOT/CodeGen`, @Akarinnnnn added AnyCPU modifications.
  - `src/types`: Second type of generated source files. Generator is located at `$REPOSITORY_ROOT/CodeGen`, mostly done by original author @rlabrecque. This type is mostly from templates.
  
- **`native/`**: Steamworks native dependencies for supported platforms. Linked from `$REPOSITORY_ROOT/com.rlabrecque.steamworks.net/Plugin`
  - **Important**: These are provided by Valve, bundled with the current version, and should not be edited.
  - If you need to update them for upgrading target SDK version, ask the whole Steamworks.NET community.

- **`anycpu/`**: The AnyCPU specific source code. This is where you should make your edits for the AnyCPU version of Steamworks.NET.
  - The sub-folder structure is mirrored with `src/`.
  - **Important**: Files prefixed with `*.g.cs` are **generated** and should not be edited directly. If you need to edit them, edit the Generator in `$REPOSITORY_ROOT/CodeGen` and regenerate the code.

### 2. `$REPOSITORY_ROOT/CodeGen`
Python project, C# binding generator(Binder). This repository uses Python 3.13 to run the code. Previously contained the submodule `CodeGen/SteamworksParser/`, later integrated into this repository via `git subtree`.

- **`templates/`**: Stores templates for generated files, including generated file headers, `Steamworks.NET/src/types`, and partial `*.g.cs` files.
- **`SteamworksParser/`**: Parser that scans Steamworks SDK C++ header files line by line, scanning types, fields, interface member functions, and type aliases. @Akarinnnnn added type size analysis, field size analysis, object layout analysis, and structure cross-platform compatibility analysis features on top of the original. Also includes some debugging scripts.
- **`Steam/`**: Stores Steamworks SDK C++ header files for the corresponding version, used for binding generation.
- **`src/`**: Binding generator.

## Development Workflow

### Branch Strategy
- Stable release branch doesn't exist currently, production-ready code is hold in tags.
- **`anycpu-2`**: Development branch for upcoming features
- **Feature branches**: `feature/description` (e.g., `feature/add-new-interface`)
- **Bug fix branches**: `fix/issue-description` (e.g., `fix/memory-leak-issue123`)
- **Hotfix branches**: `hotfix/critical-issue` (for urgent fixes to production)

### Commit Guidelines
- Use descriptive commit messages in the present tense
- Reference issue numbers when applicable (e.g., `Fixes #123`)

### Coding Standards
- Enable `.editorconfig` support to your editor, most coding standards are defined there
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise (single responsibility principle)

## Code Generation

### When to Regenerate Code
You should regenerate code when:
1. Steamworks SDK headers are updated
2. Templates are modified
3. Binder logic is changed
4. New types need to be added
5. Fixing issues in generated code

### Regeneration Process
1. Navigate to `CodeGen/` directory:
   ```bash
   cd CodeGen
   ```

2. Run the generator script:
   ```bash
   python Steamworks.NET_CodeGen.py
   ```

3. Verify generated files:
   - Check for compilation errors
   - Verify type mappings are correct
   - Ensure no breaking changes

4. Test the changes:
   In another terminal do:
   ```bash
   cd Standalone3.0
   dotnet build
   ```

### Common Issues and Solutions
- **Missing Python modules**: Run `pip install -r requirements.txt` (if available)
- **Parser errors**: Navigate to corresponding parse logic in `SteamworksParser.py` and fix it
- **Other binder errors**: Navigate to corresponding bind logic in `src/<binder-kind>.py` and fix it
- **Generation failures**: Review template files for syntax errors

## Testing

### Testing Requirements
- All changes should be tested before submission
- Verify generated code compiles correctly
- Test with sample applications when possible
- Ensure cross-platform compatibility (x86, x64, ARM) if possible
- Run existing unit tests (if available)

### Testing Strategy
1. **Compilation Testing**: Ensure the project builds without errors
2. **Runtime Testing**: Test basic Steamworks functionality, `SteamAPI.Init()` and `SteamAPI.Shutdown()`
3. **Cross-Platform Testing**: Verify on different architectures, do your best
4. **Integration Testing**: Test with Unity projects (if applicable)

### Creating Tests
When adding new features or fixing bugs:
1. When creating tests, add documentation describing the expected behavior
2. Include setup instructions for the test environment

## Pull Request Guidelines

### Before Submitting a PR
1. Ensure your code follows the project's coding standards, use `dotnet format` or format hotkey first
2. Run any existing tests
3. Update documentation if needed
4. Rebase onto the latest target branch
5. Squash commits when appropriate
6. Ensure commit messages are clear and descriptive

### PR Description Template
```markdown
## Description
Brief description of the changes. Explain what problem this solves or what feature this adds.

## Related Issues
Fixes #issue_number
Closes #issue_number
Related to #issue_number

## Changes Made
- List specific changes made
- Include technical details if relevant
- Mention any breaking changes

## Testing
Describe how you tested the changes:
- [ ] Compilation passes
- [ ] Basic functionality works
- [ ] (Optional, if possible) Cross-platform compatibility verified
- [ ] No regressions in existing features

## Checklist
- [ ] Code follows project standards
- [ ] Tests pass (or N/A if no tests exist)
- [ ] Documentation updated
- [ ] No breaking changes (or clearly documented if breaking)
- [ ] PR targets the correct branch
```

### PR Review Process
1. **Initial Review**: Maintainers are busy to fight for life, but your PR will be reviewed sometime
2. **Feedback**: Address any feedback or requested changes
3. **Approval**: Once approved, the PR will be merged
4. **Merge Strategy**: Plain Merge for history clear

## Community

### Communication Channels
- **GitHub Issues**: For bug reports and feature requests
- **GitHub Discussions**: For questions and general discussions
- **Pull Requests**: For code contributions
- **Discord Chat**: We don't have that yet

### How to Report Issues
When reporting issues, please include:
1. Clear description of the problem
2. Steps to reproduce
3. Expected vs actual behavior
4. Environment details (OS, CPU architecture, .NET version, Unity version if applicable)
5. Relevant code snippets or error messages

### How to Request Features
When requesting features:
1. Describe the use case
2. Explain the benefits
3. Provide examples of how it would be used
4. Consider if it aligns with project goals

### Code of Conduct
Please be respectful and constructive in all interactions. We follow these principles:
- Be welcoming and inclusive
- Be respectful of different viewpoints
- Focus on what is best for the community
- Show empathy towards other community members

Violations of the code of conduct should be reported to the maintainers.

### Recognition
Contributors will be:
- Acknowledged in release notes for significant contributions
- Given credit in relevant documentation

---

## Additional Resources

### Documentation
- [Steamworks Documentation](https://partner.steamgames.com/doc/sdk)
- [Original Steamworks.NET Documentation](https://steamworks.github.io/)

### Useful Commands
```bash
# Build the project
dotnet build Standalone3.0/Steamworks.NET.sln

# Clean build artifacts
dotnet clean Standalone3.0/Steamworks.NET.sln

# Run code generator
cd CodeGen
python Steamworks.NET_CodeGen.py

# Check for compilation errors
dotnet build --no-restore --verbosity minimal

# Create NuGet package yourself
cd Standalone3.0
dotnet pack --version $SEMVER -o path/to/your/test/packages/registry
```

### Troubleshooting
- **Build failures**: Check .NET SDK version compatibility
- **Missing dependencies**: `dotnet restore` and `python -m pip install`
- **Python errors**: Verify Python 3.13 is installed correctly

---

*Thank you for contributing to Steamworks.NET.AnyCPU! Your contributions help make this project better for everyone.*  
Heart from Cyberstan, no AI was hurt during the writing process🤖.