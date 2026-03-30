cmake_minimum_required(VERSION 3.10)

project(UnrealBinaryBuilder 
    VERSION 4.0.0
    DESCRIPTION "A helper application designed to create binary versions of Unreal Engine from source and package plugins."
    LANGUAGES CSharp
)

set(PROJECT_AUTHORS "Olli1080")
set(PROJECT_COMPANY "Olli1080")
set(PROJECT_COPYRIGHT "Copyright (c) 2023-2026 Olli1080")

# Set .NET Target Framework
set(DOTNET_TARGET_FRAMEWORK "net10.0")

# Use the latest C# version
set(CMAKE_CSharp_FLAGS "/langversion:latest")

# Find the dotnet executable
find_program(DOTNET_EXE NAMES dotnet)

if(DOTNET_EXE)
    # Add a global restore target
    add_custom_target(Restore ALL
        COMMAND ${DOTNET_EXE} restore ${CMAKE_SOURCE_DIR}/UnrealBinaryBuilder.sln
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
        COMMENT "Restoring NuGet packages..."
    )
endif()

# Include subdirectories
add_subdirectory(UnrealBinaryBuilder.Avalonia)
add_subdirectory(UnrealBinaryBuilder.Tests)

# Ensure Restore runs before any subproject build
add_dependencies(UnrealBinaryBuilder.Avalonia Restore)
add_dependencies(UnrealBinaryBuilder.Tests Restore)
