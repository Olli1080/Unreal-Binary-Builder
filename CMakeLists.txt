cmake_minimum_required(VERSION 3.10)

# We only enable C# if we are using a Visual Studio generator on Windows.
# This allows generating a native .sln while still supporting other platforms via fallback.
if(MSVC_IDE)
    project(UnrealBinaryBuilder 
        VERSION 4.0.0
        DESCRIPTION "A helper application designed to create binary versions of Unreal Engine from source and package plugins."
        LANGUAGES CSharp
    )
    set(USE_NATIVE_CSHARP TRUE)
else()
    project(UnrealBinaryBuilder 
        VERSION 4.0.0
        DESCRIPTION "A helper application designed to create binary versions of Unreal Engine from source and package plugins."
        LANGUAGES NONE
    )
    set(USE_NATIVE_CSHARP FALSE)
    message(STATUS "Non-Visual Studio generator detected. Falling back to dotnet CLI wrapper.")
endif()

set(PROJECT_AUTHORS "Olli1080")
set(PROJECT_COMPANY "Olli1080")
set(PROJECT_COPYRIGHT "Copyright (c) 2023-2026 Olli1080")

# Set .NET Target Framework
set(DOTNET_TARGET_FRAMEWORK "net10.0")

# Find the dotnet executable
find_program(DOTNET_EXE NAMES dotnet)
if(NOT DOTNET_EXE)
    message(FATAL_ERROR "dotnet SDK not found. Please install .NET 10.0 SDK.")
endif()

# Global restore target
add_custom_target(Restore ALL
    COMMAND ${DOTNET_EXE} restore ${CMAKE_SOURCE_DIR}/UnrealBinaryBuilder.sln
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
    COMMENT "Restoring NuGet packages..."
)

# Include subdirectories
add_subdirectory(UnrealBinaryBuilder.Avalonia)
add_subdirectory(UnrealBinaryBuilder.Tests)
